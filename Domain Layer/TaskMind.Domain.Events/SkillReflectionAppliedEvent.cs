// SkillReflectionAppliedEvent.cs
using TaskMind.Domain.Commons.Events;
using TaskMind.Domain.Enums;

namespace TaskMind.Domain.Events
{
    /// <summary>
    /// Phát sinh khi đề xuất phản ánh kỹ năng của công ty được chính thức áp dụng lên SkillProfile
    /// sau khi xác minh qua bài kiểm tra hệ thống (mục 4.3.2). [MỚI - v2.1]
    /// </summary>
    public class SkillReflectionAppliedEvent : DomainEvent
    {
        public Guid RequestId { get; init; }
        public Guid UserId { get; init; }
        public Guid SkillId { get; init; }
        public SkillLevel NewLevel { get; init; }
        public bool IsNewSkill { get; init; }
        public SkillReflectionType ReflectionType { get; init; }
    }
}
