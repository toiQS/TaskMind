// SkillLevelUpRejectedEvent.cs
using TaskMind.DomainLayer.TaskMind.Domain.Commons.Events;
using TaskMind.DomainLayer.TaskMind.Domain.Enums;

namespace TaskMind.DomainLayer.TaskMind.Domain.Events
{
    /// <summary>Phát sinh khi yêu cầu nâng level kỹ năng bị từ chối; thường kéo theo SkillPenaltyAppliedEvent (mục 4.3.1).</summary>
    public class SkillLevelUpRejectedEvent : DomainEvent
    {
        public Guid RequestId { get; init; }
        public Guid UserId { get; init; }
        public Guid SkillId { get; init; }
        public SkillLevelUpMethod RequestType { get; init; }
        public string Reason { get; init; } = string.Empty;
    }
}