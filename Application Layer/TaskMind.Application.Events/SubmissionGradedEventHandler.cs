using MediatR;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Entities;
using TaskMind.Domain.Enums;
using TaskMind.Domain.Events;

namespace TaskMind.Applications.Events.Handlers
{
    /// <summary>
    /// Xử lý khi một Submission được chấm điểm (mục 4.6, 4.11, 7.3.4). Gửi Notification kết quả; việc
    /// tự động cấp Certificate khi TestPaper.OwnerType = School và đạt điểm yêu cầu (mục 7.3.4) CHƯA
    /// thực hiện được vì IApplicationDbContext hiện thiếu DbSet&lt;TestPaper&gt;/DbSet&lt;Certificate&gt;
    /// để tra cứu và gọi Certificate.Issue(...) — cần bổ sung 2 DbSet này trước.
    /// </summary>
    internal class SubmissionGradedEventHandler : INotificationHandler<SubmissionGradedEvent>
    {
        private readonly IApplicationDbContext _dbContext;

        public SubmissionGradedEventHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task Handle(SubmissionGradedEvent notification, CancellationToken cancellationToken)
        {
            var notifResult = Notification.Create(
                notification.UserId,
                "Bài kiểm tra đã được chấm điểm",
                $"Bài làm của bạn đã được chấm: {notification.Score:N1} điểm.",
                NotificationType.System);

            if (notifResult.IsSuccess)
                _dbContext.Notifications.Add(notifResult.Data!);

            // TODO (mục 7.3.4): cấp Certificate tự động khi đạt yêu cầu — xem ghi chú ở summary class trên.

            return Task.CompletedTask;
        }
    }
}
