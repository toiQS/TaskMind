// RejectSkillLevelUpRequestCommand.cs
// [CẬP NHẬT - fix] Ghi SkillHistoryEntry (mục 4.3.3) — đề xuất bị từ chối vẫn phải lưu lại đầy đủ vào
// lịch sử kỹ năng, không chỉ các thay đổi đã áp dụng thành công. Trước đây SkillHistoryEntry hoàn
// toàn không được ghi ở bất kỳ luồng nào dù entity/DbSet đã sẵn sàng.
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
            if (profile != null)
            {
                profile.ApplyPenaltyDowngrade(request.SkillId);
                levelAfter = profile.Records.FirstOrDefault(r => r.SkillId == request.SkillId)?.Level;
            }

            var auditResult = AuditLog.Record(command.ApproverAdminId, "SkillLevelUpRejected", nameof(SkillLevelUpRequest), request.Id);
            if (auditResult.IsSuccess)
                _dbContext.AuditLogs.Add(auditResult.Data!);

            // [MỚI - fix, mục 4.3.3] Ghi nhận lịch sử — bao gồm cả đề xuất bị từ chối.
            var responsibleAccountId = request.ApproverAccountId != Guid.Empty ? request.ApproverAccountId : command.ApproverAdminId;
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
                rejectionReason: string.IsNullOrWhiteSpace(command.Reason) ? "Không đạt yêu cầu xác minh." : command.Reason);

            if (historyResult.IsSuccess)
                _dbContext.SkillHistoryEntries.Add(historyResult.Data!);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return ServiceResult.Success("Từ chối yêu cầu nâng cấp độ kỹ năng thành công");
        }
    }
}