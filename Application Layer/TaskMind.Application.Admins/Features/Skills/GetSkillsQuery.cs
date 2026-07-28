using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Admins.Dtos;
using TaskMind.Applications.Admins.Mapping;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Enums;

namespace TaskMind.Applications.Admins.Features.Skills
{
    public class GetSkillsQuery : IRequest<List<SkillDto>>
    {
        public string? SearchText { get; set; }

        /// <summary>"All" | tên SkillCategory (ProgrammingLanguage/Framework/SoftSkill/Tool/Other)</summary>
        public string CategoryFilter { get; set; } = "All";

        /// <summary>null = tất cả, true = chỉ danh mục chính thức, false = chỉ đề xuất chờ duyệt</summary>
        public bool? IsApproved { get; set; }
    }

    public class GetSkillsQueryHandler : IRequestHandler<GetSkillsQuery, List<SkillDto>>
    {
        private readonly IApplicationDbContext _db;

        public GetSkillsQueryHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<SkillDto>> Handle(GetSkillsQuery request, CancellationToken cancellationToken)
        {
            var query = _db.Skills.AsQueryable();

            if (request.IsApproved.HasValue)
                query = query.Where(s => s.IsApproved == request.IsApproved.Value);

            if (!string.IsNullOrWhiteSpace(request.CategoryFilter) && request.CategoryFilter != "All" &&
                Enum.TryParse<SkillCategory>(request.CategoryFilter, true, out var category))
            {
                query = query.Where(s => s.Category == category);
            }

            var skills = await query.ToListAsync(cancellationToken);

            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                var s = request.SearchText.Trim();
                skills = skills.Where(x => x.SkillName.Contains(s, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            return skills.Select(SkillMapper.ToDto).ToList();
        }
    }
}
