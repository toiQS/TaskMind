// InvoiceOverdueEventHandler.cs — [MỚI - fix]
// Xử lý khi một Invoice chuyển sang trạng thái quá hạn (Invoice.MarkAsOverdue()). Tái sử dụng đúng
// logic tra recipientAccountId theo SourceType như InvoiceIssuedEventHandler/InvoicePaidEventHandler,
// giờ tập trung lại một nơi thay vì copy-paste thêm lần thứ 3 trong MarkInvoiceAsOverdueCommand.
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Enums;
using TaskMind.Domain.Events;

namespace TaskMind.Applications.Events
{
    public class InvoiceOverdueEventHandler : INotificationHandler<InvoiceOverdueEvent>
    {
        private readonly IApplicationDbContext _dbContext;

        public InvoiceOverdueEventHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Handle(InvoiceOverdueEvent notification, CancellationToken cancellationToken)
        {
            Guid? recipientAccountId = notification.SourceType switch
            {
                InvoiceSourceType.CompanySubscription => (await _dbContext.AdminCompanies
                    .AsNoTracking()
                    .FirstOrDefaultAsync(ac => ac.CompanyId == notification.SourceRefId, cancellationToken))?.LinkedUserId,

                InvoiceSourceType.SchoolSubscription => (await _dbContext.AdminSchools
                    .AsNoTracking()
                    .FirstOrDefaultAsync(a => a.SchoolId == notification.SourceRefId, cancellationToken))?.LinkedUserId,

                InvoiceSourceType.ExchangeFee => (await _dbContext.ExchangeContracts
                    .AsNoTracking()
                    .FirstOrDefaultAsync(e => e.Id == notification.SourceRefId, cancellationToken))?.PartyAAccountId,

                _ => null
            };

            if (recipientAccountId is null || recipientAccountId == Guid.Empty)
                return;

            var notifResult = Domain.Entities.Notification.Create(
                recipientAccountId.Value,
                "Hoá đơn quá hạn thanh toán",
                $"Hoá đơn trị giá {notification.Amount:N0} {notification.Currency} của bạn đã quá hạn thanh toán. Vui lòng thanh toán sớm nhất có thể.",
                NotificationType.Warning);

            if (notifResult.IsSuccess)
                _dbContext.Notifications.Add(notifResult.Data!);
        }
    }
}