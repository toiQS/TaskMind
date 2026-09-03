// CompanySkillReflectionRequestedEventHandler.cs — [MỚI - fix, mục 4.3.2]
// Trước đây CompanySkillReflectionRequestedEvent không có handler nào — nhân sự bị đánh giá không hề
// biết có đề xuất liên quan tới mình cho tới khi có kết quả cuối cùng.
using MediatR;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Entities;
using TaskMind.Domain.Enums;
using TaskMind.Domain.Events;

namespace TaskMind.Applications.Events
{
    public class CompanySkillReflectionRequestedEventHandler : INotificationHandler<CompanySkillReflectionRequestedEvent>
    {
        private readonly IApplicationDbContext _dbContext;

        public CompanySkillReflectionRequestedEventHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task Handle(CompanySkillReflectionRequestedEvent notification, CancellationToken cancellationToken)
        {
            var title = notification.ReflectionType switch
            {
                SkillReflectionType.Up => "Công ty đề xuất nâng cấp độ kỹ năng",
                SkillReflectionType.Down => "Công ty đề xuất hạ cấp độ kỹ năng",
                SkillReflectionType.Add => "Công ty đề xuất bổ sung kỹ năng mới",
                _ => "Có đề xuất phản ánh kỹ năng liên quan đến bạn"
            };

            var message = notification.ReflectionType == SkillReflectionType.Down
                ? "Công ty bạn từng/đang làm việc đã gửi đề xuất hạ cấp một kỹ năng của bạn. Đề xuất đang chờ Admin hệ thống xem xét trước khi tổ chức xác minh lại."
                : "Công ty bạn đang làm việc đã gửi đề xuất phản ánh kỹ năng cho bạn. Bạn sẽ cần hoàn thành một bài kiểm tra xác minh để đề xuất chính thức có hiệu lực.";

            var notifResult = Notification.Create(notification.UserId, title, message, NotificationType.System);
            if (notifResult.IsSuccess)
                _dbContext.Notifications.Add(notifResult.Data!);

            return Task.CompletedTask;
        }
    }
}