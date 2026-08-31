// Application Layer/TaskMind.Application.Events/SkillLevelUpRequestedEventHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Entities;
using TaskMind.Domain.Enums;
using TaskMind.Domain.Events;

namespace TaskMind.Applications.Events
{
    public class SkillLevelUpRequestedEventHandler : INotificationHandler<SkillLevelUpRequestedEvent>
    {
        private readonly IApplicationDbContext _dbContext;

        public SkillLevelUpRequestedEventHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Handle(SkillLevelUpRequestedEvent notification, CancellationToken cancellationToken)
        {
            if (notification.RequestType != SkillLevelUpMethod.Endorsement)
                return; // Assessment: không cần báo ai, User tự làm bài kiểm tra.

            var request = await _dbContext.SkillLevelUpRequests
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == notification.RequestId, cancellationToken);

            if (request == null || request.ApproverAccountId == Guid.Empty)
                return;

            var skill = await _dbContext.Skills
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == notification.SkillId, cancellationToken);

            var notifResult = Notification.Create(
                request.ApproverAccountId,
                "Yêu cầu bảo lãnh nâng cấp độ kỹ năng",
                $"Có yêu cầu bảo lãnh nâng cấp kỹ năng \"{skill?.SkillName ?? notification.SkillId.ToString()}\" cần bạn xem xét.",
                NotificationType.Approval);

            if (notifResult.IsSuccess)
                _dbContext.Notifications.Add(notifResult.Data!);
        }
    }
}