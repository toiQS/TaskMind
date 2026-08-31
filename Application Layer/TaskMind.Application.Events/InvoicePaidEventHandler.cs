using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Entities;
using TaskMind.Domain.Enums;

public class InvoicePaidEventHandler : INotificationHandler<InvoicePaidEvent>
{
    private readonly IApplicationDbContext _dbContext;

    public InvoicePaidEventHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(InvoicePaidEvent notification, CancellationToken cancellationToken)
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
            "Hoá đơn đã thanh toán",
            $"Hoá đơn của bạn trị giá {notification.Amount:N0} đã được thanh toán thành công.",
            NotificationType.Success);

        if (notifResult.IsSuccess)
            _dbContext.Notifications.Add(notifResult.Data!);

        // TODO: AuditLog.Record(Guid.Empty, "PaymentIssued", nameof(Invoice), notification.InvoiceId)
    }
}