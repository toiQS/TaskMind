using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Entities;
using TaskMind.Domain.Enums;
using TaskMind.Domain.Events;

namespace TaskMind.Applications.Events.Handlers
{
    /// <summary>
    /// Xử lý khi yêu cầu nâng level kỹ năng bị từ chối (mục 4.3.1). Sự kiện này thường kéo theo
    /// SkillPenaltyAppliedEvent do SkillProfile.ApplyPenaltyDowngrade phát sinh riêng ở tầng Domain —
    /// không tự trigger tại đây để tránh coupling ngược vào Domain layer.
    /// </summary>
    internal class SkillLevelUpRejectedEventHandler : INotificationHandler<SkillLevelUpRejectedEvent>
    {
        private readonly IApplicationDbContext _dbContext;

        public SkillLevelUpRejectedEventHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Handle(SkillLevelUpRejectedEvent notification, CancellationToken cancellationToken)
        {
            var skill = await _dbContext.Skills
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == notification.SkillId, cancellationToken);

            var notifResult = Notification.Create(
                notification.UserId,
                "Yêu cầu nâng cấp độ kỹ năng bị từ chối",
                $"Yêu cầu nâng cấp kỹ năng \"{skill?.SkillName ?? notification.SkillId.ToString()}\" đã bị từ chối. Lý do: {notification.Reason}",
                NotificationType.Warning);

            if (notifResult.IsSuccess)
                _dbContext.Notifications.Add(notifResult.Data!);
        }
    }
}
