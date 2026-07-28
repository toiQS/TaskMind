using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Admins.Dtos;
using TaskMind.Applications.Admins.Mapping;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Enums;

namespace TaskMind.Applications.Admins.Features.Companies
{
    public class GetCompaniesQuery : IRequest<List<CompanyDto>>
    {
        public string? SearchText { get; set; }

        /// <summary>"All" | "Pending" | "Active" | "Suspended" | "Rejected"</summary>
        public string StatusFilter { get; set; } = "All";
    }

    public class GetCompaniesQueryHandler : IRequestHandler<GetCompaniesQuery, List<CompanyDto>>
    {
        private readonly IApplicationDbContext _db;

        public GetCompaniesQueryHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<CompanyDto>> Handle(GetCompaniesQuery request, CancellationToken cancellationToken)
        {
            var companies = await _db.Companies.ToListAsync(cancellationToken);

            if (companies.Count == 0)
                return new List<CompanyDto>();

            var companyIds = companies.Select(c => c.Id).ToList();

            var staffCounts = await _db.Staffs
                .Where(s => companyIds.Contains(s.CompanyId))
                .GroupBy(s => s.CompanyId)
                .Select(g => new { CompanyId = g.Key, Count = g.Count() })
                .ToListAsync(cancellationToken);
            var staffCountMap = staffCounts.ToDictionary(x => x.CompanyId, x => x.Count);

            var projectCounts = await _db.Projects
                .Where(p => p.SourceType == ProjectSourceType.Company && p.OwningEntityId != null && companyIds.Contains(p.OwningEntityId.Value))
                .GroupBy(p => p.OwningEntityId!.Value)
                .Select(g => new { CompanyId = g.Key, Count = g.Count() })
                .ToListAsync(cancellationToken);
            var projectCountMap = projectCounts.ToDictionary(x => x.CompanyId, x => x.Count);

            IEnumerable<CompanyDto> dtos = companies.Select(c => CompanyMapper.ToDto(
                c,
                staffCountMap.GetValueOrDefault(c.Id),
                projectCountMap.GetValueOrDefault(c.Id)));

            if (!string.IsNullOrWhiteSpace(request.StatusFilter) && request.StatusFilter != "All")
                dtos = dtos.Where(c => string.Equals(c.Status, request.StatusFilter, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                var s = request.SearchText.Trim();
                dtos = dtos.Where(c => c.Name.Contains(s, StringComparison.OrdinalIgnoreCase));
            }

            return dtos.ToList();
        }
    }
}
