using Microsoft.EntityFrameworkCore;
using TaskMind.DomainLayer.TaskMind.Domain.Commons.Cores;
using TaskMind.DomainLayer.TaskMind.Domain.Commons.Result;
using TaskMind.DomainLayer.TaskMind.Domain.Enums;
using TaskMind.DomainLayer.TaskMind.Domain.Events;

namespace TaskMind.DomainLayer.TaskMind.Domain.Entities
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

        public static Result<JobApplication> Apply(JobPosting jobPosting, Guid userId)
        {
            if (jobPosting is null || jobPosting.Id == Guid.Empty)
                return Result<JobApplication>.Failure("Tin tuyển dụng không hợp lệ.");
            if (userId == Guid.Empty)
                return Result<JobApplication>.Failure("UserId không hợp lệ.");
            if (jobPosting.PostingStatus != JobPostingStatus.Open)
                return Result<JobApplication>.Failure("Chỉ có thể ứng tuyển vào tin tuyển dụng đang mở.");

            var application = new JobApplication(jobPosting.Id, userId);

            application.AddDomainEvent(new JobApplicationSubmittedEvent
            {
                JobApplicationId = application.Id,
                JobPostingId = jobPosting.Id,
                UserId = userId
            });

            return Result<JobApplication>.Success(application);
        }

        public Result UpdateStatus(ApplicationStatus newStatus)
        {
            if (ApplicationStatus is ApplicationStatus.Accepted or ApplicationStatus.Rejected)
                return Result.Failure("Hồ sơ ứng tuyển đã được xử lý xong, không thể thay đổi thêm.");

            if (!Enum.IsDefined(newStatus))
                return Result.Failure("Trạng thái hồ sơ ứng tuyển không hợp lệ.");
            if (newStatus == ApplicationStatus ||
                newStatus == ApplicationStatus.Submitted)
                return Result.Failure("Trạng thái hồ sơ ứng tuyển không thay đổi hoặc không hợp lệ.");

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
