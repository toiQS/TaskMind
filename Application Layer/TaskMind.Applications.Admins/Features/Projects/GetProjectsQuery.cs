using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Admins.Dtos;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Enums;

namespace TaskMind.Applications.Admins.Features.Projects
{
    /// <summary>Admin xem toàn bộ dự án trên nền tảng (Company/School/OpenSource) để giám sát (mục 4.7, 4.12, 4.13).</summary>
    internal class GetProjectsQuery : ServiceResult<PagedResult<ProjectListItemDto>>
    {
        public GetProjectsFilter Filter { get; }

        public GetProjectsQuery(GetProjectsFilter filter)
        {
            Filter = filter;
        }
    }

    internal class GetProjectsHandler
    {
        private readonly IApplicationDbContext _dbContext;

        public GetProjectsHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ServiceResult<PagedResult<ProjectListItemDto>>> Handle(GetProjectsQuery query, CancellationToken cancellationToken)
        {
            var filter = query.Filter;
            var page = Math.Max(1, filter.Page);
            var pageSize = filter.PageSize is > 0 and <= 100 ? filter.PageSize : 20;

            var projectsQuery = _dbContext.Projects.AsNoTracking();

            if (filter.SourceType.HasValue)
                projectsQuery = projectsQuery.Where(p => p.SourceType == filter.SourceType.Value);

            if (filter.ProjectStatus.HasValue)
                projectsQuery = projectsQuery.Where(p => p.ProjectStatus == filter.ProjectStatus.Value);

            if (filter.IsExchangeProject.HasValue)
                projectsQuery = projectsQuery.Where(p => p.IsExchangeProject == filter.IsExchangeProject.Value);

            if (filter.OwningEntityId.HasValue)
                projectsQuery = projectsQuery.Where(p => p.OwningEntityId == filter.OwningEntityId.Value);

            if (!string.IsNullOrWhiteSpace(filter.Keyword))
            {
                var keyword = filter.Keyword.Trim();
                projectsQuery = projectsQuery.Where(p => EF.Functions.ILike(p.Title, $"%{keyword}%"));
            }

            var totalCount = await projectsQuery.CountAsync(cancellationToken);

            var page1 = await projectsQuery
                .OrderByDescending(p => p.CreatedAtUtc)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new
                {
                    p.Id,
                    p.Title,
                    p.SourceType,
                    p.ProjectStatus,
                    p.IsExchangeProject,
                    p.OwningEntityId,
                    ActiveMemberCount = p.Members.Count(m => m.LeftAt == null)
                })
                .ToListAsync(cancellationToken);

            // Resolve tên Company/School cho OwningEntityId (tham chiếu đa hình theo SourceType)
            var companyIds = page1.Where(p => p.SourceType == ProjectSourceType.Company && p.OwningEntityId.HasValue)
                .Select(p => p.OwningEntityId!.Value).Distinct().ToList();
            var schoolIds = page1.Where(p => p.SourceType == ProjectSourceType.School && p.OwningEntityId.HasValue)
                .Select(p => p.OwningEntityId!.Value).Distinct().ToList();

            var companyNames = await _dbContext.Companies.AsNoTracking()
                .Where(c => companyIds.Contains(c.Id))
                .ToDictionaryAsync(c => c.Id, c => c.CompanyName, cancellationToken);

            var schoolNames = await _dbContext.Schools.AsNoTracking()
                .Where(s => schoolIds.Contains(s.Id))
                .ToDictionaryAsync(s => s.Id, s => s.SchoolName, cancellationToken);

            var items = page1.Select(p => new ProjectListItemDto
            {
                Id = p.Id,
                Title = p.Title,
                SourceType = p.SourceType,
                ProjectStatus = p.ProjectStatus,
                IsExchangeProject = p.IsExchangeProject,
                OwningEntityId = p.OwningEntityId,
                OwningEntityName = p.SourceType switch
                {
                    ProjectSourceType.Company when p.OwningEntityId.HasValue && companyNames.TryGetValue(p.OwningEntityId.Value, out var cName) => cName,
                    ProjectSourceType.School when p.OwningEntityId.HasValue && schoolNames.TryGetValue(p.OwningEntityId.Value, out var sName) => sName,
                    ProjectSourceType.OpenSource => null,
                    _ => null
                },
                ActiveMemberCount = p.ActiveMemberCount
            }).ToList();

            var result = new PagedResult<ProjectListItemDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };

            return ServiceResult<PagedResult<ProjectListItemDto>>.Success(result, "Lấy danh sách dự án thành công");
        }
    }
}