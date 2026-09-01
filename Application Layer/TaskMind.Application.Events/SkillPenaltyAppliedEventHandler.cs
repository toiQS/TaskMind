using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Entities;
using TaskMind.Domain.Enums;
using TaskMind.Domain.Events;

namespace TaskMind.Applications.Events
{
    /// <summary>Xử lý khi hệ thống hạ level kỹ năng do xác minh không đạt, mức phạt x2 (mục 4.3.1).</summary>
    public class SkillPenaltyAppliedEventHandler : INotificationHandler<SkillPenaltyAppliedEvent>
    {
        private readonly IApplicationDbContext _dbContext;

        public SkillPenaltyAppliedEventHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Handle(SkillPenaltyAppliedEvent notification, CancellationToken cancellationToken)
        {
            var skill = await _dbContext.Skills.AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == notification.SkillId, cancellationToken);

            var notifResult = Notification.Create(
                notification.UserId,
                "Cảnh báo: kỹ năng bị hạ cấp",
                $"Kỹ năng \"{skill?.SkillName ?? notification.SkillId.ToString()}\" của bạn đã bị hạ từ {notification.PreviousLevel} " +
                $"xuống {notification.NewLevel} (mức phạt x{notification.PenaltyMultiplier}) do không xác minh/không đạt yêu cầu.",
                NotificationType.Warning);

            if (notifResult.IsSuccess)
                _dbContext.Notifications.Add(notifResult.Data!);

            // (mục 8 - vấn đề mở) Tài liệu chưa quy định ngưỡng cảnh báo tối đa. Áp dụng TẠM quy tắc:
            // tạm khoá tài khoản (Paused) sau 3 cảnh báo hạ cấp — cần Product/Admin xác nhận lại con số này.
            const int maxWarningsBeforeSuspend = 3;

            var warningCount = await _dbContext.Notifications.AsNoTracking()
                .CountAsync(n => n.RecipientAccountId == notification.UserId
                               && n.Type == NotificationType.Warning
                               && n.Title == "Cảnh báo: kỹ năng bị hạ cấp", cancellationToken);

            if (warningCount + 1 >= maxWarningsBeforeSuspend)
            {
                var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == notification.UserId, cancellationToken);
                if (user != null && user.Status != EntityStatus.Blocked)
                {
                    user.UpdateStatus(EntityStatus.Paused);

                    var suspendNotif = Notification.Create(
                        notification.UserId,
                        "Tài khoản bị tạm khoá",
                        $"Tài khoản của bạn đã bị tạm khoá do nhận đủ {maxWarningsBeforeSuspend} cảnh báo hạ cấp kỹ năng.",
                        NotificationType.Warning);

                    if (suspendNotif.IsSuccess)
                        _dbContext.Notifications.Add(suspendNotif.Data!);
                }
            }
        }
    }
}
