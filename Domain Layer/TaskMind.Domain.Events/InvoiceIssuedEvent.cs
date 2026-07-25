// InvoiceIssuedEvent.cs
using TaskMind.Domain.Commons.Events;

namespace TaskMind.Domain.Events
{
    public class InvoiceIssuedEvent : DomainEvent
    {
        public Guid InvoiceId { get; init; }
        public Guid PartnerId { get; init; }
        public decimal Amount { get; init; }
        public string Currency { get; init; } = "VND";
    }
}