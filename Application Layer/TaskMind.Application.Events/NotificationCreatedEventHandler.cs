using MediatR;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Events;

namespace TaskMind.Applications.Events.Handlers
{
    /// <summary>
    /// Xử lý khi một Notification được tạo (mục 5.3, 4.17). Notification trong ứng dụng đã được lưu
    /// bởi Notification.Create() trước khi sự kiện này phát sinh; handler này chỉ còn trách nhiệm gửi
    /// email (kênh phụ theo mục 4.17: "Thông báo qua email và trong ứng dụng").
    /// TODO: bổ sung IEmailSender (chưa có trong TaskMind.Applications.Commons) rồi gửi email tới
    /// Profile.Email tương ứng với notification.RecipientAccountId.
    /// </summary>
    internal class NotificationCreatedEventHandler : INotificationHandler<NotificationCreatedEvent>
    {
        public Task Handle(NotificationCreatedEvent notification, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
