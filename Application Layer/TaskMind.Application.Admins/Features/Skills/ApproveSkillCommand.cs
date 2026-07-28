using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Admins.Dtos;
using TaskMind.Applications.Admins.Mapping;
using TaskMind.Applications.Commons;

namespace TaskMind.Applications.Admins.Features.Skills
{
    /// <summary>Admin duyệt một đề xuất kỹ năng từ công ty/cơ sở đào tạo vào danh mục chính thức (mục 4.15).</summary>
    public class ApproveSkillCommand : IRequest<SkillDto>
    {
        public Guid SkillId { get; set; }
    }

    public class ApproveSkillCommandHandler : IRequestHandler<ApproveSkillCommand, SkillDto>
    {
        private readonly IApplicationDbContext _db;

        public ApproveSkillCommandHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<SkillDto> Handle(ApproveSkillCommand request, CancellationToken cancellationToken)
        {
            var skill = await _db.Skills.FirstOrDefaultAsync(s => s.Id == request.SkillId, cancellationToken)
                ?? throw new KeyNotFoundException("Không tìm thấy kỹ năng.");

            var result = skill.Approve();
            if (!result.IsSuccess)
                throw new InvalidOperationException(result.Message);

            await _db.SaveChangesAsync(cancellationToken);

            return SkillMapper.ToDto(skill);
        }
    }
}
