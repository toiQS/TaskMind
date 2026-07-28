using MediatR;
using TaskMind.Applications.Admins.Dtos;
using TaskMind.Applications.Admins.Mapping;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Entities;
using TaskMind.Domain.Enums;

namespace TaskMind.Applications.Admins.Features.Skills
{
    /// <summary>Admin tạo kỹ năng mới trực tiếp — được duyệt ngay vào danh mục chính thức (mục 4.15).</summary>
    public class AddSkillCommand : IRequest<SkillDto>
    {
        public string Name { get; set; } = string.Empty;
        public SkillCategory Category { get; set; }
    }

    public class AddSkillCommandHandler : IRequestHandler<AddSkillCommand, SkillDto>
    {
        private readonly IApplicationDbContext _db;

        public AddSkillCommandHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<SkillDto> Handle(AddSkillCommand request, CancellationToken cancellationToken)
        {
            var result = Skill.CreateByAdmin(request.Name, request.Category);
            if (!result.IsSuccess)
                throw new InvalidOperationException(result.Message);

            var skill = result.Data!;
            _db.Skills.Add(skill);
            await _db.SaveChangesAsync(cancellationToken);

            return SkillMapper.ToDto(skill);
        }
    }
}
