// InvoicePaidEvent.cs
using TaskMind.Domain.Commons.Events;
using TaskMind.Domain.Enums;

public class InvoicePaidEvent : DomainEvent
{
    public Guid InvoiceId { get; init; }
    public InvoiceSourceType SourceType { get; init; }   // [MỚI]
    public Guid SourceRefId { get; init; }
    public decimal Amount { get; init; }
}