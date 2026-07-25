// SkillLevelApprovedEvent.cs
using TaskMind.Domain.Commons.Events;
using TaskMind.Domain.Enums;

namespace TaskMind.Domain.Events
{
    /// <summary>Phát sinh khi yêu cầu nâng level kỹ năng được duyệt (qua endorsement hoặc đánh giá năng lực - mục 4.3.1).</summary>
    public class SkillLevelApprovedEvent : DomainEvent
    {
        public Guid UserId { get; init; }
        public Guid SkillId { get; init; }
        public SkillLevel NewLevel { get; init; }
    }
}