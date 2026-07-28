using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Admins.Dtos;
using TaskMind.Applications.Admins.Mapping;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Entities;

namespace TaskMind.Applications.Admins.Features.Profit
{
    /// <summary>Tổng hợp doanh thu theo 2 nguồn thu (mục 4.13): phí giao dịch trao đổi và phí thành viên.</summary>
    public class GetProfitSummaryQuery : IRequest<ProfitSummaryDto>
    {
        public int RecentInvoiceCount { get; set; } = 10;
    }

    public class GetProfitSummaryQueryHandler : IRequestHandler<GetProfitSummaryQuery, ProfitSummaryDto>
    {
        private readonly IApplicationDbContext _db;

        public GetProfitSummaryQueryHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<ProfitSummaryDto> Handle(GetProfitSummaryQuery request, CancellationToken cancellationToken)
        {
            var invoices = await _db.Invoices.ToListAsync(cancellationToken);

            var transactionFeeRevenue = invoices
                .Where(i => i.RelatedExchangeContractId.HasValue)
                .Sum(i => i.Amount.Amount);

            var membershipFeeRevenue = invoices
                .Where(i => !i.RelatedExchangeContractId.HasValue)
                .Sum(i => i.Amount.Amount);

            var recent = invoices
                .OrderByDescending(i => i.CreatedAtUtc)
                .Take(Math.Max(1, request.RecentInvoiceCount))
                .Select(InvoiceMapper.ToDto)
                .ToList();

            return new ProfitSummaryDto
            {
                TotalRevenue = transactionFeeRevenue + membershipFeeRevenue,
                TransactionFeeRevenue = transactionFeeRevenue,
                MembershipFeeRevenue = membershipFeeRevenue,
                TotalInvoices = invoices.Count,
                RecentInvoices = recent
            };
        }
    }
}
