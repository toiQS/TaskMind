using MediatR;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Entities;
using TaskMind.Domain.Enums;
using TaskMind.Domain.Events;

namespace TaskMind.Applications.Events.Handlers
{
    /// <summary>Xử lý khi User được cấp tài khoản Staff sau khi Company mời và xác minh thành công (mục 4.1.1, 4.5).</summary>
    public class StaffJoinedEventHandler : INotificationHandler<StaffJoinedEvent>
    {
        private readonly IApplicationDbContext _dbContext;

        public StaffJoinedEventHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task Handle(StaffJoinedEvent notification, CancellationToken cancellationToken)
        {
            var notifResult = Notification.Create(
                notification.LinkedUserId,
                "Gia nhập công ty",
                "Bạn đã được cấp tài khoản Staff sau khi công ty xác minh thành công.",
                NotificationType.Approval);

            if (notifResult.IsSuccess)
                _dbContext.Notifications.Add(notifResult.Data!);

            return Task.CompletedTask;
        }
    }
}
