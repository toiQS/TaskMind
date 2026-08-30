// InvoicePaidEvent.cs
using TaskMind.Domain.Commons.Events;

namespace TaskMind.Domain.Events
{
    /// <summary>[CẬP NHẬT] PartnerId cũ đổi thành SourceRefId để khớp Invoice.SourceRefId (mục 4.14).</summary>
    public class InvoicePaidEvent : DomainEvent
    {
        public Guid InvoiceId { get; init; }
        public Guid SourceRefId { get; init; }
        public decimal Amount { get; init; }
    }
}
