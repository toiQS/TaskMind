using TaskMind.Domain.Commons.Events;

namespace TaskMind.Domain.Events
{
    /// <summary>Phát sinh khi một Student chuyển từ cơ sở đào tạo này sang cơ sở đào tạo khác.</summary>
    public class StudentTransferredEvent : DomainEvent
    {
        public Guid StudentAccountId { get; init; }
        public Guid LinkedUserId { get; init; }
        public Guid OldSchoolId { get; init; }
        public Guid NewSchoolId { get; init; }
    }
}