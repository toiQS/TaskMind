using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Admins.Dtos;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Enums;

namespace TaskMind.Applications.Admins.Features.Jobs
{
    /// <summary>Admin xem thống kê tổng quan tuyển dụng toàn nền tảng (mục 4.18) — có thể ghép vào Dashboard (mục 4.14) khi cần.</summary>
    public class GetRecruitmentStatsQuery : ServiceResult<RecruitmentStatsDto>
    {
    }

    public class GetRecruitmentStatsHandler
    {
        private readonly IApplicationDbContext _dbContext;

        public GetRecruitmentStatsHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ServiceResult<RecruitmentStatsDto>> Handle(GetRecruitmentStatsQuery query, CancellationToken cancellationToken)
        {
            var totalJobPostings = await _dbContext.JobPostings.CountAsync(cancellationToken);
            var openJobPostings = await _dbContext.JobPostings.CountAsync(p => p.PostingStatus == JobPostingStatus.Open, cancellationToken);
            var totalApplications = await _dbContext.JobApplications.CountAsync(cancellationToken);

            var postingsByStatus = await _dbContext.JobPostings
                .GroupBy(p => p.PostingStatus)
                .Select(g => new JobPostingStatusCountDto { Status = g.Key, Count = g.Count() })
                .ToListAsync(cancellationToken);

            var applicationsByStatus = await _dbContext.JobApplications
                .GroupBy(a => a.ApplicationStatus)
                .Select(g => new ApplicationStatusCountDto { Status = g.Key, Count = g.Count() })
                .ToListAsync(cancellationToken);

            var topCompanies = await (
                    from posting in _dbContext.JobPostings.AsNoTracking()
                    group posting by posting.CompanyId into g
                    orderby g.Count() descending
                    select new { CompanyId = g.Key, PostingCount = g.Count() })
                .Take(5)
                .ToListAsync(cancellationToken);

            var companyIds = topCompanies.Select(t => t.CompanyId).ToList();
            var companyNames = await _dbContext.Companies
                .AsNoTracking()
                .Where(c => companyIds.Contains(c.Id))
                .ToDictionaryAsync(c => c.Id, c => c.CompanyName, cancellationToken);

            var dto = new RecruitmentStatsDto
            {
                TotalJobPostings = totalJobPostings,
                OpenJobPostings = openJobPostings,
                TotalApplications = totalApplications,
                PostingsByStatus = postingsByStatus,
                ApplicationsByStatus = applicationsByStatus,
                TopCompaniesByPostingCount = topCompanies.Select(t => new TopCompanyByPostingDto
                {
                    CompanyId = t.CompanyId,
                    CompanyName = companyNames.TryGetValue(t.CompanyId, out var name) ? name : string.Empty,
                    PostingCount = t.PostingCount
                }).ToList()
            };

            return ServiceResult<RecruitmentStatsDto>.Success(dto, "Lấy thống kê tuyển dụng thành công");
        }
    }
}