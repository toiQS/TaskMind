using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Entities;
using TaskMind.Domain.Enums;
using TaskMind.Domain.Events;

namespace TaskMind.Applications.Events.Handlers
{
    /// <summary>
    /// Xử lý khi một Invoice được xuất (mục 4.14, 4.17). SourceRefId tham chiếu đa hình theo SourceType
    /// (mục 8 - vấn đề mở) nên phải resolve người nhận Notification tuỳ từng trường hợp:
    /// CompanySubscription -> AdminCompany, SchoolSubscription -> AdminSchool, ExchangeFee -> PartyA của ExchangeContract.
    /// </summary>
    internal class InvoiceIssuedEventHandler : INotificationHandler<InvoiceIssuedEvent>
    {
        private readonly IApplicationDbContext _dbContext;

        public InvoiceIssuedEventHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Handle(InvoiceIssuedEvent notification, CancellationToken cancellationToken)
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

            var notifResult = Notification.Create(
                recipientAccountId.Value,
                "Hoá đơn mới",
                $"Bạn có hoá đơn mới trị giá {notification.Amount:N0} {notification.Currency}.",
                NotificationType.System);

            if (notifResult.IsSuccess)
                _dbContext.Notifications.Add(notifResult.Data!);
        }
    }
}
