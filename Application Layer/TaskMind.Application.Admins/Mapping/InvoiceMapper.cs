using TaskMind.Applications.Admins.Dtos;
using TaskMind.Domain.Entities;

namespace TaskMind.Applications.Admins.Mapping
{
    public static class InvoiceMapper
    {
        public static InvoiceDto ToDto(Invoice invoice) => new InvoiceDto
        {
            Id = invoice.Id,
            PartnerId = invoice.PartnerId,
            PartnerType = invoice.PartnerType.ToString(),
            Amount = invoice.Amount.Amount,
            Currency = invoice.Amount.Currency,
            Source = invoice.RelatedExchangeContractId.HasValue ? "TransactionFee" : "MembershipFee",
            Status = invoice.InvoiceStatus.ToString(),
            CreatedDateUtc = invoice.CreatedAtUtc.UtcDateTime,
            PaidDateUtc = invoice.PaidAtUtc
        };
    }
}