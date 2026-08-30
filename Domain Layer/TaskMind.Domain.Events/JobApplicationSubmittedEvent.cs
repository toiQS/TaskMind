// JobApplicationSubmittedEvent.cs
using TaskMind.Domain.Commons.Events;

namespace TaskMind.Domain.Events
{
    /// <summary>Phát sinh khi User nộp hồ sơ ứng tuyển vào một JobPosting (mục 4.18).</summary>
    public class JobApplicationSubmittedEvent : DomainEvent
    {
        public Guid JobApplicationId { get; init; }
        public Guid JobPostingId { get; init; }
        public Guid UserId { get; init; }
    }
}
