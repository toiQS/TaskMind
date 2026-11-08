using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TaskMind.ApplicationLayer.Commons;
using TaskMind.DomainLayer.TaskMind.Domain.Events;

namespace TaskMind.ApplicationLayer.Commons.Emails;

public sealed class NotificationCreatedEventHandler : INotificationHandler<NotificationCreatedEvent>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IPublisher _publisher;
    private readonly ILogger<NotificationCreatedEventHandler> _logger;

    public NotificationCreatedEventHandler(
        IApplicationDbContext dbContext,
        IPublisher publisher,
        ILogger<NotificationCreatedEventHandler> logger)
    {
        _dbContext = dbContext;
        _publisher = publisher;
        _logger = logger;
    }

    public async Task Handle(NotificationCreatedEvent notificationEvent, CancellationToken cancellationToken)
    {
        var notification = await _dbContext.Notifications
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.Id == notificationEvent.NotificationId,
                cancellationToken);
        if (notification is null)
        {
            _logger.LogError(
                "Notification {NotificationId} was not found after its creation event",
                notificationEvent.NotificationId);
            return;
        }

        var account = await _dbContext.Accounts
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.Id == notificationEvent.RecipientAccountId,
                cancellationToken);
        if (account is null || string.IsNullOrWhiteSpace(account.Profile.Email))
        {
            _logger.LogWarning(
                "Cannot email notification {NotificationId}: recipient {RecipientAccountId} has no account email",
                notificationEvent.NotificationId,
                notificationEvent.RecipientAccountId);
            return;
        }

        await _publisher.Publish(
            new SendEmailEvent(account.Profile.Email, notification.Title, notification.Message),
            cancellationToken);
    }
}
