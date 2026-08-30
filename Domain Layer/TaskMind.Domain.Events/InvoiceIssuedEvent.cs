// InvoiceIssuedEvent.cs
using TaskMind.Domain.Commons.Events;
using TaskMind.Domain.Enums;

namespace TaskMind.Domain.Events
{
    /// <summary>[CẬP NHẬT] PartnerId/PartnerType cũ đổi thành SourceRefId/SourceType để khớp Invoice.SourceType (mục 4.14).</summary>
    public class InvoiceIssuedEvent : DomainEvent
    {
        public Guid InvoiceId { get; init; }
        public InvoiceSourceType SourceType { get; init; }
        public Guid SourceRefId { get; init; }
        public decimal Amount { get; init; }
        public string Currency { get; init; } = "VND";
    }
}
