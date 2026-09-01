using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Admins.Dtos;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Enums;


namespace TaskMind.Applications.Admins.Features.Jobs
{
    public class GetDashbroadQuery() : IRequest<ServiceResult<DashboardRecruitmentStatsDto>>
    {

    }
    public class GetDashbroadHandler : IRequestHandler<GetDashbroadQuery, ServiceResult<DashboardRecruitmentStatsDto>>
    {
        private readonly IApplicationDbContext _dbContext;
        public GetDashbroadHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ServiceResult<DashboardRecruitmentStatsDto>> Handle(GetDashbroadQuery query, CancellationToken cancellationToken)
        {
            var totalJobPostings = await _dbContext.JobPostings.CountAsync(cancellationToken);
            var totalJobsClosers = await _dbContext.JobPostings.CountAsync(j => j.PostingStatus == JobPostingStatus.Closed, cancellationToken);
            var statsDto = new DashboardRecruitmentStatsDto
            {
                TotalJobPostings = totalJobPostings,
                TotalJobsClosers = totalJobsClosers
            };

            // Gộp thành 1 truy vấn thay vì N+1
            var openPostings = await _dbContext.JobPostings
                .AsNoTracking()
                .Where(j => j.PostingStatus == JobPostingStatus.Open)
                .Select(j => j.RequiredSkillIds)
                .ToListAsync(cancellationToken);

            var skillCounts = openPostings
                .SelectMany(ids => ids)
                .GroupBy(id => id)
                .ToDictionary(g => g.Key, g => g.Count());

            var skills = await _dbContext.Skills
                .AsNoTracking()
                .Where(s => skillCounts.Keys.Contains(s.Id))
                .ToListAsync(cancellationToken);

            statsDto.JobPostingsByStatus = skills.Select(s => new NodeChatDto
            {
                Name = s.SkillName, 
                Value = skillCounts.TryGetValue(s.Id, out var c) ? c : 0
            }).ToList();

            return ServiceResult<DashboardRecruitmentStatsDto>.Success(statsDto);
        }
    }
}
