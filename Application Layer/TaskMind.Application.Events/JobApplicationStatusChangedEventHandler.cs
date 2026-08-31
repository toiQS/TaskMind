using MediatR;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Entities;
using TaskMind.Domain.Enums;
using TaskMind.Domain.Events;

namespace TaskMind.Applications.Events.Handlers
{
    /// <summary>Xử lý khi trạng thái hồ sơ ứng tuyển thay đổi; thông báo cho ứng viên (mục 4.18).</summary>
    public class JobApplicationStatusChangedEventHandler : INotificationHandler<JobApplicationStatusChangedEvent>
    {
        private readonly IApplicationDbContext _dbContext;

        public JobApplicationStatusChangedEventHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task Handle(JobApplicationStatusChangedEvent notification, CancellationToken cancellationToken)
        {
            var notifResult = Notification.Create(
                notification.UserId,
                "Cập nhật trạng thái ứng tuyển",
                $"Hồ sơ ứng tuyển của bạn đã chuyển sang trạng thái: {notification.NewStatus}.",
                notification.NewStatus == ApplicationStatus.Accepted ? NotificationType.Success : NotificationType.System);

            if (notifResult.IsSuccess)
                _dbContext.Notifications.Add(notifResult.Data!);

            return Task.CompletedTask;
        }
    }
}
