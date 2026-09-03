// SkillReflectionAppliedEventHandler.cs — [MỚI - fix, mục 4.3.2, 4.3.3]
// Trước đây:
//  - Domain đã tính đúng ResultLevel/IsNewSkill nhưng KHÔNG có handler nào gọi
//    SkillProfile.ApplyCompanyReflectionResult(...) — kết quả không bao giờ chạm vào hồ sơ thật dù
//    toàn bộ workflow domain (AdminAccept/AssignTestPaper/ApplyVerificationResult) chạy đúng.
//  - SkillHistoryEntry hoàn toàn không được ghi ở luồng này (mục 4.3.3).
//
// [CẬP NHẬT - fix]
//  1) Nếu SkillProfile chưa tồn tại (thường gặp với Add — nhân sự chưa từng khai báo kỹ năng nào),
//     trước đây `profile?.ApplyCompanyReflectionResult(...)` no-op ÂM THẦM: SkillHistoryEntry vẫn ghi
//     Outcome = Applied và Notification vẫn báo "đã ghi nhận" dù hồ sơ thật sự KHÔNG thay đổi gì. Giờ
//     tự động khởi tạo SkillProfile mới nếu thiếu (SkillProfile.Create chỉ cần UserId) trước khi áp dụng.
//  2) Kiểm tra Result trả về từ ApplyCompanyReflectionResult — nếu áp dụng thất bại (vd. Up/Down nhưng
//     hồ sơ không có sẵn kỹ năng đó, một tình huống dữ liệu bất thường vì Down đã được validate có
//     currentRecord tại thời điểm tạo đề xuất), KHÔNG ghi SkillHistoryEntry với Outcome = Applied giả;
//     thay vào đó ghi Outcome = Rejected kèm lý do kỹ thuật, và Notification phản ánh đúng thực tế
//     thay vì báo thành công sai sự thật.
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

            // [MỚI - fix] Tự động khởi tạo SkillProfile nếu User chưa từng có hồ sơ kỹ năng nào —
            // trước đây trường hợp này khiến toàn bộ việc áp dụng bị bỏ qua âm thầm.
            if (profile == null)
            {
                var profileResult = SkillProfile.Create(notification.UserId);
                if (profileResult.IsSuccess)
                {
                    profile = profileResult.Data!;
                    _dbContext.SkillProfiles.Add(profile);
                }
            }

            var levelBefore = request.ReflectionType == SkillReflectionType.Down
                ? request.CurrentLevelAtRequest
                : profile?.Records.FirstOrDefault(r => r.SkillId == notification.SkillId)?.Level;

            var applyResult = profile?.ApplyCompanyReflectionResult(notification.SkillId, notification.NewLevel, notification.IsNewSkill);
            var appliedSuccessfully = applyResult?.IsSuccess ?? false;

            var staff = await _dbContext.Staffs
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == request.StaffAccountId, cancellationToken);

            if (appliedSuccessfully)
            {
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
            else
            {
                // [MỚI - fix] Áp dụng thất bại (dữ liệu bất thường: request.ApplyVerificationResult() ở
                // aggregate CompanySkillReflectionRequest đã coi là Applied, nhưng SkillProfile lại
                // không có kỹ năng tương ứng để cập nhật) — KHÔNG ghi lịch sử "Applied" giả, và báo
                // đúng thực tế cho người dùng thay vì thông báo thành công sai sự thật.
                var failureReason = applyResult?.Message ?? "Không tìm thấy hồ sơ kỹ năng để áp dụng thay đổi.";

                var historyResult = SkillHistoryEntry.Record(
                    userId: notification.UserId,
                    skillId: notification.SkillId,
                    changeSource: SkillChangeSource.CompanyReflection,
                    responsibleAccountId: request.ResponsibleAccountId,
                    outcome: SkillHistoryOutcome.Rejected,
                    levelBefore: levelBefore,
                    levelAfter: null,
                    isNewlyAdded: notification.IsNewSkill,
                    companyId: request.CompanyId,
                    projectId: request.ProjectId,
                    tenureStartUtc: staff?.CreatedAtUtc,
                    tenureEndUtc: staff?.LeftAtUtc,
                    evidenceDescription: request.EvidenceDescription,
                    relatedSubmissionId: request.VerificationSubmissionId,
                    relatedRequestId: request.Id,
                    rejectionReason: $"Lỗi kỹ thuật khi áp dụng lên hồ sơ: {failureReason}");

                if (historyResult.IsSuccess)
                    _dbContext.SkillHistoryEntries.Add(historyResult.Data!);

                var notifResult = Notification.Create(
                    notification.UserId,
                    "Không thể áp dụng thay đổi kỹ năng",
                    "Đề xuất phản ánh kỹ năng đã được xác minh thành công nhưng hệ thống gặp lỗi khi cập " +
                    "nhật hồ sơ kỹ năng của bạn. Vui lòng liên hệ hỗ trợ để được xử lý thủ công.",
                    NotificationType.Warning);

                if (notifResult.IsSuccess)
                    _dbContext.Notifications.Add(notifResult.Data!);
            }
        }
    }
}