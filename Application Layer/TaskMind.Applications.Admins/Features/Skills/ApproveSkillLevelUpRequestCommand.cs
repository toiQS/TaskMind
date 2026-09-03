// ApproveSkillLevelUpRequestCommand.cs
// [CẬP NHẬT - fix]
//  1) request.Approve() không còn tự phát sinh SkillLevelApprovedEvent (xem SkillLevelUpRequest.cs) —
//     trước đây cả nó lẫn profile.ApplyLevelUp() cùng raise một event, khiến
//     SkillLevelApprovedEventHandler chạy 2 lần / gửi trùng Notification + email cho cùng một lần duyệt.
//  2) Thêm fallback Notification cho trường hợp hiếm SkillProfile == null (không tìm thấy hồ sơ kỹ
//     năng) — nếu không, sau khi bỏ event ở (1), user sẽ không nhận được bất kỳ thông báo nào.
//  3) Ghi SkillHistoryEntry (mục 4.3.3) — trước đây hoàn toàn không được ghi ở luồng này dù entity/
//     DbSet đã sẵn sàng.
//  4) [MỚI - fix] Kiểm tra Result trả về từ profile.ApplyLevelUp(). Trước đây khi profile != null
//     nhưng KHÔNG có UserSkillRecord đúng SkillId (lệch dữ liệu), ApplyLevelUp() trả về Failure và
//     KHÔNG phát sinh SkillLevelApprovedEvent — request vẫn chuyển Approved trong DB, AuditLog vẫn
//     ghi, SkillHistoryEntry vẫn ghi Outcome = Applied, nhưng User không hề nhận Notification và hồ
//     sơ kỹ năng thực tế KHÔNG hề thay đổi — "im lặng thất bại". Giờ kiểm tra rõ kết quả, chỉ ghi lịch
//     sử Applied khi áp dụng thực sự thành công; nếu thất bại, ghi Rejected kèm lý do kỹ thuật và báo
//     đúng thực tế cho người dùng, tương tự cách SkillReflectionAppliedEventHandler đã xử lý.
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Commons.Result;
using TaskMind.Domain.Entities;
using TaskMind.Domain.Enums;

namespace TaskMind.Applications.Admins.Features.Skills
{
    public class ApproveSkillLevelUpRequestCommand : IRequest<ServiceResult>
    {
        public Guid RequestId { get; }
        public Guid ApproverAdminId { get; }

        public ApproveSkillLevelUpRequestCommand(Guid requestId, Guid approverAdminId)
        {
            RequestId = requestId;
            ApproverAdminId = approverAdminId;
        }
    }

    public class ApproveSkillLevelUpRequestHandler : IRequestHandler<ApproveSkillLevelUpRequestCommand, ServiceResult>
    {
        private readonly IApplicationDbContext _dbContext;

        public ApproveSkillLevelUpRequestHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ServiceResult> Handle(ApproveSkillLevelUpRequestCommand command, CancellationToken cancellationToken)
        {
            var request = await _dbContext.SkillLevelUpRequests
                .FirstOrDefaultAsync(r => r.Id == command.RequestId, cancellationToken);

            if (request == null)
                return ServiceResult.NotFound("Không tìm thấy yêu cầu nâng cấp độ kỹ năng.");

            var approveResult = request.Approve();
            if (!approveResult.IsSuccess)
                return ServiceResult.Failure(approveResult.Message);

            var newLevel = (SkillLevel)Math.Min((int)SkillLevel.Expert, (int)request.CurrentLevel + 1);

            var profile = await _dbContext.SkillProfiles
                .FirstOrDefaultAsync(p => p.UserId == request.UserId, cancellationToken);

            Result? applyResult = null;
            if (profile != null)
            {
                // Nguồn phát SkillLevelApprovedEvent DUY NHẤT — SkillLevelApprovedEventHandler sẽ lo
                // phần Notification/email cho user, NHƯNG chỉ khi việc áp dụng thực sự thành công.
                applyResult = profile.ApplyLevelUp(request.SkillId, newLevel);
            }

            var appliedSuccessfully = applyResult?.IsSuccess ?? false;

            if (profile == null)
            {
                // [MỚI - fix] Fallback hiếm gặp: không tìm thấy SkillProfile nên không có event nào
                // được phát sinh — vẫn phải báo cho user biết yêu cầu đã được duyệt.
                var fallbackNotif = Notification.Create(
                    request.UserId,
                    "Nâng cấp độ kỹ năng thành công",
                    $"Yêu cầu nâng cấp kỹ năng của bạn đã được duyệt lên mức {newLevel}.",
                    NotificationType.Success);

                if (fallbackNotif.IsSuccess)
                    _dbContext.Notifications.Add(fallbackNotif.Data!);
            }
            else if (!appliedSuccessfully)
            {
                // [MỚI - fix] profile tồn tại nhưng ApplyLevelUp thất bại (thường do thiếu
                // UserSkillRecord đúng SkillId — dữ liệu bất thường). KHÔNG được để im lặng: báo đúng
                // sự thật cho user thay vì mặc định "thành công".
                var failureNotif = Notification.Create(
                    request.UserId,
                    "Không thể áp dụng nâng cấp kỹ năng",
                    "Yêu cầu nâng cấp kỹ năng của bạn đã được duyệt nhưng hệ thống gặp lỗi khi cập nhật " +
                    "hồ sơ kỹ năng (không tìm thấy kỹ năng tương ứng trong hồ sơ). Vui lòng liên hệ hỗ trợ để được xử lý thủ công.",
                    NotificationType.Warning);

                if (failureNotif.IsSuccess)
                    _dbContext.Notifications.Add(failureNotif.Data!);
            }

            var auditResult = AuditLog.Record(command.ApproverAdminId, "SkillLevelUpApproved", nameof(SkillLevelUpRequest), request.Id);
            if (auditResult.IsSuccess)
                _dbContext.AuditLogs.Add(auditResult.Data!);

            // [MỚI - fix, mục 4.3.3] Ghi nhận lịch sử thay đổi kỹ năng — CHỈ đánh dấu Applied khi việc
            // áp dụng lên SkillProfile thực sự thành công (hoặc không có profile để áp dụng, coi như
            // trường hợp fallback đặc biệt); nếu áp dụng thất bại do lệch dữ liệu, ghi Rejected kèm lý
            // do kỹ thuật để không tạo lịch sử "Applied" giả.
            var responsibleAccountId = request.ApproverAccountId != Guid.Empty ? request.ApproverAccountId : command.ApproverAdminId;

            Result<SkillHistoryEntry> historyResult;
            if (profile != null && !appliedSuccessfully)
            {
                historyResult = SkillHistoryEntry.Record(
                    userId: request.UserId,
                    skillId: request.SkillId,
                    changeSource: SkillChangeSource.UserInitiated,
                    responsibleAccountId: responsibleAccountId,
                    outcome: SkillHistoryOutcome.Rejected,
                    levelBefore: request.CurrentLevel,
                    levelAfter: null,
                    relatedSubmissionId: request.SubmissionId,
                    relatedRequestId: request.Id,
                    rejectionReason: $"Lỗi kỹ thuật khi áp dụng lên hồ sơ: {applyResult?.Message ?? "không rõ nguyên nhân"}");
            }
            else
            {
                historyResult = SkillHistoryEntry.Record(
                    userId: request.UserId,
                    skillId: request.SkillId,
                    changeSource: SkillChangeSource.UserInitiated,
                    responsibleAccountId: responsibleAccountId,
                    outcome: SkillHistoryOutcome.Applied,
                    levelBefore: request.CurrentLevel,
                    levelAfter: newLevel,
                    relatedSubmissionId: request.SubmissionId,
                    relatedRequestId: request.Id);
            }

            if (historyResult.IsSuccess)
                _dbContext.SkillHistoryEntries.Add(historyResult.Data!);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return profile != null && !appliedSuccessfully
                ? ServiceResult.Success("Yêu cầu đã được duyệt nhưng áp dụng lên hồ sơ kỹ năng gặp lỗi — đã ghi nhận để xử lý thủ công")
                : ServiceResult.Success("Duyệt yêu cầu nâng cấp độ kỹ năng thành công");
        }
    }
}