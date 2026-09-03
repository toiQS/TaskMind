// RejectSkillLevelUpRequestCommand.cs
// [CẬP NHẬT - fix] Ghi SkillHistoryEntry (mục 4.3.3) — đề xuất bị từ chối vẫn phải lưu lại đầy đủ vào
// lịch sử kỹ năng, không chỉ các thay đổi đã áp dụng thành công. Trước đây SkillHistoryEntry hoàn
// toàn không được ghi ở bất kỳ luồng nào dù entity/DbSet đã sẵn sàng.
//
// [CẬP NHẬT - fix #2] Kiểm tra Result trả về từ profile.ApplyPenaltyDowngrade(). Trước đây nếu
// profile != null nhưng KHÔNG có UserSkillRecord đúng SkillId (dữ liệu bất thường), penalty âm thầm
// không được áp dụng (Result.Failure bị bỏ qua), không phát sinh SkillPenaltyAppliedEvent (nên user
// KHÔNG nhận được Notification cảnh báo nào), nhưng SkillHistoryEntry vẫn ghi levelAfter dựa trên
// Records rỗng — dữ liệu lịch sử sai lệch. Giờ kiểm tra rõ kết quả và phản ánh đúng thực tế.
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Commons.Result;
using TaskMind.Domain.Entities;
using TaskMind.Domain.Enums;

namespace TaskMind.Applications.Admins.Features.Skills
{
    public class RejectSkillLevelUpRequestCommand : IRequest<ServiceResult>
    {
        public Guid RequestId { get; }
        public Guid ApproverAdminId { get; }
        public string Reason { get; }

        public RejectSkillLevelUpRequestCommand(Guid requestId, Guid approverAdminId, string reason)
        {
            RequestId = requestId;
            ApproverAdminId = approverAdminId;
            Reason = reason;
        }
    }

    public class RejectSkillLevelUpRequestHandler : IRequestHandler<RejectSkillLevelUpRequestCommand, ServiceResult>
    {
        private readonly IApplicationDbContext _dbContext;

        public RejectSkillLevelUpRequestHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ServiceResult> Handle(RejectSkillLevelUpRequestCommand command, CancellationToken cancellationToken)
        {
            var request = await _dbContext.SkillLevelUpRequests
                .FirstOrDefaultAsync(r => r.Id == command.RequestId, cancellationToken);

            if (request == null)
                return ServiceResult.NotFound("Không tìm thấy yêu cầu nâng cấp độ kỹ năng.");

            var rejectResult = request.Reject(command.Reason);
            if (!rejectResult.IsSuccess)
                return ServiceResult.Failure(rejectResult.Message);

            var profile = await _dbContext.SkillProfiles
                .FirstOrDefaultAsync(p => p.UserId == request.UserId, cancellationToken);

            SkillLevel? levelAfter = null;
            Result? penaltyResult = null;
            if (profile != null)
            {
                penaltyResult = profile.ApplyPenaltyDowngrade(request.SkillId);
                if (penaltyResult.IsSuccess)
                    levelAfter = profile.Records.FirstOrDefault(r => r.SkillId == request.SkillId)?.Level;
            }

            var penaltyApplied = penaltyResult?.IsSuccess ?? false;

            // [MỚI - fix] Nếu profile tồn tại nhưng penalty áp dụng thất bại (thiếu UserSkillRecord —
            // dữ liệu bất thường), không có SkillPenaltyAppliedEvent nào được phát sinh nên user sẽ
            // không nhận cảnh báo hạ cấp — cần một Notification riêng để tránh im lặng.
            if (profile != null && !penaltyApplied)
            {
                var failureNotif = Notification.Create(
                    request.UserId,
                    "Yêu cầu nâng cấp độ kỹ năng bị từ chối",
                    $"Yêu cầu nâng cấp kỹ năng đã bị từ chối. Lý do: {command.Reason} " +
                    "(Hệ thống gặp lỗi khi áp dụng mức phạt hạ cấp do không tìm thấy kỹ năng tương ứng trong hồ sơ — vui lòng liên hệ hỗ trợ.)",
                    NotificationType.Warning);

                if (failureNotif.IsSuccess)
                    _dbContext.Notifications.Add(failureNotif.Data!);
            }
            // Trường hợp penaltyApplied == true: SkillPenaltyAppliedEvent (raise trong
            // ApplyPenaltyDowngrade) sẽ tự lo Notification cảnh báo qua SkillPenaltyAppliedEventHandler.
            // Trường hợp profile == null: chưa từng khai báo kỹ năng nào — không có gì để phạt, giữ
            // nguyên hành vi cũ (không gửi thêm thông báo phạt).

            var auditResult = AuditLog.Record(command.ApproverAdminId, "SkillLevelUpRejected", nameof(SkillLevelUpRequest), request.Id);
            if (auditResult.IsSuccess)
                _dbContext.AuditLogs.Add(auditResult.Data!);

            // [MỚI - fix, mục 4.3.3] Ghi nhận lịch sử — bao gồm cả đề xuất bị từ chối. Nếu penalty áp
            // dụng thất bại, ghi rõ trong lý do từ chối để không đánh lừa dữ liệu lịch sử.
            var responsibleAccountId = request.ApproverAccountId != Guid.Empty ? request.ApproverAccountId : command.ApproverAdminId;
            var rejectionReasonForHistory = string.IsNullOrWhiteSpace(command.Reason) ? "Không đạt yêu cầu xác minh." : command.Reason;
            if (profile != null && !penaltyApplied)
                rejectionReasonForHistory += $" [Lỗi kỹ thuật khi áp dụng phạt hạ cấp: {penaltyResult?.Message}]";

            var historyResult = SkillHistoryEntry.Record(
                userId: request.UserId,
                skillId: request.SkillId,
                changeSource: SkillChangeSource.UserInitiated,
                responsibleAccountId: responsibleAccountId,
                outcome: SkillHistoryOutcome.Rejected,
                levelBefore: request.CurrentLevel,
                levelAfter: levelAfter,
                relatedSubmissionId: request.SubmissionId,
                relatedRequestId: request.Id,
                rejectionReason: rejectionReasonForHistory);

            if (historyResult.IsSuccess)
                _dbContext.SkillHistoryEntries.Add(historyResult.Data!);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return ServiceResult.Success("Từ chối yêu cầu nâng cấp độ kỹ năng thành công");
        }
    }
}