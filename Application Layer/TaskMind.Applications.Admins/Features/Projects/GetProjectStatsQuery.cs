// GetProjectStatsQuery.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Admins.Dtos;
using TaskMind.Applications.Commons;

namespace TaskMind.Applications.Admins.Features.Projects
{
    public class GetProjectStatsQuery : IRequest<ServiceResult<ProjectStatsDto>>
    {
    }

    public class GetProjectStatsHandler : IRequestHandler<GetProjectStatsQuery, ServiceResult<ProjectStatsDto>>
    {
        private readonly IApplicationDbContext _dbContext;

        public GetProjectStatsHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ServiceResult<ProjectStatsDto>> Handle(GetProjectStatsQuery query, CancellationToken cancellationToken)
        {
            var totalProjects = await _dbContext.Projects.CountAsync(cancellationToken);

            var byStatus = await _dbContext.Projects
                .GroupBy(p => p.ProjectStatus)
                .Select(g => new ProjectStatusCountDto { Status = g.Key, Count = g.Count() })
                .ToListAsync(cancellationToken);

            var bySourceType = await _dbContext.Projects
                .GroupBy(p => p.SourceType)
                .Select(g => new ProjectSourceTypeCountDto { SourceType = g.Key, Count = g.Count() })
                .ToListAsync(cancellationToken);

            var exchangeCount = await _dbContext.Projects.CountAsync(p => p.IsExchangeProject, cancellationToken);

            var dto = new ProjectStatsDto
            {
                TotalProjects = totalProjects,
                ByStatus = byStatus,
                BySourceType = bySourceType,
                ExchangeProjectCount = exchangeCount,
                publicProjectCount = totalProjects - exchangeCount
            };

            return ServiceResult<ProjectStatsDto>.Success(dto, "Lấy thống kê dự án thành công");
        }
    }
}