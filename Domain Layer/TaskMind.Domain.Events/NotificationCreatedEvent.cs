// NotificationCreatedEvent.cs
using TaskMind.Domain.Commons.Events;
using TaskMind.Domain.Enums;

namespace TaskMind.Domain.Events
{
    public class NotificationCreatedEvent : DomainEvent
    {
        public Guid NotificationId { get; init; }
        public Guid RecipientAccountId { get; init; }
        public string Title { get; init; } = string.Empty;
        public NotificationType Type { get; init; }
    }
}