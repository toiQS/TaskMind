// GetSchoolsQuery.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Admins.Dtos;
using TaskMind.Applications.Commons;

namespace TaskMind.Applications.Admins.Features.Schools
{
    public class GetSchoolsQuery : IRequest<ServiceResult<PagedResult<SchoolListItemDto>>>
    {
        public GetSchoolsFilter Filter { get; }

        public GetSchoolsQuery(GetSchoolsFilter filter)
        {
            Filter = filter;
        }
    }

    public class GetSchoolsHandler : IRequestHandler<GetSchoolsQuery, ServiceResult<PagedResult<SchoolListItemDto>>>
    {
        private readonly IApplicationDbContext _dbContext;

        public GetSchoolsHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ServiceResult<PagedResult<SchoolListItemDto>>> Handle(GetSchoolsQuery query, CancellationToken cancellationToken)
        {
            var filter = query.Filter;
            var page = Math.Max(1, filter.Page);
            var pageSize = filter.PageSize is > 0 and <= 100 ? filter.PageSize : 20;

            var schoolsQuery = _dbContext.Schools.AsNoTracking();

            if (filter.IsVerified.HasValue)
                schoolsQuery = schoolsQuery.Where(s => s.IsVerified == filter.IsVerified.Value);

            if (filter.Status.HasValue)
                schoolsQuery = schoolsQuery.Where(s => s.Status == filter.Status.Value);

            if (!string.IsNullOrWhiteSpace(filter.Keyword))
            {
                var keyword = filter.Keyword.Trim();
                schoolsQuery = schoolsQuery.Where(s =>
                    EF.Functions.ILike(s.SchoolName, $"%{keyword}%") ||
                    EF.Functions.ILike(s.Email, $"%{keyword}%"));
            }

            var totalCount = await schoolsQuery.CountAsync(cancellationToken);

            var items = await schoolsQuery
                .OrderByDescending(s => s.JoinDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new SchoolListItemDto
                {
                    Id = s.Id,
                    SchoolName = s.SchoolName,
                    Field = s.Field,
                    Email = s.Email,
                    IsVerified = s.IsVerified,
                    Status = s.Status,
                    MembershipPackage = s.MembershipPackage,
                    JoinDate = s.JoinDate
                })
                .ToListAsync(cancellationToken);

            var result = new PagedResult<SchoolListItemDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };

            return ServiceResult<PagedResult<SchoolListItemDto>>.Success(result, "Lấy danh sách cơ sở đào tạo thành công");
        }
    }
}