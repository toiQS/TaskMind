using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Admins.Dtos;
using TaskMind.Applications.Commons;

namespace TaskMind.Applications.Admins.Features.Companies
{
    /// <summary>Admin xem danh sách công ty, lọc theo trạng thái duyệt/hoạt động (mục 4.4).</summary>
    public class GetCompaniesQuery : ServiceResult<PagedResult<CompanyListItemDto>>
    {
        public GetCompaniesFilter Filter { get; }

        public GetCompaniesQuery(GetCompaniesFilter filter)
        {
            Filter = filter;
        }
    }

    public class GetCompaniesHandler
    {
        private readonly IApplicationDbContext _dbContext;

        public GetCompaniesHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ServiceResult<PagedResult<CompanyListItemDto>>> Handle(GetCompaniesQuery query, CancellationToken cancellationToken)
        {
            var filter = query.Filter;
            var page = Math.Max(1, filter.Page);
            var pageSize = filter.PageSize is > 0 and <= 100 ? filter.PageSize : 20;

            var companiesQuery = _dbContext.Companies.AsNoTracking();

            if (filter.IsVerified.HasValue)
                companiesQuery = companiesQuery.Where(c => c.IsVerified == filter.IsVerified.Value);

            if (filter.Status.HasValue)
                companiesQuery = companiesQuery.Where(c => c.Status == filter.Status.Value);

            if (!string.IsNullOrWhiteSpace(filter.Keyword))
            {
                var keyword = filter.Keyword.Trim();
                companiesQuery = companiesQuery.Where(c =>
                    EF.Functions.ILike(c.CompanyName, $"%{keyword}%") ||
                    EF.Functions.ILike(c.TaxCode, $"%{keyword}%") ||
                    EF.Functions.ILike(c.Email, $"%{keyword}%"));
            }

            var totalCount = await companiesQuery.CountAsync(cancellationToken);

            var items = await companiesQuery
                .OrderByDescending(c => c.JoinDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new CompanyListItemDto
                {
                    Id = c.Id,
                    CompanyName = c.CompanyName,
                    TaxCode = c.TaxCode,
                    Field = c.Field,
                    Email = c.Email,
                    IsVerified = c.IsVerified,
                    Status = c.Status,
                    MembershipPackage = c.MembershipPackage,
                    JoinDate = c.JoinDate
                })
                .ToListAsync(cancellationToken);

            var result = new PagedResult<CompanyListItemDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };

            return ServiceResult<PagedResult<CompanyListItemDto>>.Success(result, "Lấy danh sách công ty thành công");
        }
    }
}