using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Commons;

namespace TaskMind.Applications.Admins.Features.Skills
{
    /// <summary>
    /// Admin từ chối một kỹ năng đang chờ duyệt (mục 4.16). Domain.Skill không có method Reject riêng
    /// (chỉ SkillCatalog.Reject có, nhưng ta không dùng SkillCatalog ở tầng Application này) — nên
    /// kiểm tra bất biến "chỉ xoá được kỹ năng chưa duyệt" trực tiếp ở Handler rồi xoá khỏi DbSet.
    /// </summary>
    internal class RejectSkillCommand : ServiceResult
    {
        public Guid SkillId { get; }

        public RejectSkillCommand(Guid skillId)
        {
            SkillId = skillId;
        }
    }

    internal class RejectSkillHandler
    {
        private readonly IApplicationDbContext _dbContext;

        public RejectSkillHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ServiceResult> Handle(RejectSkillCommand command, CancellationToken cancellationToken)
        {
            var skill = await _dbContext.Skills
                .FirstOrDefaultAsync(s => s.Id == command.SkillId, cancellationToken);

            if (skill == null)
                return ServiceResult.NotFound("Không tìm thấy kỹ năng.");

            if (skill.IsApproved)
                return ServiceResult.Failure("Không thể từ chối kỹ năng đã được duyệt.");

            _dbContext.Skills.Remove(skill);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return ServiceResult.Success("Từ chối kỹ năng thành công");
        }
    }
}