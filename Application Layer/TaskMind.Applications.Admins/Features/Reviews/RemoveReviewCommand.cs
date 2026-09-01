// RemoveReviewCommand.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Entities;

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

            _dbContext.Reviews.Remove(review);

            var auditResult = AuditLog.Record(command.ApproverAdminId, "ReviewRemovedByAdmin", nameof(Review), review.Id);
            if (auditResult.IsSuccess)
                _dbContext.AuditLogs.Add(auditResult.Data!);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return ServiceResult.Success("Gỡ đánh giá thành công");
        }
    }
}