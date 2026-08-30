// ReviewCreatedEvent.cs
using TaskMind.Domain.Commons.Events;
using TaskMind.Domain.Enums;

namespace TaskMind.Domain.Events
{
    /// <summary>Phát sinh khi một Review mới được tạo; dùng để cập nhật điểm uy tín tổng hợp (mục 4.19).</summary>
    public class ReviewCreatedEvent : DomainEvent
    {
        public Guid ReviewId { get; init; }
        public ReviewTargetType TargetType { get; init; }
        public Guid TargetRefId { get; init; }
        public int Rating { get; init; }
    }
}
