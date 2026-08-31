using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Admins.Dtos;
using TaskMind.Applications.Commons;

namespace TaskMind.Applications.Admins.Features.Skills
{
    /// <summary>Admin/Approver xem hàng chờ duyệt nâng cấp độ kỹ năng (mục 4.3.1).</summary>
    public class GetSkillLevelUpRequestsQuery : ServiceResult<PagedResult<SkillLevelUpRequestListItemDto>>
    {
        public GetSkillLevelUpRequestsFilter Filter { get; }

        public GetSkillLevelUpRequestsQuery(GetSkillLevelUpRequestsFilter filter)
        {
            Filter = filter;
        }
    }

    public class GetSkillLevelUpRequestsHandler
    {
        private readonly IApplicationDbContext _dbContext;

        public GetSkillLevelUpRequestsHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ServiceResult<PagedResult<SkillLevelUpRequestListItemDto>>> Handle(GetSkillLevelUpRequestsQuery query, CancellationToken cancellationToken)
        {
            var filter = query.Filter;
            var page = Math.Max(1, filter.Page);
            var pageSize = filter.PageSize is > 0 and <= 100 ? filter.PageSize : 20;

            var requestsQuery = _dbContext.SkillLevelUpRequests.AsNoTracking();

            if (filter.RequestStatus.HasValue)
                requestsQuery = requestsQuery.Where(r => r.RequestStatus == filter.RequestStatus.Value);

            if (filter.UserId.HasValue)
                requestsQuery = requestsQuery.Where(r => r.UserId == filter.UserId.Value);

            if (filter.ApproverAccountId.HasValue)
                requestsQuery = requestsQuery.Where(r => r.ApproverAccountId == filter.ApproverAccountId.Value);

            var totalCount = await requestsQuery.CountAsync(cancellationToken);

            var page1 = await requestsQuery
                .OrderBy(r => r.RequestStatus) // PendingEndorsement/PendingAssessment lên trước
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new
                {
                    r.Id,
                    r.UserId,
                    r.SkillId,
                    r.CurrentLevel,
                    r.RequestType,
                    r.RequestStatus,
                    r.ApproverAccountId,
                    r.SubmissionId
                })
                .ToListAsync(cancellationToken);

            var skillIds = page1.Select(p => p.SkillId).Distinct().ToList();
            var skillNames = await _dbContext.Skills.AsNoTracking()
                .Where(s => skillIds.Contains(s.Id))
                .ToDictionaryAsync(s => s.Id, s => s.SkillName, cancellationToken);

            var items = page1.Select(p => new SkillLevelUpRequestListItemDto
            {
                Id = p.Id,
                UserId = p.UserId,
                SkillId = p.SkillId,
                SkillName = skillNames.TryGetValue(p.SkillId, out var name) ? name : string.Empty,
                CurrentLevel = p.CurrentLevel,
                RequestType = p.RequestType,
                RequestStatus = p.RequestStatus,
                ApproverAccountId = p.ApproverAccountId,
                SubmissionId = p.SubmissionId
            }).ToList();

            var result = new PagedResult<SkillLevelUpRequestListItemDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };

            return ServiceResult<PagedResult<SkillLevelUpRequestListItemDto>>.Success(result, "Lấy danh sách yêu cầu nâng cấp độ kỹ năng thành công");
        }
    }
}