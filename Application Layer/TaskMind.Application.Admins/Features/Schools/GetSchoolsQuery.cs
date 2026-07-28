using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Admins.Dtos;
using TaskMind.Applications.Admins.Mapping;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Enums;

namespace TaskMind.Applications.Admins.Features.Schools
{
    public class GetSchoolsQuery : IRequest<List<SchoolDto>>
    {
        public string? SearchText { get; set; }

        /// <summary>"All" | "Pending" | "Active" | "Suspended" | "Rejected"</summary>
        public string StatusFilter { get; set; } = "All";
    }

    public class GetSchoolsQueryHandler : IRequestHandler<GetSchoolsQuery, List<SchoolDto>>
    {
        private readonly IApplicationDbContext _db;

        public GetSchoolsQueryHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<SchoolDto>> Handle(GetSchoolsQuery request, CancellationToken cancellationToken)
        {
            var schools = await _db.Schools.ToListAsync(cancellationToken);

            if (schools.Count == 0)
                return new List<SchoolDto>();

            var schoolIds = schools.Select(s => s.Id).ToList();

            var teacherCounts = await _db.Teachers
                .Where(t => schoolIds.Contains(t.SchoolId))
                .GroupBy(t => t.SchoolId)
                .Select(g => new { SchoolId = g.Key, Count = g.Count() })
                .ToListAsync(cancellationToken);
            var teacherCountMap = teacherCounts.ToDictionary(x => x.SchoolId, x => x.Count);

            var studentCounts = await _db.Students
                .Where(s => schoolIds.Contains(s.SchoolId))
                .GroupBy(s => s.SchoolId)
                .Select(g => new { SchoolId = g.Key, Count = g.Count() })
                .ToListAsync(cancellationToken);
            var studentCountMap = studentCounts.ToDictionary(x => x.SchoolId, x => x.Count);

            var projectCounts = await _db.Projects
                .Where(p => p.SourceType == ProjectSourceType.School && p.OwningEntityId != null && schoolIds.Contains(p.OwningEntityId.Value))
                .GroupBy(p => p.OwningEntityId!.Value)
                .Select(g => new { SchoolId = g.Key, Count = g.Count() })
                .ToListAsync(cancellationToken);
            var projectCountMap = projectCounts.ToDictionary(x => x.SchoolId, x => x.Count);

            IEnumerable<SchoolDto> dtos = schools.Select(s => SchoolMapper.ToDto(
                s,
                teacherCountMap.GetValueOrDefault(s.Id),
                studentCountMap.GetValueOrDefault(s.Id),
                projectCountMap.GetValueOrDefault(s.Id)));

            if (!string.IsNullOrWhiteSpace(request.StatusFilter) && request.StatusFilter != "All")
                dtos = dtos.Where(s => string.Equals(s.Status, request.StatusFilter, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                var s2 = request.SearchText.Trim();
                dtos = dtos.Where(s => s.Name.Contains(s2, StringComparison.OrdinalIgnoreCase));
            }

            return dtos.ToList();
        }
    }
}
