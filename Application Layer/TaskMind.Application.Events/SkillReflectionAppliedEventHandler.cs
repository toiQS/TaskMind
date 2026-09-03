// SkillReflectionAppliedEventHandler.cs — [MỚI - fix, mục 4.3.2, 4.3.3]
// Trước đây:
//  - Domain đã tính đúng ResultLevel/IsNewSkill nhưng KHÔNG có handler nào gọi
//    SkillProfile.ApplyCompanyReflectionResult(...) — kết quả không bao giờ chạm vào hồ sơ thật dù
//    toàn bộ workflow domain (AdminAccept/AssignTestPaper/ApplyVerificationResult) chạy đúng.
//  - SkillHistoryEntry hoàn toàn không được ghi ở luồng này (mục 4.3.3).
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Entities;
using TaskMind.Domain.Enums;
using TaskMind.Domain.Events;

namespace TaskMind.Applications.Events
{
    public class SkillReflectionAppliedEventHandler : INotificationHandler<SkillReflectionAppliedEvent>
    {
        private readonly IApplicationDbContext _dbContext;

        public SkillReflectionAppliedEventHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Handle(SkillReflectionAppliedEvent notification, CancellationToken cancellationToken)
        {
            var request = await _dbContext.CompanySkillReflectionRequests
                .FirstOrDefaultAsync(r => r.Id == notification.RequestId, cancellationToken);

            if (request == null)
                return; // Dữ liệu bất thường — không đủ ngữ cảnh để áp dụng/ghi lịch sử an toàn.

            var profile = await _dbContext.SkillProfiles
                .FirstOrDefaultAsync(p => p.UserId == notification.UserId, cancellationToken);

            var levelBefore = request.ReflectionType == SkillReflectionType.Down
                ? request.CurrentLevelAtRequest
                : profile?.Records.FirstOrDefault(r => r.SkillId == notification.SkillId)?.Level;

            // Áp dụng chính thức lên hồ sơ kỹ năng (mục 4.3.2) — trước đây bước này bị thiếu hoàn toàn.
            profile?.ApplyCompanyReflectionResult(notification.SkillId, notification.NewLevel, notification.IsNewSkill);

            var staff = await _dbContext.Staffs
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == request.StaffAccountId, cancellationToken);

            var historyResult = SkillHistoryEntry.Record(
                userId: notification.UserId,
                skillId: notification.SkillId,
                changeSource: SkillChangeSource.CompanyReflection,
                responsibleAccountId: request.ResponsibleAccountId,
                outcome: SkillHistoryOutcome.Applied,
                levelBefore: levelBefore,
                levelAfter: notification.NewLevel,
                isNewlyAdded: notification.IsNewSkill,
                companyId: request.CompanyId,
                projectId: request.ProjectId,
                tenureStartUtc: staff?.CreatedAtUtc,
                tenureEndUtc: staff?.LeftAtUtc,
                evidenceDescription: request.EvidenceDescription,
                relatedSubmissionId: request.VerificationSubmissionId,
                relatedRequestId: request.Id);

            if (historyResult.IsSuccess)
                _dbContext.SkillHistoryEntries.Add(historyResult.Data!);

            var title = notification.ReflectionType switch
            {
                SkillReflectionType.Down => "Kỹ năng của bạn đã bị hạ cấp",
                SkillReflectionType.Add => "Kỹ năng mới đã được ghi nhận vào hồ sơ",
                _ => "Kỹ năng của bạn đã được nâng cấp"
            };

            var message = notification.ReflectionType == SkillReflectionType.Down
                ? $"Sau xác minh, kỹ năng của bạn đã chính thức bị hạ xuống mức {notification.NewLevel}, theo đề xuất từ công ty."
                : $"Bạn đã hoàn thành xác minh và kỹ năng đã được ghi nhận ở mức {notification.NewLevel}.";

            var notifResult = Notification.Create(
                notification.UserId, title, message,
                notification.ReflectionType == SkillReflectionType.Down ? NotificationType.Warning : NotificationType.Success);

            if (notifResult.IsSuccess)
                _dbContext.Notifications.Add(notifResult.Data!);
        }
    }
}