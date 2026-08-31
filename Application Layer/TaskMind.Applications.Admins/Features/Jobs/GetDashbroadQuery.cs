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

            var skills = await _dbContext.Skills.ToListAsync(cancellationToken);

            foreach (var skill in skills)
            {
                var jobPostingsCount = await _dbContext.JobPostings.CountAsync(j => j.RequiredSkillIds.Contains(skill.Id) && j.PostingStatus == JobPostingStatus.Open, cancellationToken);
                statsDto.JobPostingsByStatus.Add(new NodeChatDto
                {
                    Name = skill.Name,
                    Value = jobPostingsCount
                });
            }

            return ServiceResult<DashboardRecruitmentStatsDto>.Success(statsDto);
        }
    }
}
