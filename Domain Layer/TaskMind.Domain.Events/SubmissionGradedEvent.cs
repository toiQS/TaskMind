// SubmissionGradedEvent.cs
using TaskMind.Domain.Commons.Events;

namespace TaskMind.Domain.Events
{
    /// <summary>Phát sinh khi một Submission được chấm điểm; có thể kích hoạt cấp Certificate hoặc cập nhật SkillLevelUpRequest liên kết (mục 4.6, 4.11, 4.3.1).</summary>
    public class SubmissionGradedEvent : DomainEvent
    {
        public Guid SubmissionId { get; init; }
        public Guid TestPaperId { get; init; }
        public Guid UserId { get; init; }
        public decimal Score { get; init; }
    }
}
