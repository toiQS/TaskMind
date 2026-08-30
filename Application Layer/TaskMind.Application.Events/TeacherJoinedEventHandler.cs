using MediatR;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Entities;
using TaskMind.Domain.Enums;
using TaskMind.Domain.Events;

namespace TaskMind.Applications.Events.Handlers
{
    /// <summary>Xử lý khi User được cấp tài khoản Teacher sau khi cơ sở đào tạo mời và xác minh thành công (mục 4.1.1, 4.9).</summary>
    internal class TeacherJoinedEventHandler : INotificationHandler<TeacherJoinedEvent>
    {
        private readonly IApplicationDbContext _dbContext;

        public TeacherJoinedEventHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task Handle(TeacherJoinedEvent notification, CancellationToken cancellationToken)
        {
            var notifResult = Notification.Create(
                notification.LinkedUserId,
                "Gia nhập cơ sở đào tạo",
                "Bạn đã được cấp tài khoản Teacher sau khi cơ sở đào tạo xác minh thành công.",
                NotificationType.Approval);

            if (notifResult.IsSuccess)
                _dbContext.Notifications.Add(notifResult.Data!);

            return Task.CompletedTask;
        }
    }
}
