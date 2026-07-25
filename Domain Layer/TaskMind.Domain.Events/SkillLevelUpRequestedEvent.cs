// SkillLevelUpRequestedEvent.cs
using TaskMind.Domain.Commons.Events;
using TaskMind.Domain.Enums;

namespace TaskMind.Domain.Events
{
    /// <summary>Phát sinh khi User gửi yêu cầu nâng level một kỹ năng cụ thể (mục 4.3.1).</summary>
    public class SkillLevelUpRequestedEvent : DomainEvent
    {
        public Guid UserId { get; init; }
        public Guid SkillId { get; init; }
        public SkillLevel CurrentLevel { get; init; }
        public Guid RequestId { get; init; }
    }
}