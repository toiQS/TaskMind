// SubmissionGradedEventHandler.cs
// [CẬP NHẬT - fix] Trước đây chỉ xử lý cấp Certificate cho TestPaper thuộc School — bỏ sót hoàn toàn
// nhánh xác minh CompanySkillReflectionRequest (mục 4.3.2): dù
// LinkReflectionVerificationSubmissionCommand đã liên kết VerificationSubmissionId, không nơi nào
// thực sự gọi request.ApplyVerificationResult(...) khi bài được chấm điểm, khiến đề xuất kẹt mãi ở
// trạng thái PendingVerification. Dùng chung PassThreshold với luồng Certificate cho nhất quán (mục 8
// - vấn đề mở: ánh xạ điểm số sang level cụ thể vẫn cần quy định thêm).
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

        // TODO: nên đưa ra config thay vì hardcode (mục 8 - vấn đề mở).
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

            // Cấp Certificate tự động khi TestPaper.OwnerType = School và đạt yêu cầu (mục 7.3.4).
            var testPaper = await _dbContext.TestPapers
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == notification.TestPaperId, cancellationToken);

            if (testPaper?.OwnerType == TestOwnerType.School && testPassed)
            {
                var certResult = Certificate.Issue(notification.UserId, notification.SubmissionId);
                if (certResult.IsSuccess)
                    _dbContext.Certificates.Add(certResult.Data!);
            }

            // [MỚI - fix, mục 4.3.2] Nếu bài làm này đã được liên kết làm bằng chứng xác minh cho một
            // đề xuất phản ánh kỹ năng của công ty đang chờ xác minh, áp dụng kết quả ngay khi có điểm.
            // request được lấy có tracking (không AsNoTracking) để lời gọi ApplyVerificationResult bên
            // dưới thực sự mutate + phát sinh domain event, được vòng lặp publish trong SaveChangesAsync
            // của ApplicationDbContext/WeblicationDbContext gom lại như mọi aggregate khác.
            var reflectionRequest = await _dbContext.CompanySkillReflectionRequests
                .FirstOrDefaultAsync(r =>
                    r.VerificationSubmissionId == notification.SubmissionId &&
                    r.Status == SkillReflectionStatus.PendingVerification,
                    cancellationToken);

            reflectionRequest?.ApplyVerificationResult(testPassed);
        }
    }
}