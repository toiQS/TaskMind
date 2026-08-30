// JobApplicationStatusChangedEvent.cs
using TaskMind.Domain.Commons.Events;
using TaskMind.Domain.Enums;

namespace TaskMind.Domain.Events
{
    /// <summary>Phát sinh khi trạng thái hồ sơ ứng tuyển thay đổi; kích hoạt Notification cho ứng viên (mục 4.18).</summary>
    public class JobApplicationStatusChangedEvent : DomainEvent
    {
        public Guid JobApplicationId { get; init; }
        public Guid UserId { get; init; }
        public ApplicationStatus NewStatus { get; init; }
    }
}
