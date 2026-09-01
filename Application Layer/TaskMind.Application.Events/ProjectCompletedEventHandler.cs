using MediatR;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Entities;
using TaskMind.Domain.Enums;
using TaskMind.Domain.Events;

namespace TaskMind.Applications.Events
{
    /// <summary>
    /// Xử lý khi một dự án hoàn thành (mục 6 - DDD note). Gửi Notification tới từng thành viên.
    /// Nếu IsExchangeProject = true, việc tạo Invoice được điều phối riêng qua luồng
    /// ExchangeContract.Complete() -> ExchangeContractCompletedEvent (xem ExchangeContractCompletedEventHandler),
    /// không xử lý tại đây để tránh trùng lặp trách nhiệm giữa hai aggregate.
    /// </summary>
    public class ProjectCompletedEventHandler : INotificationHandler<ProjectCompletedEvent>
    {
        private readonly IApplicationDbContext _dbContext;

        public ProjectCompletedEventHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task Handle(ProjectCompletedEvent notification, CancellationToken cancellationToken)
        {
            foreach (var accountId in notification.MemberAccountIds)
            {
                var notifResult = Notification.Create(
                    accountId,
                    "Dự án đã hoàn thành",
                    "Dự án bạn tham gia đã hoàn thành. Lịch sử tham gia đã được ghi nhận vào hồ sơ của bạn.",
                    NotificationType.System);

                if (notifResult.IsSuccess)
                    _dbContext.Notifications.Add(notifResult.Data!);
            }

            // TODO: cập nhật SkillProfile của từng thành viên — cần cơ chế map ProjectRole -> Skill liên quan,
            // hiện chưa được quy định trong tài liệu nghiệp vụ.

            return Task.CompletedTask;
        }
    }
}
