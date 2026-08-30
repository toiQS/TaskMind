// MessageSentEvent.cs
using TaskMind.Domain.Commons.Events;

namespace TaskMind.Domain.Events
{
    /// <summary>Phát sinh khi một Message mới được gửi trong Chat; kích hoạt Notification tới các thành viên còn lại (mục 4.22, 4.17).</summary>
    public class MessageSentEvent : DomainEvent
    {
        public Guid MessageId { get; init; }
        public Guid ChatId { get; init; }
        public Guid SenderAccountId { get; init; }
        public Guid[] RecipientAccountIds { get; init; } = Array.Empty<Guid>();
    }
}
