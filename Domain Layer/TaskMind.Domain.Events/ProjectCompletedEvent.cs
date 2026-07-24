using TaskMind.Domain.Commons.Events;

namespace TaskMind.Domain.Events
{
    /// <summary>
    /// Phát sinh khi một dự án chuyển sang trạng thái Completed.
    /// Theo ghi chú mục 6 (DDD): event này kích hoạt cập nhật SkillProfile của các thành viên
    /// và tạo Invoice nếu là dự án trao đổi (Exchange & Billing context).
    /// </summary>
    public class ProjectCompletedEvent : DomainEvent
    {
        public Guid ProjectId { get; init; }
        public Guid[] MemberAccountIds { get; init; } = Array.Empty<Guid>();
        public bool IsExchangeProject { get; init; }
    }
}