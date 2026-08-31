using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Entities;

namespace TaskMind.Applications.Admins.Features.Skills
{
    /// <summary>
    /// Từ chối yêu cầu nâng cấp độ kỹ năng (mục 4.3.1). Sau khi Reject() raise SkillLevelUpRejectedEvent,
    /// áp dụng luôn SkillProfile.ApplyPenaltyDowngrade (x2) theo đúng quy tắc cảnh báo chính thức.
    /// </summary>
    public class RejectSkillLevelUpRequestCommand : ServiceResult
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

    public class RejectSkillLevelUpRequestHandler
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

            profile?.ApplyPenaltyDowngrade(request.SkillId);

            var auditResult = AuditLog.Record(command.ApproverAdminId, "SkillLevelUpRejected", nameof(SkillLevelUpRequest), request.Id);
            if (auditResult.IsSuccess)
                _dbContext.AuditLogs.Add(auditResult.Data!);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return ServiceResult.Success("Từ chối yêu cầu nâng cấp độ kỹ năng thành công");
        }
    }
}