// SkillLevelUpRejectedEvent.cs
using TaskMind.Domain.Commons.Events;

namespace TaskMind.Domain.Events
{
    /// <summary>Phát sinh khi yêu cầu nâng level kỹ năng bị từ chối; thường kéo theo SkillPenaltyAppliedEvent (mục 4.3.1).</summary>
    public class SkillLevelUpRejectedEvent : DomainEvent
    {
        public Guid UserId { get; init; }
        public Guid SkillId { get; init; }
        public string Reason { get; init; } = string.Empty;
    }
}