// SkillReflectionAdminAcceptedEventHandler.cs — [MỚI - fix, mục 4.3.2]
// Trước đây SkillReflectionAdminAcceptedEvent hoàn toàn không có handler — nhân sự không biết mình
// cần chuẩn bị làm lại bài kiểm tra xác minh sau khi Admin chấp nhận xử lý đề xuất hạ cấp.
using MediatR;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Entities;
using TaskMind.Domain.Enums;
using TaskMind.Domain.Events;

namespace TaskMind.Applications.Events
{
    public class SkillReflectionAdminAcceptedEventHandler : INotificationHandler<SkillReflectionAdminAcceptedEvent>
    {
        private readonly IApplicationDbContext _dbContext;

        public SkillReflectionAdminAcceptedEventHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task Handle(SkillReflectionAdminAcceptedEvent notification, CancellationToken cancellationToken)
        {
            var notifResult = Notification.Create(
                notification.UserId,
                "Cần làm lại bài kiểm tra xác minh kỹ năng",
                "Admin hệ thống đã chấp nhận xem xét đề xuất hạ cấp một kỹ năng của bạn. Bạn cần hoàn " +
                "thành lại bài kiểm tra ở cấp độ hiện tại; nếu đạt, cấp độ được giữ nguyên và đề xuất bị từ chối.",
                NotificationType.Warning);

            if (notifResult.IsSuccess)
                _dbContext.Notifications.Add(notifResult.Data!);

            return Task.CompletedTask;
        }
    }
}