// Domain Layer/TaskMind.Domain.Events/InvoicePaidEvent.cs
using TaskMind.Domain.Commons.Events;
using TaskMind.Domain.Enums;

namespace TaskMind.Domain.Events   // [MỚI - fix] trước đây thiếu namespace, class rơi vào global namespace
{
    public class InvoicePaidEvent : DomainEvent
    {
        public Guid InvoiceId { get; init; }
        public InvoiceSourceType SourceType { get; init; }
        public Guid SourceRefId { get; init; }
        public decimal Amount { get; init; }
    }
}