using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Admins.Dtos;
using TaskMind.Applications.Admins.Mapping;
using TaskMind.Applications.Commons;

namespace TaskMind.Applications.Admins.Features.Skills
{
    public class GetSkillDetailQuery : IRequest<SkillDetailDto>
    {
        public Guid SkillId { get; set; }
    }

    public class GetSkillDetailQueryHandler : IRequestHandler<GetSkillDetailQuery, SkillDetailDto>
    {
        private readonly IApplicationDbContext _db;

        public GetSkillDetailQueryHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<SkillDetailDto> Handle(GetSkillDetailQuery request, CancellationToken cancellationToken)
        {
            var skill = await _db.Skills.FirstOrDefaultAsync(s => s.Id == request.SkillId, cancellationToken)
                ?? throw new KeyNotFoundException("Không tìm thấy kỹ năng.");

            var relatedSkills = await _db.Skills
                .Where(s => skill.RelatedSkillIds.Contains(s.Id))
                .ToListAsync(cancellationToken);

            // Số người dùng đã khai báo kỹ năng này trong SkillProfile.
            var usageCount = await _db.SkillProfiles
                .SelectMany(sp => sp.Records)
                .CountAsync(r => r.SkillId == request.SkillId, cancellationToken);

            var dto = SkillMapper.ToDto(skill);
            var detail = new SkillDetailDto
            {
                Id = dto.Id,
                Name = dto.Name,
                Category = dto.Category,
                IsApproved = dto.IsApproved,
                SuggestedBy = dto.SuggestedBy,
                CreatedDateUtc = dto.CreatedDateUtc,
                RelatedSkills = relatedSkills.Select(SkillMapper.ToDto).ToList(),
                UsageCount = usageCount
            };

            return detail;
        }
    }
}
