// SkillLevelUpRequestedEvent.cs
using TaskMind.DomainLayer.TaskMind.Domain.Commons.Events;
using TaskMind.DomainLayer.TaskMind.Domain.Enums;

namespace TaskMind.DomainLayer.TaskMind.Domain.Events
{
    /// <summary>Phát sinh khi User gửi yêu cầu nâng level một kỹ năng cụ thể (mục 4.3.1). [CẬP NHẬT] thêm RequestType.</summary>
    public class SkillLevelUpRequestedEvent : DomainEvent
    {
        public Guid UserId { get; init; }
        public Guid SkillId { get; init; }
        public SkillLevel CurrentLevel { get; init; }
        public Guid RequestId { get; init; }
        public SkillLevelUpMethod RequestType { get; init; }
    }
}
