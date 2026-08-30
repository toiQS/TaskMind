using MediatR;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Entities;
using TaskMind.Domain.Enums;
using TaskMind.Domain.Events;

namespace TaskMind.Applications.Events.Handlers
{
    /// <summary>Xử lý khi hệ thống cấp chứng chỉ điện tử cho User (mục 4.20).</summary>
    internal class CertificateIssuedEventHandler : INotificationHandler<CertificateIssuedEvent>
    {
        private readonly IApplicationDbContext _dbContext;

        public CertificateIssuedEventHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task Handle(CertificateIssuedEvent notification, CancellationToken cancellationToken)
        {
            var notifResult = Notification.Create(
                notification.UserId,
                "Chứng chỉ mới",
                $"Bạn đã được cấp chứng chỉ điện tử. Mã xác minh: {notification.VerificationCode}.",
                NotificationType.Success);

            if (notifResult.IsSuccess)
                _dbContext.Notifications.Add(notifResult.Data!);

            return Task.CompletedTask;
        }
    }
}
