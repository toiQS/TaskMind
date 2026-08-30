using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Admins.Dtos;
using TaskMind.Applications.Commons;

namespace TaskMind.Applications.Admins.Features.Jobs
{
    /// <summary>Admin xem toàn bộ tin tuyển dụng trên nền tảng (mọi công ty) để giám sát/kiểm duyệt (mục 4.18).</summary>
    public class GetJobPostingsQuery : ServiceResult<PagedResult<JobPostingListItemDto>>
    {
        public GetJobPostingsFilter Filter { get; }

        public GetJobPostingsQuery(GetJobPostingsFilter filter)
        {
            Filter = filter;
        }
    }

    public class GetJobPostingsHandler
    {
        private readonly IApplicationDbContext _dbContext;

        public GetJobPostingsHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ServiceResult<PagedResult<JobPostingListItemDto>>> Handle(GetJobPostingsQuery query, CancellationToken cancellationToken)
        {
            var filter = query.Filter;
            var page = Math.Max(1, filter.Page);
            var pageSize = filter.PageSize is > 0 and <= 100 ? filter.PageSize : 20;

            var postingsQuery =
                from posting in _dbContext.JobPostings.AsNoTracking()
                join company in _dbContext.Companies.AsNoTracking()
                    on posting.CompanyId equals company.Id
                select new { posting, company };

            if (filter.CompanyId.HasValue)
                postingsQuery = postingsQuery.Where(x => x.posting.CompanyId == filter.CompanyId.Value);

            if (filter.PostingStatus.HasValue)
                postingsQuery = postingsQuery.Where(x => x.posting.PostingStatus == filter.PostingStatus.Value);

            if (!string.IsNullOrWhiteSpace(filter.Keyword))
            {
                var keyword = filter.Keyword.Trim();
                postingsQuery = postingsQuery.Where(x =>
                    EF.Functions.ILike(x.posting.Title, $"%{keyword}%") ||
                    EF.Functions.ILike(x.company.CompanyName, $"%{keyword}%"));
            }

            var totalCount = await postingsQuery.CountAsync(cancellationToken);

            var page1 = await postingsQuery
                .OrderByDescending(x => x.posting.Id) // JobPosting không có CreatedAtUtc (AggregateRoot thường, không phải Auditable)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new
                {
                    x.posting.Id,
                    x.posting.CompanyId,
                    x.company.CompanyName,
                    x.posting.Title,
                    x.posting.PostingStatus,
                    RequiredSkillCount = x.posting.RequiredSkillIds.Count
                })
                .ToListAsync(cancellationToken);

            var postingIds = page1.Select(p => p.Id).ToList();

            var applicationCounts = await _dbContext.JobApplications
                .AsNoTracking()
                .Where(a => postingIds.Contains(a.JobPostingId))
                .GroupBy(a => a.JobPostingId)
                .Select(g => new { JobPostingId = g.Key, Count = g.Count() })
                .ToListAsync(cancellationToken);

            var items = page1.Select(p => new JobPostingListItemDto
            {
                Id = p.Id,
                CompanyId = p.CompanyId,
                CompanyName = p.CompanyName,
                Title = p.Title,
                PostingStatus = p.PostingStatus,
                RequiredSkillCount = p.RequiredSkillCount,
                ApplicationCount = applicationCounts.FirstOrDefault(a => a.JobPostingId == p.Id)?.Count ?? 0
            }).ToList();

            var result = new PagedResult<JobPostingListItemDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };

            return ServiceResult<PagedResult<JobPostingListItemDto>>.Success(result, "Lấy danh sách tin tuyển dụng thành công");
        }
    }
}