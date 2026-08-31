using TaskMind.Domain.Enums;

namespace TaskMind.Applications.Admins.Dtos
{
    public class ReviewListItemDto
    {
        public Guid Id { get; set; }
        public ReviewTargetType TargetType { get; set; }
        public Guid TargetRefId { get; set; }
        public Guid ReviewerAccountId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAtUtc { get; set; }
    }

    public class GetReviewsFilter
    {
        public ReviewTargetType? TargetType { get; set; }
        public Guid? TargetRefId { get; set; }
        public int? MinRating { get; set; }
        public int? MaxRating { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}