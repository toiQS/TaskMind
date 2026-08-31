using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Admins.Dtos;
using TaskMind.Applications.Commons;

namespace TaskMind.Applications.Admins.Features.Reviews
{
    /// <summary>Admin xem đánh giá đa hình (User/Company/School) để kiểm duyệt (mục 4.19).</summary>
    public class GetReviewsQuery : ServiceResult<PagedResult<ReviewListItemDto>>
    {
        public GetReviewsFilter Filter { get; }

        public GetReviewsQuery(GetReviewsFilter filter)
        {
            Filter = filter;
        }
    }

    public class GetReviewsHandler
    {
        private readonly IApplicationDbContext _dbContext;

        public GetReviewsHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ServiceResult<PagedResult<ReviewListItemDto>>> Handle(GetReviewsQuery query, CancellationToken cancellationToken)
        {
            var filter = query.Filter;
            var page = Math.Max(1, filter.Page);
            var pageSize = filter.PageSize is > 0 and <= 100 ? filter.PageSize : 20;

            var reviewsQuery = _dbContext.Reviews.AsNoTracking();

            if (filter.TargetType.HasValue)
                reviewsQuery = reviewsQuery.Where(r => r.TargetType == filter.TargetType.Value);

            if (filter.TargetRefId.HasValue)
                reviewsQuery = reviewsQuery.Where(r => r.TargetRefId == filter.TargetRefId.Value);

            if (filter.MinRating.HasValue)
                reviewsQuery = reviewsQuery.Where(r => r.Rating >= filter.MinRating.Value);

            if (filter.MaxRating.HasValue)
                reviewsQuery = reviewsQuery.Where(r => r.Rating <= filter.MaxRating.Value);

            var totalCount = await reviewsQuery.CountAsync(cancellationToken);

            var items = await reviewsQuery
                .OrderByDescending(r => r.CreatedAtUtc)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new ReviewListItemDto
                {
                    Id = r.Id,
                    TargetType = r.TargetType,
                    TargetRefId = r.TargetRefId,
                    ReviewerAccountId = r.ReviewerAccountId,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAtUtc = r.CreatedAtUtc
                })
                .ToListAsync(cancellationToken);

            var result = new PagedResult<ReviewListItemDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };

            return ServiceResult<PagedResult<ReviewListItemDto>>.Success(result, "Lấy danh sách đánh giá thành công");
        }
    }
}