using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Entities;

namespace TaskMind.Applications.Admins.Features.Reviews
{
    /// <summary>Admin gỡ một đánh giá vi phạm (spam/xúc phạm) — Review không có Status nên xoá cứng, có ghi AuditLog.</summary>
    public class RemoveReviewCommand : ServiceResult
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

    public class RemoveReviewHandler
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

            _dbContext.Reviews.Remove(review);

            var auditResult = AuditLog.Record(command.ApproverAdminId, "ReviewRemovedByAdmin", nameof(Review), review.Id);
            if (auditResult.IsSuccess)
                _dbContext.AuditLogs.Add(auditResult.Data!);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return ServiceResult.Success("Gỡ đánh giá thành công");
        }
    }
}