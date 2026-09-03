// InvoiceOverdueEvent.cs — [MỚI - fix]
// Trước đây Invoice.MarkAsOverdue() không raise domain event nào, khiến MarkInvoiceAsOverdueCommand
// phải tự copy-paste switch-case "tra recipientAccountId theo SourceType" — logic này đã lặp lại 2
// lần ở InvoiceIssuedEventHandler và InvoicePaidEventHandler. Thêm event này để MarkAsOverdue() đi
// theo đúng pattern domain-event như MarkAsPaid()/Create(), tránh lặp code lần thứ 3 và tránh rủi ro
// quên cập nhật một trong ba nơi khi có InvoiceSourceType mới.
using TaskMind.Domain.Commons.Events;
using TaskMind.Domain.Enums;

namespace TaskMind.Domain.Events
{
    public class InvoiceOverdueEvent : DomainEvent
    {
        public Guid InvoiceId { get; init; }
        public InvoiceSourceType SourceType { get; init; }
        public Guid SourceRefId { get; init; }
        public decimal Amount { get; init; }
        public string Currency { get; init; } = "VND";
    }
}