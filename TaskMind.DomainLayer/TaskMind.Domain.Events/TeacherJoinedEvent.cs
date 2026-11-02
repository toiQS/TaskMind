// TeacherJoinedEvent.cs
using TaskMind.DomainLayer.TaskMind.Domain.Commons.Events;

namespace TaskMind.DomainLayer.TaskMind.Domain.Events
{
    /// <summary>Phát sinh khi một User được cấp tài khoản Teacher sau khi School mời và xác minh thành công (mục 4.1.1, 4.9).</summary>
    public class TeacherJoinedEvent : DomainEvent
    {
        public Guid TeacherAccountId { get; init; }
        public Guid LinkedUserId { get; init; }
        public Guid SchoolId { get; init; }
    }
}