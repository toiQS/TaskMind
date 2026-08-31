using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Admins.Dtos;
using TaskMind.Applications.Commons;

namespace TaskMind.Applications.Admins.Features.Certificates
{
    /// <summary>Admin tra cứu/xác minh chứng chỉ điện tử đã cấp (mục 4.20).</summary>
    public class GetCertificatesQuery : ServiceResult<PagedResult<CertificateListItemDto>>
    {
        public GetCertificatesFilter Filter { get; }

        public GetCertificatesQuery(GetCertificatesFilter filter)
        {
            Filter = filter;
        }
    }

    public class GetCertificatesHandler
    {
        private readonly IApplicationDbContext _dbContext;

        public GetCertificatesHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ServiceResult<PagedResult<CertificateListItemDto>>> Handle(GetCertificatesQuery query, CancellationToken cancellationToken)
        {
            var filter = query.Filter;
            var page = Math.Max(1, filter.Page);
            var pageSize = filter.PageSize is > 0 and <= 100 ? filter.PageSize : 20;

            var certsQuery = _dbContext.Certificates.AsNoTracking();

            if (filter.UserId.HasValue)
                certsQuery = certsQuery.Where(c => c.UserId == filter.UserId.Value);

            if (!string.IsNullOrWhiteSpace(filter.VerificationCode))
                certsQuery = certsQuery.Where(c => c.VerificationCode == filter.VerificationCode.Trim().ToUpper());

            var totalCount = await certsQuery.CountAsync(cancellationToken);

            var items = await certsQuery
                .OrderByDescending(c => c.IssuedAtUtc)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new CertificateListItemDto
                {
                    Id = c.Id,
                    UserId = c.UserId,
                    SubmissionId = c.SubmissionId,
                    VerificationCode = c.VerificationCode,
                    IssuedAtUtc = c.IssuedAtUtc
                })
                .ToListAsync(cancellationToken);

            var result = new PagedResult<CertificateListItemDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };

            return ServiceResult<PagedResult<CertificateListItemDto>>.Success(result, "Lấy danh sách chứng chỉ thành công");
        }
    }
}