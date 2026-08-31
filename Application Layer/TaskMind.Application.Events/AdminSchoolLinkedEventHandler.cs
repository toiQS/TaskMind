using MediatR;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Entities;
using TaskMind.Domain.Enums;
using TaskMind.Domain.Events;

namespace TaskMind.Applications.Events
{
    /// <summary>Xử lý khi User đăng ký thành lập cơ sở đào tạo được Admin hệ thống xác minh thành công (mục 4.1.1, 4.8).</summary>
    public class AdminSchoolLinkedEventHandler : INotificationHandler<AdminSchoolLinkedEvent>
    {
        private readonly IApplicationDbContext _dbContext;

        public AdminSchoolLinkedEventHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task Handle(AdminSchoolLinkedEvent notification, CancellationToken cancellationToken)
        {
            var notifResult = Notification.Create(
                notification.LinkedUserId,
                "Cấp quyền quản trị cơ sở đào tạo",
                "Bạn đã được cấp tài khoản Admin school sau khi hồ sơ thành lập cơ sở đào tạo được xác minh.",
                NotificationType.Approval);

            if (notifResult.IsSuccess)
                _dbContext.Notifications.Add(notifResult.Data!);

            return Task.CompletedTask;
        }
    }
}
