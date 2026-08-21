using Microsoft.EntityFrameworkCore;
using TaskMind.Domain.Commons.Cores;
using TaskMind.Domain.Commons.Result;
using TaskMind.Domain.Enums;
using TaskMind.Domain.Events;

namespace TaskMind.Domain.Entities
{
    /// <summary>Thông báo hệ thống gửi tới một tài khoản cụ thể (mục 5.3), Aggregate Root độc lập, giao tiếp qua domain event (mục 6).</summary>
    [Index(nameof(RecipientAccountId), nameof(IsRead))]
    [Index(nameof(RecipientAccountId), nameof(CreatedAtUtc))]
    public class Notification : AggregateRoot
    {
        public Guid RecipientAccountId { get; private set; }
        public string Title { get; private set; } = string.Empty;
        public string Message { get; private set; } = string.Empty;
        public NotificationType Type { get; private set; } = NotificationType.System;
        public bool IsRead { get; private set; }
        public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;

        private Notification() { }

        private Notification(Guid recipientAccountId, string title, string message, NotificationType type)
        {
            RecipientAccountId = recipientAccountId;
            Title = title;
            Message = message;
            Type = type;
        }

        public static Result<Notification> Create(Guid recipientAccountId, string title, string message, NotificationType type = NotificationType.System)
        {
            if (recipientAccountId == Guid.Empty)
                return Result<Notification>.Failure("RecipientAccountId không hợp lệ.");
            if (string.IsNullOrWhiteSpace(title))
                return Result<Notification>.Failure("Tiêu đề thông báo không được để trống.");

            var notification = new Notification(recipientAccountId, title.Trim(), message?.Trim() ?? string.Empty, type);
            notification.AddDomainEvent(new NotificationCreatedEvent
            {
                NotificationId = notification.Id,
                RecipientAccountId = recipientAccountId,
                Title = notification.Title,
                Type = type
            });
            return Result<Notification>.Success(notification);
        }

        public Result MarkAsRead()
        {
            if (IsRead) return Result.Failure("Thông báo đã được đọc trước đó.");
            IsRead = true;
            return Result.Success();
        }
    }
}
