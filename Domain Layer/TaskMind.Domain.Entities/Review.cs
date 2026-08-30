using Microsoft.EntityFrameworkCore;
using TaskMind.Domain.Commons.Cores;
using TaskMind.Domain.Commons.Result;
using TaskMind.Domain.Enums;
using TaskMind.Domain.Events;

namespace TaskMind.Domain.Entities
{
    /// <summary>Aggregate Root Review [MỚI] — đánh giá đa hình giữa User/Company/School (mục 4.19).</summary>
    [Index(nameof(TargetType), nameof(TargetRefId))]
    [Index(nameof(ReviewerAccountId))]
    public class Review : AggregateRoot
    {
        public ReviewTargetType TargetType { get; private set; }
        public Guid TargetRefId { get; private set; }
        public Guid ReviewerAccountId { get; private set; }
        public int Rating { get; private set; }
        public string Comment { get; private set; } = string.Empty;
        public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;

        private Review() { }

        private Review(ReviewTargetType targetType, Guid targetRefId, Guid reviewerAccountId, int rating, string comment)
        {
            TargetType = targetType;
            TargetRefId = targetRefId;
            ReviewerAccountId = reviewerAccountId;
            Rating = rating;
            Comment = comment;
        }

        public static Result<Review> Create(ReviewTargetType targetType, Guid targetRefId, Guid reviewerAccountId, int rating, string? comment = null)
        {
            if (targetRefId == Guid.Empty)
                return Result<Review>.Failure("TargetRefId không hợp lệ.");
            if (reviewerAccountId == Guid.Empty)
                return Result<Review>.Failure("ReviewerAccountId không hợp lệ.");
            if (rating is < 1 or > 5)
                return Result<Review>.Failure("Điểm đánh giá phải trong khoảng 1-5.");

            var review = new Review(targetType, targetRefId, reviewerAccountId, rating, comment?.Trim() ?? string.Empty);

            review.AddDomainEvent(new ReviewCreatedEvent
            {
                ReviewId = review.Id,
                TargetType = targetType,
                TargetRefId = targetRefId,
                Rating = rating
            });

            return Result<Review>.Success(review);
        }
    }
}
