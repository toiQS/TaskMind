using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Entities;
using TaskMind.Domain.Enums;
using TaskMind.Domain.Events;

namespace TaskMind.Applications.Events.Handlers
{
    /// <summary>Xử lý khi yêu cầu nâng level kỹ năng được duyệt (mục 4.3.1).</summary>
    internal class SkillLevelApprovedEventHandler : INotificationHandler<SkillLevelApprovedEvent>
    {
        private readonly IApplicationDbContext _dbContext;

        public SkillLevelApprovedEventHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Handle(SkillLevelApprovedEvent notification, CancellationToken cancellationToken)
        {
            var skill = await _dbContext.Skills
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == notification.SkillId, cancellationToken);

            var notifResult = Notification.Create(
                notification.UserId,
                "Nâng cấp độ kỹ năng thành công",
                $"Kỹ năng \"{skill?.SkillName ?? notification.SkillId.ToString()}\" của bạn đã được nâng lên mức {notification.NewLevel}.",
                NotificationType.Success);

            if (notifResult.IsSuccess)
                _dbContext.Notifications.Add(notifResult.Data!);
        }
    }
}
