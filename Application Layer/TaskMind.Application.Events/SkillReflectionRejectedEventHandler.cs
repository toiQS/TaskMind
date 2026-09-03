// SkillReflectionRejectedEventHandler.cs — [MỚI - fix, mục 4.3.2, 4.3.3]
// Xử lý khi đề xuất phản ánh kỹ năng của công ty bị từ chối — do Admin dismiss ngay từ đầu (Down),
// không đạt bài kiểm tra xác minh (Up/Add), hoặc nhân sự đạt lại bài kiểm tra nên đề xuất hạ cấp
// không thành công (Down). Mọi trường hợp đều phải được ghi vào SkillHistoryEntry (mục 4.3.3) dù
// không áp dụng thay đổi nào lên SkillProfile.
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Entities;
using TaskMind.Domain.Enums;
using TaskMind.Domain.Events;

namespace TaskMind.Applications.Events
{
    public class SkillReflectionRejectedEventHandler : INotificationHandler<SkillReflectionRejectedEvent>
    {
        private readonly IApplicationDbContext _dbContext;

        public SkillReflectionRejectedEventHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Handle(SkillReflectionRejectedEvent notification, CancellationToken cancellationToken)
        {
            var request = await _dbContext.CompanySkillReflectionRequests
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == notification.RequestId, cancellationToken);

            if (request != null)
            {
                var staff = await _dbContext.Staffs
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Id == request.StaffAccountId, cancellationToken);

                var historyResult = SkillHistoryEntry.Record(
                    userId: notification.UserId,
                    skillId: notification.SkillId,
                    changeSource: SkillChangeSource.CompanyReflection,
                    responsibleAccountId: request.ResponsibleAccountId,
                    outcome: SkillHistoryOutcome.Rejected,
                    levelBefore: request.CurrentLevelAtRequest,
                    levelAfter: null,
                    companyId: request.CompanyId,
                    projectId: request.ProjectId,
                    tenureStartUtc: staff?.CreatedAtUtc,
                    tenureEndUtc: staff?.LeftAtUtc,
                    evidenceDescription: request.EvidenceDescription,
                    relatedSubmissionId: request.VerificationSubmissionId,
                    relatedRequestId: request.Id,
                    rejectionReason: notification.Reason);

                if (historyResult.IsSuccess)
                    _dbContext.SkillHistoryEntries.Add(historyResult.Data!);
            }

            var notifResult = Notification.Create(
                notification.UserId,
                "Đề xuất phản ánh kỹ năng đã bị từ chối",
                notification.ReflectionType == SkillReflectionType.Down
                    ? $"Bạn đã xác minh lại thành công (hoặc Admin hệ thống chưa chấp nhận xử lý) — cấp độ kỹ năng hiện tại được giữ nguyên. Lý do: {notification.Reason}"
                    : $"Đề xuất phản ánh kỹ năng không được áp dụng. Lý do: {notification.Reason}",
                NotificationType.System);

            if (notifResult.IsSuccess)
                _dbContext.Notifications.Add(notifResult.Data!);
        }
    }
}