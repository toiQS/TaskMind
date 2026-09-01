// SkillReflectionAdminAcceptedEvent.cs
using TaskMind.Domain.Commons.Events;

namespace TaskMind.Domain.Events
{
    /// <summary>
    /// Phát sinh khi Admin hệ thống chấp nhận xử lý một đề xuất hạ cấp (Down), chuyển đề xuất sang
    /// trạng thái chờ nhân sự làm lại bài kiểm tra xác minh (mục 4.3.2). [MỚI - v2.1]
    /// </summary>
    public class SkillReflectionAdminAcceptedEvent : DomainEvent
    {
        public Guid RequestId { get; init; }
        public Guid UserId { get; init; }
        public Guid SkillId { get; init; }
        public Guid ApproverAdminId { get; init; }
    }
}
