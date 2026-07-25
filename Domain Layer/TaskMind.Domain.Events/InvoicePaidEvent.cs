// InvoicePaidEvent.cs
using TaskMind.Domain.Commons.Events;

namespace TaskMind.Domain.Events
{
    public class InvoicePaidEvent : DomainEvent
    {
        public Guid InvoiceId { get; init; }
        public Guid PartnerId { get; init; }
        public decimal Amount { get; init; }
    }
}