using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Commons;

namespace TaskMind.Applications.Admins.Features.Skills
{
    /// <summary>Admin duyệt một kỹ năng do công ty/cơ sở đào tạo đề xuất (mục 4.16).</summary>
    public class ApproveSkillCommand : ServiceResult
    {
        public Guid SkillId { get; }
        public Guid ApproverAdminId { get; }

        public ApproveSkillCommand(Guid skillId, Guid approverAdminId)
        {
            SkillId = skillId;
            ApproverAdminId = approverAdminId;
        }
    }

    public class ApproveSkillHandler
    {
        private readonly IApplicationDbContext _dbContext;

        public ApproveSkillHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ServiceResult> Handle(ApproveSkillCommand command, CancellationToken cancellationToken)
        {
            var skill = await _dbContext.Skills
                .FirstOrDefaultAsync(s => s.Id == command.SkillId, cancellationToken);

            if (skill == null)
                return ServiceResult.NotFound("Không tìm thấy kỹ năng.");

            var result = skill.Approve();
            if (!result.IsSuccess)
                return ServiceResult.Failure(result.Message);

            // TODO: AuditLog.Record(command.ApproverAdminId, "SkillApproved", nameof(Skill), skill.Id)

            await _dbContext.SaveChangesAsync(cancellationToken);

            return ServiceResult.Success("Duyệt kỹ năng thành công");
        }
    }
}