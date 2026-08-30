using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Entities;
using TaskMind.Domain.Enums;
using TaskMind.Domain.Events;

namespace TaskMind.Applications.Events.Handlers
{
    /// <summary>
    /// Xử lý khi User nộp hồ sơ ứng tuyển (mục 4.18). JobPosting.CompanyId trỏ tới Company, nên phải
    /// resolve qua AdminCompany để tìm đúng người nhận Notification (tương tự CompanyVerifiedEventHandler).
    /// </summary>
    internal class JobApplicationSubmittedEventHandler : INotificationHandler<JobApplicationSubmittedEvent>
    {
        private readonly IApplicationDbContext _dbContext;

        public JobApplicationSubmittedEventHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Handle(JobApplicationSubmittedEvent notification, CancellationToken cancellationToken)
        {
            var posting = await _dbContext.JobPostings
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == notification.JobPostingId, cancellationToken);

            if (posting == null)
                return;

            var adminCompany = await _dbContext.AdminCompanies
                .AsNoTracking()
                .FirstOrDefaultAsync(ac => ac.CompanyId == posting.CompanyId, cancellationToken);

            if (adminCompany == null)
                return;

            var notifResult = Notification.Create(
                adminCompany.LinkedUserId,
                "Ứng viên mới",
                $"Có ứng viên mới ứng tuyển vào tin tuyển dụng \"{posting.Title}\".",
                NotificationType.System);

            if (notifResult.IsSuccess)
                _dbContext.Notifications.Add(notifResult.Data!);
        }
    }
}
