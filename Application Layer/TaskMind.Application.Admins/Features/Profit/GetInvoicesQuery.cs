using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Admins.Dtos;
using TaskMind.Applications.Admins.Mapping;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Entities;

namespace TaskMind.Applications.Admins.Features.Profit
{
    /// <summary>Danh sách hoá đơn/giao dịch (mục 4.13, 5.5), kèm tên đối tác để hiển thị và tìm kiếm.</summary>
    public class GetInvoicesQuery : IRequest<List<InvoiceListItemDto>>
    {
        public string? SearchText { get; set; }

        /// <summary>"All" | "TransactionFee" | "MembershipFee"</summary>
        public string SourceFilter { get; set; } = "All";
    }

    /// <summary>InvoiceDto kèm tên đối tác (Company/School), phục vụ hiển thị danh sách giao dịch ở ProfitView.</summary>
    public class InvoiceListItemDto : InvoiceDto
    {
        public string PartnerName { get; set; } = string.Empty;
    }

    public class GetInvoicesQueryHandler : IRequestHandler<GetInvoicesQuery, List<InvoiceListItemDto>>
    {
        private readonly IApplicationDbContext _db;

        public GetInvoicesQueryHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<InvoiceListItemDto>> Handle(GetInvoicesQuery request, CancellationToken cancellationToken)
        {
            var invoices = await _db.Invoices
                .OrderByDescending(i => i.CreatedAtUtc)
                .ToListAsync(cancellationToken);

            if (!string.IsNullOrWhiteSpace(request.SourceFilter) && request.SourceFilter != "All")
            {
                invoices = invoices
                    .Where(i => string.Equals(
                        i.RelatedExchangeContractId.HasValue ? "TransactionFee" : "MembershipFee",
                        request.SourceFilter, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            var companyIds = invoices.Where(i => i.PartnerType == InvoicePartnerType.Company)
                .Select(i => i.PartnerId).Distinct().ToList();
            var schoolIds = invoices.Where(i => i.PartnerType == InvoicePartnerType.School)
                .Select(i => i.PartnerId).Distinct().ToList();

            var companyNames = await _db.Companies
                .Where(c => companyIds.Contains(c.Id))
                .ToDictionaryAsync(c => c.Id, c => c.CompanyName, cancellationToken);
            var schoolNames = await _db.Schools
                .Where(s => schoolIds.Contains(s.Id))
                .ToDictionaryAsync(s => s.Id, s => s.SchoolName, cancellationToken);

            var result = invoices.Select(i =>
            {
                var dto = InvoiceMapper.ToDto(i);
                var partnerName = i.PartnerType == InvoicePartnerType.Company
                    ? companyNames.GetValueOrDefault(i.PartnerId, "(Không rõ công ty)")
                    : schoolNames.GetValueOrDefault(i.PartnerId, "(Không rõ cơ sở đào tạo)");

                return new InvoiceListItemDto
                {
                    Id = dto.Id,
                    PartnerId = dto.PartnerId,
                    PartnerType = dto.PartnerType,
                    Amount = dto.Amount,
                    Currency = dto.Currency,
                    Source = dto.Source,
                    Status = dto.Status,
                    CreatedDateUtc = dto.CreatedDateUtc,
                    PaidDateUtc = dto.PaidDateUtc,
                    PartnerName = partnerName
                };
            });

            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                var s = request.SearchText.Trim();
                result = result.Where(x => x.PartnerName.Contains(s, StringComparison.OrdinalIgnoreCase));
            }

            return result.ToList();
        }
    }
}
