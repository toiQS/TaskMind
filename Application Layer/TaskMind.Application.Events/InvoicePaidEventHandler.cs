using MediatR;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Events;

namespace TaskMind.Applications.Events.Handlers
{
    /// <summary>
    /// Xử lý khi một Invoice được thanh toán (mục 7.3.6: AuditLog ghi nhận Action = PaymentIssued).
    /// LƯU Ý: khác với InvoiceIssuedEvent, InvoicePaidEvent hiện KHÔNG mang SourceType — nên không thể
    /// resolve người nhận Notification (Company/School/ExchangeContract) như InvoiceIssuedEventHandler.
    /// Đề xuất: bổ sung SourceType vào InvoicePaidEvent (Invoice.MarkAsPaid) nếu muốn gửi Notification
    /// "hoá đơn đã thanh toán" chính xác.
    /// </summary>
    internal class InvoicePaidEventHandler : INotificationHandler<InvoicePaidEvent>
    {
        private readonly IApplicationDbContext _dbContext;

        public InvoicePaidEventHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task Handle(InvoicePaidEvent notification, CancellationToken cancellationToken)
        {
            // TODO: AuditLog.Record(Guid.Empty, "PaymentIssued", nameof(Invoice), notification.InvoiceId)
            // khi IApplicationDbContext bổ sung DbSet<AuditLog> (mục 4.21, 7.3.6).

            return Task.CompletedTask;
        }
    }
}
