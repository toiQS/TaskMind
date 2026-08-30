using MediatR;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Entities;
using TaskMind.Domain.Enums;
using TaskMind.Domain.Events;

namespace TaskMind.Applications.Events.Handlers
{
    /// <summary>Xử lý khi User đăng ký thành lập công ty được Admin hệ thống xác minh thành công (mục 4.1.1, 4.4).</summary>
    internal class AdminCompanyLinkedEventHandler : INotificationHandler<AdminCompanyLinkedEvent>
    {
        private readonly IApplicationDbContext _dbContext;

        public AdminCompanyLinkedEventHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task Handle(AdminCompanyLinkedEvent notification, CancellationToken cancellationToken)
        {
            var notifResult = Notification.Create(
                notification.LinkedUserId,
                "Cấp quyền quản trị công ty",
                "Bạn đã được cấp tài khoản Admin company sau khi hồ sơ thành lập công ty được xác minh.",
                NotificationType.Approval);

            if (notifResult.IsSuccess)
                _dbContext.Notifications.Add(notifResult.Data!);

            return Task.CompletedTask;
        }
    }
}
