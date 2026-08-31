using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Entities;
using TaskMind.Domain.Enums;
using TaskMind.Domain.Events;

namespace TaskMind.Applications.Events.Handlers
{
    /// <summary>Xử lý khi Admin hệ thống duyệt một cơ sở đào tạo (mục 4.8, 7.3.1) — tương tự CompanyVerifiedEventHandler.</summary>
    public class SchoolVerifiedEventHandler : INotificationHandler<SchoolVerifiedEvent>
    {
        private readonly IApplicationDbContext _dbContext;

        public SchoolVerifiedEventHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Handle(SchoolVerifiedEvent notification, CancellationToken cancellationToken)
        {
            var adminSchool = await _dbContext.AdminSchools
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.SchoolId == notification.SchoolId, cancellationToken);

            // TODO: AuditLog.Record(Guid.Empty, "SchoolVerified", nameof(School), notification.SchoolId)

            if (adminSchool == null)
                return;

            var notifResult = Notification.Create(
                adminSchool.LinkedUserId,
                "Cơ sở đào tạo đã được xác thực",
                $"Cơ sở đào tạo \"{notification.SchoolName}\" của bạn đã được Admin hệ thống duyệt và có thể hoạt động đầy đủ.",
                NotificationType.Success);

            if (notifResult.IsSuccess)
                _dbContext.Notifications.Add(notifResult.Data!);
        }
    }
}
