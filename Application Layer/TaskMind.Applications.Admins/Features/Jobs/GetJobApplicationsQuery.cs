using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Admins.Dtos;
using TaskMind.Applications.Commons;

namespace TaskMind.Applications.Admins.Features.Jobs
{
    /// <summary>Admin xem toàn bộ hồ sơ ứng tuyển trên nền tảng (mục 4.18), lọc theo tin tuyển dụng/ứng viên/trạng thái.</summary>
    public class GetJobApplicationsQuery : ServiceResult<PagedResult<JobApplicationDetailDto>>
    {
        public GetJobApplicationsFilter Filter { get; }

        public GetJobApplicationsQuery(GetJobApplicationsFilter filter)
        {
            Filter = filter;
        }
    }

    public class GetJobApplicationsHandler
    {
        private readonly IApplicationDbContext _dbContext;

        public GetJobApplicationsHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ServiceResult<PagedResult<JobApplicationDetailDto>>> Handle(GetJobApplicationsQuery query, CancellationToken cancellationToken)
        {
            var filter = query.Filter;
            var page = Math.Max(1, filter.Page);
            var pageSize = filter.PageSize is > 0 and <= 100 ? filter.PageSize : 20;

            var baseQuery =
                from application in _dbContext.JobApplications.AsNoTracking()
                join posting in _dbContext.JobPostings.AsNoTracking()
                    on application.JobPostingId equals posting.Id
                join company in _dbContext.Companies.AsNoTracking()
                    on posting.CompanyId equals company.Id
                select new { application, posting, company };

            if (filter.JobPostingId.HasValue)
                baseQuery = baseQuery.Where(x => x.application.JobPostingId == filter.JobPostingId.Value);

            if (filter.UserId.HasValue)
                baseQuery = baseQuery.Where(x => x.application.UserId == filter.UserId.Value);

            if (filter.ApplicationStatus.HasValue)
                baseQuery = baseQuery.Where(x => x.application.ApplicationStatus == filter.ApplicationStatus.Value);

            var totalCount = await baseQuery.CountAsync(cancellationToken);

            var page1 = await baseQuery
                .OrderByDescending(x => x.application.AppliedAtUtc)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new
                {
                    x.application.Id,
                    x.application.JobPostingId,
                    PostingTitle = x.posting.Title,
                    x.application.UserId,
                    x.application.ApplicationStatus,
                    x.application.AppliedAtUtc,
                    //x.company.Id.GetHashCode(), // placeholder, xem ghi chú
                    CompanyId = x.company.Id,
                    CompanyName = x.company.CompanyName
                })
                .ToListAsync(cancellationToken);

            var userIds = page1.Select(p => p.UserId).Distinct().ToList();
            var users = await _dbContext.Users
                .AsNoTracking()
                .Include(u => u.Profile)
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, cancellationToken);

            var items = page1.Select(p => new JobApplicationDetailDto
            {
                Id = p.Id,
                JobPostingId = p.JobPostingId,
                JobPostingTitle = p.PostingTitle,
                UserId = p.UserId,
                UserEmail = users.TryGetValue(p.UserId, out var u) ? u.Profile.Email : string.Empty,
                UserFullName = users.TryGetValue(p.UserId, out var u2) ? $"{u2.Profile.FirstName} {u2.Profile.LastName}".Trim() : string.Empty,
                ApplicationStatus = p.ApplicationStatus,
                AppliedAtUtc = p.AppliedAtUtc,
                CompanyId = p.CompanyId,
                CompanyName = p.CompanyName
            }).ToList();

            var result = new PagedResult<JobApplicationDetailDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };

            return ServiceResult<PagedResult<JobApplicationDetailDto>>.Success(result, "Lấy danh sách hồ sơ ứng tuyển thành công");
        }
    }
}