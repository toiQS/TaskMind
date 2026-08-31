// Application Layer/TaskMind.Application.Events/NotificationCreatedEventHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Events;

namespace TaskMind.Applications.Events.Handlers
{
    /// <summary>
    /// Xử lý khi một Notification được tạo (mục 5.3, 4.17). Notification trong ứng dụng đã được lưu
    /// bởi Notification.Create() trước đó; handler này chỉ đảm nhiệm kênh phụ "gửi email" bằng cách
    /// tra Profile.Email tương ứng RecipientAccountId rồi publish SendEmailEvent (mục 4.17: "Thông báo
    /// qua email và trong ứng dụng").
    /// </summary>
    internal class NotificationCreatedEventHandler : INotificationHandler<NotificationCreatedEvent>
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly IPublisher _publisher;

        public NotificationCreatedEventHandler(IApplicationDbContext dbContext, IPublisher publisher)
        {
            _dbContext = dbContext;
            _publisher = publisher;
        }

        public async Task Handle(NotificationCreatedEvent notification, CancellationToken cancellationToken)
        {
            var email = await ResolveEmailAsync(notification.RecipientAccountId, cancellationToken);
            if (string.IsNullOrWhiteSpace(email))
                return; // Không tìm thấy tài khoản/email — bỏ qua kênh email, vẫn giữ Notification trong app.

            var fullNotification = await _dbContext.Notifications
                .AsNoTracking()
                .FirstOrDefaultAsync(n => n.Id == notification.NotificationId, cancellationToken);

            var body = fullNotification?.Message ?? notification.Title;

            await _publisher.Publish(new SendEmailEvent(email, notification.Title, body), cancellationToken);
        }

        /// <summary>Tra Email theo AccountId tham chiếu đa hình — mỗi AccountId chỉ khớp đúng 1 bảng.</summary>
        private async Task<string?> ResolveEmailAsync(Guid accountId, CancellationToken cancellationToken)
        {
            var user = await _dbContext.Users.AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == accountId, cancellationToken);
            if (user != null) return user.Profile.Email;

            var staff = await _dbContext.Staffs.AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == accountId, cancellationToken);
            if (staff != null) return staff.Profile.Email;

            var student = await _dbContext.Students.AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == accountId, cancellationToken);
            if (student != null) return student.Profile.Email;

            var teacher = await _dbContext.Teachers.AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == accountId, cancellationToken);
            if (teacher != null) return teacher.Profile.Email;

            var adminCompany = await _dbContext.AdminCompanies.AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == accountId, cancellationToken);
            if (adminCompany != null) return adminCompany.Profile.Email;

            var adminSchool = await _dbContext.AdminSchools.AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == accountId, cancellationToken);
            if (adminSchool != null) return adminSchool.Profile.Email;

            var admin = await _dbContext.Admins.AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == accountId, cancellationToken);
            return admin?.Profile.Email;
        }
    }
}