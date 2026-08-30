using Microsoft.EntityFrameworkCore;
using TaskMind.Domain.Commons.Cores;
using TaskMind.Domain.Commons.Result;
using TaskMind.Domain.Enums;
using TaskMind.Domain.Events;

namespace TaskMind.Domain.Entities
{
    /// <summary>Aggregate Root JobApplication [MỚI] — hồ sơ User ứng tuyển vào một JobPosting (mục 4.18).</summary>
    [Index(nameof(JobPostingId), nameof(UserId), IsUnique = true)]
    [Index(nameof(UserId), nameof(ApplicationStatus))]
    public class JobApplication : AggregateRoot
    {
        public Guid JobPostingId { get; private set; }
        public Guid UserId { get; private set; }

        /// <summary>Đặt tên khác "Status" để không che khuất EntityBase.Status (EntityStatus).</summary>
        public ApplicationStatus ApplicationStatus { get; private set; } = ApplicationStatus.Submitted;
        public DateTime AppliedAtUtc { get; private set; } = DateTime.UtcNow;

        private JobApplication() { }

        private JobApplication(Guid jobPostingId, Guid userId)
        {
            JobPostingId = jobPostingId;
            UserId = userId;
        }

        public static Result<JobApplication> Apply(Guid jobPostingId, Guid userId)
        {
            if (jobPostingId == Guid.Empty)
                return Result<JobApplication>.Failure("JobPostingId không hợp lệ.");
            if (userId == Guid.Empty)
                return Result<JobApplication>.Failure("UserId không hợp lệ.");

            var application = new JobApplication(jobPostingId, userId);

            application.AddDomainEvent(new JobApplicationSubmittedEvent
            {
                JobApplicationId = application.Id,
                JobPostingId = jobPostingId,
                UserId = userId
            });

            return Result<JobApplication>.Success(application);
        }

        public Result UpdateStatus(ApplicationStatus newStatus)
        {
            if (ApplicationStatus is ApplicationStatus.Accepted or ApplicationStatus.Rejected)
                return Result.Failure("Hồ sơ ứng tuyển đã được xử lý xong, không thể thay đổi thêm.");

            ApplicationStatus = newStatus;

            AddDomainEvent(new JobApplicationStatusChangedEvent
            {
                JobApplicationId = Id,
                UserId = UserId,
                NewStatus = newStatus
            });

            return Result.Success();
        }
    }
}
