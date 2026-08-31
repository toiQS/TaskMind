using MediatR;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Entities;
using TaskMind.Domain.Enums;
using TaskMind.Domain.Events;

namespace TaskMind.Applications.Events
{
    /// <summary>Xử lý khi User được cấp tài khoản Student sau ghi danh và xác minh thành công (mục 4.1.1, 4.9).</summary>
    public class StudentEnrolledEventHandler : INotificationHandler<StudentEnrolledEvent>
    {
        private readonly IApplicationDbContext _dbContext;

        public StudentEnrolledEventHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task Handle(StudentEnrolledEvent notification, CancellationToken cancellationToken)
        {
            var notifResult = Notification.Create(
                notification.LinkedUserId,
                "Ghi danh thành công",
                "Bạn đã được cấp tài khoản Student tại cơ sở đào tạo đã đăng ký.",
                NotificationType.Approval);

            if (notifResult.IsSuccess)
                _dbContext.Notifications.Add(notifResult.Data!);

            return Task.CompletedTask;
        }
    }
}
