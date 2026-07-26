using TaskMind.Domain.Commons.Events;

namespace TaskMind.Domain.Events
{
    /// <summary>Phát sinh khi một User được cấp tài khoản Student sau khi ghi danh và xác minh thành công tại một School (mục 4.1.1, 4.9).</summary>
    public class StudentEnrolledEvent : DomainEvent
    {
        public Guid StudentAccountId { get; init; }
        public Guid LinkedUserId { get; init; }
        public Guid SchoolId { get; init; }
    }
}