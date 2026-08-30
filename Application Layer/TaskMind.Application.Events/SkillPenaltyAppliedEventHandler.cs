using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Entities;
using TaskMind.Domain.Enums;
using TaskMind.Domain.Events;

namespace TaskMind.Applications.Events.Handlers
{
    /// <summary>Xử lý khi hệ thống hạ level kỹ năng do xác minh không đạt, mức phạt x2 (mục 4.3.1).</summary>
    internal class SkillPenaltyAppliedEventHandler : INotificationHandler<SkillPenaltyAppliedEvent>
    {
        private readonly IApplicationDbContext _dbContext;

        public SkillPenaltyAppliedEventHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Handle(SkillPenaltyAppliedEvent notification, CancellationToken cancellationToken)
        {
            var skill = await _dbContext.Skills
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == notification.SkillId, cancellationToken);

            var notifResult = Notification.Create(
                notification.UserId,
                "Cảnh báo: kỹ năng bị hạ cấp",
                $"Kỹ năng \"{skill?.SkillName ?? notification.SkillId.ToString()}\" của bạn đã bị hạ từ {notification.PreviousLevel} " +
                $"xuống {notification.NewLevel} (mức phạt x{notification.PenaltyMultiplier}) do không xác minh/không đạt yêu cầu. " +
                "Đây được ghi nhận như cảnh báo chính thức trên tài khoản.",
                NotificationType.Warning);

            if (notifResult.IsSuccess)
                _dbContext.Notifications.Add(notifResult.Data!);

            // TODO (mục 8 - vấn đề mở): chưa có quy tắc đếm số lần cảnh báo tối đa trước khi khoá/hạn chế tài khoản.
        }
    }
}
