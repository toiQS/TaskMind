using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Entities;

namespace TaskMind.Applications.Admins.Features.Skills
{
    /// <summary>
    /// Duyệt yêu cầu nâng cấp độ kỹ năng (mục 4.3.1). Cập nhật cả SkillLevelUpRequest.Approve()
    /// (raise SkillLevelApprovedEvent) lẫn SkillProfile.ApplyLevelUp(...) để đồng bộ UserSkillRecord.
    /// </summary>
    public class ApproveSkillLevelUpRequestCommand : ServiceResult
    {
        public Guid RequestId { get; }
        public Guid ApproverAdminId { get; }

        public ApproveSkillLevelUpRequestCommand(Guid requestId, Guid approverAdminId)
        {
            RequestId = requestId;
            ApproverAdminId = approverAdminId;
        }
    }

    public class ApproveSkillLevelUpRequestHandler
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

            var profile = await _dbContext.SkillProfiles
                .FirstOrDefaultAsync(p => p.UserId == request.UserId, cancellationToken);

            if (profile != null)
            {
                var newLevel = (Domain.Enums.SkillLevel)Math.Min(
                    (int)Domain.Enums.SkillLevel.Expert, (int)request.CurrentLevel + 1);
                profile.ApplyLevelUp(request.SkillId, newLevel);
            }

            var auditResult = AuditLog.Record(command.ApproverAdminId, "SkillLevelUpApproved", nameof(SkillLevelUpRequest), request.Id);
            if (auditResult.IsSuccess)
                _dbContext.AuditLogs.Add(auditResult.Data!);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return ServiceResult.Success("Duyệt yêu cầu nâng cấp độ kỹ năng thành công");
        }
    }
}