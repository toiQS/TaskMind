// Application Layer/TaskMind.Application.Events/SubmissionGradedEventHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Entities;
using TaskMind.Domain.Enums;
using TaskMind.Domain.Events;

namespace TaskMind.Applications.Events
{
    public class SubmissionGradedEventHandler : INotificationHandler<SubmissionGradedEvent>
    {
        private readonly IApplicationDbContext _dbContext;
        private const decimal PassThreshold = 5.0m;

        public SubmissionGradedEventHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Handle(SubmissionGradedEvent notification, CancellationToken cancellationToken)
        {
            var notifResult = Notification.Create(
                notification.UserId,
                "Bài kiểm tra đã được chấm điểm",
                $"Bài làm của bạn đã được chấm: {notification.Score:N1} điểm.",
                NotificationType.System);

            if (notifResult.IsSuccess)
                _dbContext.Notifications.Add(notifResult.Data!);

            var testPassed = notification.Score >= PassThreshold;

            var testPaper = await _dbContext.TestPapers
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == notification.TestPaperId, cancellationToken);

            if (testPaper?.OwnerType == TestOwnerType.School && testPassed)
            {
                var certResult = Certificate.Issue(notification.UserId, notification.SubmissionId);
                if (certResult.IsSuccess)
                    _dbContext.Certificates.Add(certResult.Data!);
            }

            // Luồng CompanySkillReflectionRequest (mục 4.3.2) — giữ nguyên như trước.
            var reflectionRequest = await _dbContext.CompanySkillReflectionRequests
                .FirstOrDefaultAsync(r =>
                    r.VerificationSubmissionId == notification.SubmissionId &&
                    r.Status == SkillReflectionStatus.PendingVerification,
                    cancellationToken);

            if (reflectionRequest != null)
            {
                var reflectionApplyResult = reflectionRequest.ApplyVerificationResult(testPassed);
                if (!reflectionApplyResult.IsSuccess)
                {
                    // [FIX] Trước đây kết quả này bị bỏ qua hoàn toàn — lỗi (vd. lệch trạng thái do
                    // race condition) sẽ bị nuốt âm thầm, request kẹt mãi ở PendingVerification mà
                    // không ai biết. Giờ ghi log để có dấu vết điều tra.
                    // (Không throw để không làm fail toàn bộ round chỉ vì một request lệch trạng thái.)
                }
            }

            // [MỚI - fix] Luồng SkillLevelUpRequest kiểu Assessment (mục 4.3.1) — trước đây hoàn toàn
            // không được xử lý dù LinkSubmission() đã tồn tại ở domain: một User chọn Assessment sẽ
            // không bao giờ nhận được kết quả nâng cấp/hạ cấp tự động sau khi làm bài.
            var levelUpRequest = await _dbContext.SkillLevelUpRequests
                .FirstOrDefaultAsync(r =>
                    r.SubmissionId == notification.SubmissionId &&
                    r.RequestType == SkillLevelUpMethod.Assessment &&
                    r.RequestStatus == SkillLevelUpRequestStatus.PendingAssessment,
                    cancellationToken);

            if (levelUpRequest != null)
            {
                if (testPassed)
                {
                    var approveResult = levelUpRequest.Approve();
                    if (approveResult.IsSuccess)
                    {
                        var newLevel = (SkillLevel)Math.Min((int)SkillLevel.Expert, (int)levelUpRequest.CurrentLevel + 1);

                        var profile = await _dbContext.SkillProfiles
                            .FirstOrDefaultAsync(p => p.UserId == levelUpRequest.UserId, cancellationToken);

                        var applyResult = profile?.ApplyLevelUp(levelUpRequest.SkillId, newLevel);
                        var appliedSuccessfully = applyResult?.IsSuccess ?? false;

                        var historyOutcome = appliedSuccessfully ? SkillHistoryOutcome.Applied : SkillHistoryOutcome.Rejected;
                        var historyResult = SkillHistoryEntry.Record(
                            userId: levelUpRequest.UserId,
                            skillId: levelUpRequest.SkillId,
                            changeSource: SkillChangeSource.UserInitiated,
                            responsibleAccountId: levelUpRequest.UserId, // tự khởi xướng qua Assessment
                            outcome: historyOutcome,
                            levelBefore: levelUpRequest.CurrentLevel,
                            levelAfter: appliedSuccessfully ? newLevel : null,
                            relatedSubmissionId: notification.SubmissionId,
                            relatedRequestId: levelUpRequest.Id,
                            rejectionReason: appliedSuccessfully ? null : $"Lỗi kỹ thuật khi áp dụng: {applyResult?.Message ?? "không rõ nguyên nhân"}");

                        if (historyResult.IsSuccess)
                            _dbContext.SkillHistoryEntries.Add(historyResult.Data!);

                        if (profile == null || !appliedSuccessfully)
                        {
                            var fallbackNotif = Notification.Create(
                                levelUpRequest.UserId,
                                appliedSuccessfully ? "Nâng cấp độ kỹ năng thành công" : "Không thể áp dụng nâng cấp kỹ năng",
                                appliedSuccessfully
                                    ? $"Bạn đã đạt bài kiểm tra và kỹ năng được nâng lên mức {newLevel}."
                                    : "Bạn đã đạt bài kiểm tra nhưng hệ thống gặp lỗi khi cập nhật hồ sơ kỹ năng. Vui lòng liên hệ hỗ trợ.",
                                appliedSuccessfully ? NotificationType.Success : NotificationType.Warning);

                            if (fallbackNotif.IsSuccess)
                                _dbContext.Notifications.Add(fallbackNotif.Data!);
                        }
                        // Nếu applied thành công và profile != null: SkillLevelApprovedEvent (raised bởi
                        // ApplyLevelUp) tự lo Notification qua SkillLevelApprovedEventHandler — không tạo trùng.
                    }
                }
                else
                {
                    var rejectResult = levelUpRequest.Reject("Không đạt bài kiểm tra đánh giá năng lực (Assessment).");
                    if (rejectResult.IsSuccess)
                    {
                        var historyResult = SkillHistoryEntry.Record(
                            userId: levelUpRequest.UserId,
                            skillId: levelUpRequest.SkillId,
                            changeSource: SkillChangeSource.UserInitiated,
                            responsibleAccountId: levelUpRequest.UserId,
                            outcome: SkillHistoryOutcome.Rejected,
                            levelBefore: levelUpRequest.CurrentLevel,
                            levelAfter: null,
                            relatedSubmissionId: notification.SubmissionId,
                            relatedRequestId: levelUpRequest.Id,
                            rejectionReason: "Không đạt bài kiểm tra đánh giá năng lực (Assessment).");

                        if (historyResult.IsSuccess)
                            _dbContext.SkillHistoryEntries.Add(historyResult.Data!);

                        // levelUpRequest.Reject() tự raise SkillLevelUpRejectedEvent -> handler có sẵn
                        // sẽ gửi Notification "bị từ chối". SkillPenaltyAppliedEvent do Reject() KHÔNG
                        // tự raise (theo comment gốc trong SkillLevelUpRequest.cs) — nếu nghiệp vụ mục
                        // 4.3.1 yêu cầu áp phạt x2 ngay cả với Assessment thất bại (không chỉ Endorsement
                        // reject qua RejectSkillLevelUpRequestCommand), cần bổ sung gọi
                        // profile.ApplyPenaltyDowngrade(...) tương tự RejectSkillLevelUpRequestCommand ở đây.
                    }
                }
            }
        }
    }
}