// GetAuditLogsQuery.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Admins.Dtos;
using TaskMind.Applications.Commons;

namespace TaskMind.Applications.Admins.Features.Audits
{
    public class GetAuditLogsQuery : IRequest<ServiceResult<PagedResult<AuditLogListItemDto>>>
    {
        public GetAuditLogsFilter Filter { get; }

        public GetAuditLogsQuery(GetAuditLogsFilter filter)
        {
            Filter = filter;
        }
    }

    public class GetAuditLogsHandler : IRequestHandler<GetAuditLogsQuery, ServiceResult<PagedResult<AuditLogListItemDto>>>
    {
        private readonly IApplicationDbContext _dbContext;

        public GetAuditLogsHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ServiceResult<PagedResult<AuditLogListItemDto>>> Handle(GetAuditLogsQuery query, CancellationToken cancellationToken)
        {
            var filter = query.Filter;
            var page = Math.Max(1, filter.Page);
            var pageSize = filter.PageSize is > 0 and <= 100 ? filter.PageSize : 20;

            var logsQuery = _dbContext.AuditLogs.AsNoTracking();

            if (filter.ActorAccountId.HasValue)
                logsQuery = logsQuery.Where(l => l.ActorAccountId == filter.ActorAccountId.Value);

            if (!string.IsNullOrWhiteSpace(filter.EntityType))
                logsQuery = logsQuery.Where(l => l.EntityType == filter.EntityType);

            if (filter.EntityId.HasValue)
                logsQuery = logsQuery.Where(l => l.EntityId == filter.EntityId.Value);

            if (filter.FromDateUtc.HasValue)
                logsQuery = logsQuery.Where(l => l.CreatedAtUtc >= filter.FromDateUtc.Value);

            if (filter.ToDateUtc.HasValue)
                logsQuery = logsQuery.Where(l => l.CreatedAtUtc <= filter.ToDateUtc.Value);

            var totalCount = await logsQuery.CountAsync(cancellationToken);

            var items = await logsQuery
                .OrderByDescending(l => l.CreatedAtUtc)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(l => new AuditLogListItemDto
                {
                    Id = l.Id,
                    ActorAccountId = l.ActorAccountId,
                    Action = l.Action,
                    EntityType = l.EntityType,
                    EntityId = l.EntityId,
                    CreatedAtUtc = l.CreatedAtUtc
                })
                .ToListAsync(cancellationToken);

            var result = new PagedResult<AuditLogListItemDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };

            return ServiceResult<PagedResult<AuditLogListItemDto>>.Success(result, "Lấy nhật ký kiểm toán thành công");
        }
    }
}