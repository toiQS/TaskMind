using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Admins.Dtos;
using TaskMind.Applications.Commons;

namespace TaskMind.Applications.Admins.Features.Skills
{
    /// <summary>Admin xem danh mục kỹ năng, lọc theo trạng thái duyệt (mục 4.16) — dùng chung cho cả xem toàn bộ và xem hàng chờ duyệt.</summary>
    internal class GetSkillsQuery : ServiceResult<PagedResult<SkillListItemDto>>
    {
        public GetSkillsFilter Filter { get; }

        public GetSkillsQuery(GetSkillsFilter filter)
        {
            Filter = filter;
        }
    }

    internal class GetSkillsHandler
    {
        private readonly IApplicationDbContext _dbContext;

        public GetSkillsHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ServiceResult<PagedResult<SkillListItemDto>>> Handle(GetSkillsQuery query, CancellationToken cancellationToken)
        {
            var filter = query.Filter;
            var page = Math.Max(1, filter.Page);
            var pageSize = filter.PageSize is > 0 and <= 100 ? filter.PageSize : 20;

            var skillsQuery = _dbContext.Skills.AsNoTracking();

            if (filter.IsApproved.HasValue)
                skillsQuery = skillsQuery.Where(s => s.IsApproved == filter.IsApproved.Value);

            if (filter.Category.HasValue)
                skillsQuery = skillsQuery.Where(s => s.Category == filter.Category.Value);

            if (!string.IsNullOrWhiteSpace(filter.Keyword))
            {
                var keyword = filter.Keyword.Trim();
                skillsQuery = skillsQuery.Where(s => EF.Functions.ILike(s.SkillName, $"%{keyword}%"));
            }

            var totalCount = await skillsQuery.CountAsync(cancellationToken);

            var items = await skillsQuery
                .OrderBy(s => s.IsApproved) // chờ duyệt (false) lên trước
                .ThenBy(s => s.SkillName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new SkillListItemDto
                {
                    Id = s.Id,
                    SkillName = s.SkillName,
                    Category = s.Category,
                    IsApproved = s.IsApproved,
                    SuggestedBy = s.SuggestedBy,
                    RelatedSkillIds = s.RelatedSkillIds.ToList()
                })
                .ToListAsync(cancellationToken);

            var result = new PagedResult<SkillListItemDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };

            return ServiceResult<PagedResult<SkillListItemDto>>.Success(result, "Lấy danh mục kỹ năng thành công");
        }
    }
}