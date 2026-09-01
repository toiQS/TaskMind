// CreateSkillByAdminCommand.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Entities;
using TaskMind.Domain.Enums;

namespace TaskMind.Applications.Admins.Features.Skills
{
    public class CreateSkillByAdminCommand : IRequest<ServiceResult<Guid>>
    {
        public string SkillName { get; }
        public SkillCategory Category { get; }

        public CreateSkillByAdminCommand(string skillName, SkillCategory category)
        {
            SkillName = skillName;
            Category = category;
        }
    }

    public class CreateSkillByAdminHandler : IRequestHandler<CreateSkillByAdminCommand, ServiceResult<Guid>>
    {
        private readonly IApplicationDbContext _dbContext;

        public CreateSkillByAdminHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ServiceResult<Guid>> Handle(CreateSkillByAdminCommand command, CancellationToken cancellationToken)
        {
            var exists = await _dbContext.Skills
                .AnyAsync(s => s.SkillName.ToLower() == command.SkillName.Trim().ToLower(), cancellationToken);

            if (exists)
                return ServiceResult<Guid>.Failure("Kỹ năng đã tồn tại trong danh mục.");

            var skillResult = Skill.CreateByAdmin(command.SkillName, command.Category);
            if (!skillResult.IsSuccess)
                return ServiceResult<Guid>.Failure(skillResult.Message);

            _dbContext.Skills.Add(skillResult.Data!);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return ServiceResult<Guid>.Success(skillResult.Data!.Id, "Tạo kỹ năng thành công");
        }
    }
}