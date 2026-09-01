// GetReputationScoreQuery.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Enums;

namespace TaskMind.Applications.Admins.Features.Reviews
{
    public class GetReputationScoreQuery : IRequest<ServiceResult<ReputationScoreDto>>
    {
        public ReviewTargetType TargetType { get; }
        public Guid TargetRefId { get; }

        public GetReputationScoreQuery(ReviewTargetType targetType, Guid targetRefId)
        {
            TargetType = targetType;
            TargetRefId = targetRefId;
        }
    }

    public class ReputationScoreDto
    {
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
    }

    public class GetReputationScoreHandler : IRequestHandler<GetReputationScoreQuery, ServiceResult<ReputationScoreDto>>
    {
        private readonly IApplicationDbContext _dbContext;
        public GetReputationScoreHandler(IApplicationDbContext dbContext) => _dbContext = dbContext;

        public async Task<ServiceResult<ReputationScoreDto>> Handle(GetReputationScoreQuery query, CancellationToken cancellationToken)
        {
            var reviews = _dbContext.Reviews.AsNoTracking()
                .Where(r => r.TargetType == query.TargetType && r.TargetRefId == query.TargetRefId);

            var count = await reviews.CountAsync(cancellationToken);
            var avg = count == 0 ? 0 : await reviews.AverageAsync(r => (double)r.Rating, cancellationToken);

            return ServiceResult<ReputationScoreDto>.Success(new ReputationScoreDto
            {
                AverageRating = Math.Round(avg, 2),
                ReviewCount = count
            }, "Lấy điểm uy tín thành công");
        }
    }
}