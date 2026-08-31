using MediatR;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Entities;
using TaskMind.Domain.Enums;
using TaskMind.Domain.Events;

namespace TaskMind.Applications.Events.Handlers
{
    /// <summary>Xử lý khi một Message mới được gửi trong Chat; tạo Notification tới các thành viên còn lại (mục 4.22, 4.17).</summary>
    public class MessageSentEventHandler : INotificationHandler<MessageSentEvent>
    {
        private readonly IApplicationDbContext _dbContext;

        public MessageSentEventHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task Handle(MessageSentEvent notification, CancellationToken cancellationToken)
        {
            foreach (var recipientId in notification.RecipientAccountIds)
            {
                var notifResult = Notification.Create(
                    recipientId,
                    "Tin nhắn mới",
                    "Bạn có tin nhắn mới trong một nhóm trò chuyện.",
                    NotificationType.System);

                if (notifResult.IsSuccess)
                    _dbContext.Notifications.Add(notifResult.Data!);
            }

            return Task.CompletedTask;
        }
    }
}
