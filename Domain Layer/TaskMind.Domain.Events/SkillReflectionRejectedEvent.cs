// SkillReflectionRejectedEvent.cs
using TaskMind.Domain.Commons.Events;
using TaskMind.Domain.Enums;

namespace TaskMind.Domain.Events
{
    /// <summary>
    /// Phát sinh khi đề xuất phản ánh kỹ năng của công ty bị từ chối — do Admin dismiss ngay từ đầu
    /// (Down), do không đạt bài kiểm tra xác minh (Up/Add), hoặc do nhân sự đạt lại bài kiểm tra nên
    /// đề xuất hạ cấp không thành công (Down) (mục 4.3.2). [MỚI - v2.1]
    /// </summary>
    public class SkillReflectionRejectedEvent : DomainEvent
    {
        public Guid RequestId { get; init; }
        public Guid UserId { get; init; }
        public Guid SkillId { get; init; }
        public SkillReflectionType ReflectionType { get; init; }
        public string Reason { get; init; } = string.Empty;
    }
}
