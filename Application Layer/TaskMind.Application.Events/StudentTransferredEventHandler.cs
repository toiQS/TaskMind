using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Entities;
using TaskMind.Domain.Enums;
using TaskMind.Domain.Events;

namespace TaskMind.Applications.Events
{
    /// <summary>Xử lý khi một Student chuyển từ cơ sở đào tạo này sang cơ sở đào tạo khác.</summary>
    public class StudentTransferredEventHandler : INotificationHandler<StudentTransferredEvent>
    {
        private readonly IApplicationDbContext _dbContext;

        public StudentTransferredEventHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Handle(StudentTransferredEvent notification, CancellationToken cancellationToken)
        {
            var newSchool = await _dbContext.Schools
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == notification.NewSchoolId, cancellationToken);

            var notifResult = Notification.Create(
                notification.LinkedUserId,
                "Chuyển cơ sở đào tạo",
                $"Bạn đã được chuyển sang cơ sở đào tạo \"{newSchool?.SchoolName ?? notification.NewSchoolId.ToString()}\".",
                NotificationType.System);

            if (notifResult.IsSuccess)
                _dbContext.Notifications.Add(notifResult.Data!);
        }
    }
}
