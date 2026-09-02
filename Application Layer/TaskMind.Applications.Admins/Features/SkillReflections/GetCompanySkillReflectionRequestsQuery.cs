// GetCompanySkillReflectionRequestsQuery.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Admins.Dtos;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Enums;

namespace TaskMind.Applications.Admins.Features.SkillReflections
{
    public class GetCompanySkillReflectionRequestsFilter
    {
        public SkillReflectionStatus? Status { get; set; }
        public SkillReflectionType? ReflectionType { get; set; }
        public Guid? CompanyId { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class CompanySkillReflectionRequestListItemDto
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public Guid UserId { get; set; }
        public Guid SkillId { get; set; }
        public string SkillName { get; set; } = string.Empty;
        public SkillReflectionType ReflectionType { get; set; }
        public SkillReflectionStatus Status { get; set; }
        public string EvidenceDescription { get; set; } = string.Empty;
        public int? IncidentFrequency { get; set; }
        public DateTimeOffset CreatedAtUtc { get; set; }
    }

    public class GetCompanySkillReflectionRequestsQuery : IRequest<ServiceResult<PagedResult<CompanySkillReflectionRequestListItemDto>>>
    {
        public GetCompanySkillReflectionRequestsFilter Filter { get; }
        public GetCompanySkillReflectionRequestsQuery(GetCompanySkillReflectionRequestsFilter filter) => Filter = filter;
    }

    public class GetCompanySkillReflectionRequestsHandler
        : IRequestHandler<GetCompanySkillReflectionRequestsQuery, ServiceResult<PagedResult<CompanySkillReflectionRequestListItemDto>>>
    {
        private readonly IApplicationDbContext _dbContext;
        public GetCompanySkillReflectionRequestsHandler(IApplicationDbContext dbContext) => _dbContext = dbContext;

        public async Task<ServiceResult<PagedResult<CompanySkillReflectionRequestListItemDto>>> Handle(
            GetCompanySkillReflectionRequestsQuery query, CancellationToken cancellationToken)
        {
            var filter = query.Filter;
            var page = Math.Max(1, filter.Page);
            var pageSize = filter.PageSize is > 0 and <= 100 ? filter.PageSize : 20;

            var q = _dbContext.CompanySkillReflectionRequests.AsNoTracking();

            if (filter.Status.HasValue) q = q.Where(r => r.Status == filter.Status.Value);
            if (filter.ReflectionType.HasValue) q = q.Where(r => r.ReflectionType == filter.ReflectionType.Value);
            if (filter.CompanyId.HasValue) q = q.Where(r => r.CompanyId == filter.CompanyId.Value);

            var totalCount = await q.CountAsync(cancellationToken);

            var page1 = await q
                .OrderBy(r => r.Status == SkillReflectionStatus.PendingAdminReview ? 0 : 1)
                .ThenByDescending(r => r.CreatedAtUtc)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var skillIds = page1.Select(r => r.SkillId).Distinct().ToList();
            var skillNames = await _dbContext.Skills.AsNoTracking()
                .Where(s => skillIds.Contains(s.Id))
                .ToDictionaryAsync(s => s.Id, s => s.SkillName, cancellationToken);

            var items = page1.Select(r => new CompanySkillReflectionRequestListItemDto
            {
                Id = r.Id,
                CompanyId = r.CompanyId,
                UserId = r.UserId,
                SkillId = r.SkillId,
                SkillName = skillNames.TryGetValue(r.SkillId, out var n) ? n : string.Empty,
                ReflectionType = r.ReflectionType,
                Status = r.Status,
                EvidenceDescription = r.EvidenceDescription,
                IncidentFrequency = r.IncidentFrequency,
                CreatedAtUtc = r.CreatedAtUtc
            }).ToList();

            return ServiceResult<PagedResult<CompanySkillReflectionRequestListItemDto>>.Success(new PagedResult<CompanySkillReflectionRequestListItemDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            }, "Lấy danh sách đề xuất phản ánh kỹ năng thành công");
        }
    }
}