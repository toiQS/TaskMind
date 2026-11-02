// StaffJoinedEvent.cs
using TaskMind.DomainLayer.TaskMind.Domain.Commons.Events;

namespace TaskMind.DomainLayer.TaskMind.Domain.Events
{
    /// <summary>Phát sinh khi một User được cấp tài khoản Staff sau khi Company mời và xác minh thành công (mục 4.1.1, 4.5).</summary>
    public class StaffJoinedEvent : DomainEvent
    {
        public Guid StaffAccountId { get; init; }
        public Guid LinkedUserId { get; init; }
        public Guid CompanyId { get; init; }
    }
}