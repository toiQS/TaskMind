using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Commons;

namespace TaskMind.Applications.Admins.Features.Skills
{
    /// <summary>Admin liên kết hai kỹ năng có liên quan với nhau trong danh mục (mục 4.16).</summary>
    public class LinkRelatedSkillCommand : ServiceResult
    {
        public Guid SkillId { get; }
        public Guid RelatedSkillId { get; }

        public LinkRelatedSkillCommand(Guid skillId, Guid relatedSkillId)
        {
            SkillId = skillId;
            RelatedSkillId = relatedSkillId;
        }
    }

    public class LinkRelatedSkillHandler
    {
        private readonly IApplicationDbContext _dbContext;

        public LinkRelatedSkillHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ServiceResult> Handle(LinkRelatedSkillCommand command, CancellationToken cancellationToken)
        {
            var skill = await _dbContext.Skills
                .FirstOrDefaultAsync(s => s.Id == command.SkillId, cancellationToken);

            if (skill == null)
                return ServiceResult.NotFound("Không tìm thấy kỹ năng.");

            var relatedExists = await _dbContext.Skills
                .AnyAsync(s => s.Id == command.RelatedSkillId, cancellationToken);

            if (!relatedExists)
                return ServiceResult.NotFound("Không tìm thấy kỹ năng liên quan.");

            var result = skill.LinkRelatedSkill(command.RelatedSkillId);
            if (!result.IsSuccess)
                return ServiceResult.Failure(result.Message);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return ServiceResult.Success("Liên kết kỹ năng thành công");
        }
    }
}