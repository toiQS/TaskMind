// RejectSkillLevelUpRequestCommand.cs
// [CẬP NHẬT - fix] profile.ApplyPenaltyDowngrade(...) trước đây không kiểm tra Result trả về — nếu
// thất bại (ví dụ không tìm thấy record kỹ năng trong hồ sơ, dữ liệu bất thường), SkillPenaltyAppliedEvent
// không phát sinh (user không có Notification cảnh báo hạ cấp) nhưng SkillHistoryEntry vẫn ghi như
// bình thường (dù levelAfter khi đó = null nên không sai fact, chỉ thiếu ghi nhận rõ ràng lý do).
// Giờ kiểm tra rõ và note lại rejectionReason kỹ thuật khi việc hạ cấp thất bại.
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Commons;
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
            bool penaltyApplied = false;
            string? technicalNote = null;

            if (profile != null)
            {
                var penaltyResult = profile.ApplyPenaltyDowngrade(request.SkillId);
                penaltyApplied = penaltyResult.IsSuccess;

                if (penaltyApplied)
                {
                    levelAfter = profile.Records.FirstOrDefault(r => r.SkillId == request.SkillId)?.Level;
                }
                else
                {
                    // [MỚI - fix] Không nuốt lỗi âm thầm — ghi rõ lý do kỹ thuật vào lịch sử để dễ truy vết.
                    technicalNote = $" (Lưu ý kỹ thuật: không thể áp dụng hạ cấp penalty — {penaltyResult.Message})";
                }
            }
            else
            {
                technicalNote = " (Lưu ý kỹ thuật: không tìm thấy hồ sơ kỹ năng để áp dụng hạ cấp penalty)";
            }

            var auditResult = AuditLog.Record(command.ApproverAdminId, "SkillLevelUpRejected", nameof(SkillLevelUpRequest), request.Id);
            if (auditResult.IsSuccess)
                _dbContext.AuditLogs.Add(auditResult.Data!);

            var responsibleAccountId = request.ApproverAccountId != Guid.Empty ? request.ApproverAccountId : command.ApproverAdminId;
            var baseReason = string.IsNullOrWhiteSpace(command.Reason) ? "Không đạt yêu cầu xác minh." : command.Reason;

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
                rejectionReason: baseReason + technicalNote);

            if (historyResult.IsSuccess)
                _dbContext.SkillHistoryEntries.Add(historyResult.Data!);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return ServiceResult.Success("Từ chối yêu cầu nâng cấp độ kỹ năng thành công");
        }
    }
}