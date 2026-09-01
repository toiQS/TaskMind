// RejectSkillCommand.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Commons;

namespace TaskMind.Applications.Admins.Features.Skills
{
    public class RejectSkillCommand : IRequest<ServiceResult>
    {
        public Guid SkillId { get; }

        public RejectSkillCommand(Guid skillId)
        {
            SkillId = skillId;
        }
    }

    public class RejectSkillHandler : IRequestHandler<RejectSkillCommand, ServiceResult>
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