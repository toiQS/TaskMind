using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Entities;
using TaskMind.Domain.Enums;
using TaskMind.Domain.Events;

namespace TaskMind.Applications.Events
{
    /// <summary>
    /// Xử lý khi Admin hệ thống duyệt một công ty (mục 4.4, 7.3.1). CompanyVerifiedEvent không mang
    /// LinkedUserId trực tiếp nên phải tra AdminCompany theo CompanyId để xác định người nhận Notification.
    /// </summary>
    public class CompanyVerifiedEventHandler : INotificationHandler<CompanyVerifiedEvent>
    {
        private readonly IApplicationDbContext _dbContext;

        public CompanyVerifiedEventHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Handle(CompanyVerifiedEvent notification, CancellationToken cancellationToken)
        {
            var adminCompany = await _dbContext.AdminCompanies
                .AsNoTracking()
                .FirstOrDefaultAsync(ac => ac.CompanyId == notification.CompanyId, cancellationToken);

            // TODO: AuditLog.Record(Guid.Empty /* Admin hệ thống */, "CompanyVerified", nameof(Company), notification.CompanyId)
            // khi IApplicationDbContext bổ sung DbSet<AuditLog> (mục 4.21).

            if (adminCompany == null)
                return; // Chưa có AdminCompany liên kết — xem mục 8 (vấn đề mở: LinkedUserId).

            var notifResult = Notification.Create(
                adminCompany.LinkedUserId,
                "Công ty đã được xác thực",
                $"Công ty \"{notification.CompanyName}\" của bạn đã được Admin hệ thống duyệt và có thể hoạt động đầy đủ.",
                NotificationType.Success);

            if (notifResult.IsSuccess)
                _dbContext.Notifications.Add(notifResult.Data!);
        }
    }
}
