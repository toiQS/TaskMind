using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Admins.Dtos;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Enums;

namespace TaskMind.Applications.Admins.Features.Invoices
{
    public class GetInvoicesFilter
    {
        public InvoiceSourceType? SourceType { get; set; }
        public InvoiceStatus? InvoiceStatus { get; set; }
        public Guid? SourceRefId { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    /// <summary>Admin xem/lọc hoá đơn (mục 4.14) — chi tiết hơn RecentInvoices trong Dashboard.</summary>
    public class GetInvoicesQuery : ServiceResult<PagedResult<RecentInvoiceDto>>
    {
        public GetInvoicesFilter Filter { get; }

        public GetInvoicesQuery(GetInvoicesFilter filter)
        {
            Filter = filter;
        }
    }

    public class GetInvoicesHandler
    {
        private readonly IApplicationDbContext _dbContext;

        public GetInvoicesHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ServiceResult<PagedResult<RecentInvoiceDto>>> Handle(GetInvoicesQuery query, CancellationToken cancellationToken)
        {
            var filter = query.Filter;
            var page = Math.Max(1, filter.Page);
            var pageSize = filter.PageSize is > 0 and <= 100 ? filter.PageSize : 20;

            var invoicesQuery = _dbContext.Invoices.AsNoTracking();

            if (filter.SourceType.HasValue)
                invoicesQuery = invoicesQuery.Where(i => i.SourceType == filter.SourceType.Value);

            if (filter.InvoiceStatus.HasValue)
                invoicesQuery = invoicesQuery.Where(i => i.InvoiceStatus == filter.InvoiceStatus.Value);

            if (filter.SourceRefId.HasValue)
                invoicesQuery = invoicesQuery.Where(i => i.SourceRefId == filter.SourceRefId.Value);

            var totalCount = await invoicesQuery.CountAsync(cancellationToken);

            var items = await invoicesQuery
                .OrderByDescending(i => i.CreatedAtUtc)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(i => new RecentInvoiceDto
                {
                    Id = i.Id,
                    SourceType = i.SourceType,
                    SourceRefId = i.SourceRefId,
                    Amount = i.Amount.Amount,
                    Currency = i.Amount.Currency,
                    Status = i.InvoiceStatus,
                    CreatedAtUtc = i.CreatedAtUtc,
                    PaidAtUtc = i.PaidAtUtc
                })
                .ToListAsync(cancellationToken);

            var result = new PagedResult<RecentInvoiceDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };

            return ServiceResult<PagedResult<RecentInvoiceDto>>.Success(result, "Lấy danh sách hoá đơn thành công");
        }
    }
}