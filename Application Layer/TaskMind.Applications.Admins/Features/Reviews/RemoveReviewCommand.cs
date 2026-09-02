// RemoveReviewCommand.cs
// [CẬP NHẬT - fix] Bổ sung Notification cho người viết đánh giá — trước đây chỉ có AuditLog, người
// viết review không hề biết đánh giá của mình đã bị Admin gỡ.
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Entities;
using TaskMind.Domain.Enums;

namespace TaskMind.Applications.Admins.Features.Reviews
{
    public class RemoveReviewCommand : IRequest<ServiceResult>
    {
        public Guid ReviewId { get; }
        public Guid ApproverAdminId { get; }
        public string? Reason { get; }

        public RemoveReviewCommand(Guid reviewId, Guid approverAdminId, string? reason = null)
        {
            ReviewId = reviewId;
            ApproverAdminId = approverAdminId;
            Reason = reason;
        }
    }

    public class RemoveReviewHandler : IRequestHandler<RemoveReviewCommand, ServiceResult>
    {
        private readonly IApplicationDbContext _dbContext;

        public RemoveReviewHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ServiceResult> Handle(RemoveReviewCommand command, CancellationToken cancellationToken)
        {
            var review = await _dbContext.Reviews
                .FirstOrDefaultAsync(r => r.Id == command.ReviewId, cancellationToken);

            if (review == null)
                return ServiceResult.NotFound("Không tìm thấy đánh giá.");

            var reviewerAccountId = review.ReviewerAccountId;

            _dbContext.Reviews.Remove(review);

            var auditResult = AuditLog.Record(command.ApproverAdminId, "ReviewRemovedByAdmin", nameof(Review), review.Id);
            if (auditResult.IsSuccess)
                _dbContext.AuditLogs.Add(auditResult.Data!);

            var notifResult = Notification.Create(
                reviewerAccountId,
                "Đánh giá của bạn đã bị gỡ",
                "Một đánh giá bạn đã viết đã bị Admin hệ thống gỡ bỏ do vi phạm chính sách nền tảng." +
                (string.IsNullOrWhiteSpace(command.Reason) ? "" : $" Lý do: {command.Reason}"),
                NotificationType.Warning);

            if (notifResult.IsSuccess)
                _dbContext.Notifications.Add(notifResult.Data!);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return ServiceResult.Success("Gỡ đánh giá thành công");
        }
    }
}
