using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Admins.Dtos;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Enums;

namespace TaskMind.Applications.Admins.Features.Projects
{
    public class GetProjectDetailQuery : ServiceResult<ProjectDetailDto>
    {
        public Guid ProjectId { get; }

        public GetProjectDetailQuery(Guid projectId)
        {
            ProjectId = projectId;
        }
    }

    public class GetProjectDetailHandler
    {
        private readonly IApplicationDbContext _dbContext;

        public GetProjectDetailHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ServiceResult<ProjectDetailDto>> Handle(GetProjectDetailQuery query, CancellationToken cancellationToken)
        {
            var project = await _dbContext.Projects
                .AsNoTracking()
                .Include(p => p.Members)
                .FirstOrDefaultAsync(p => p.Id == query.ProjectId, cancellationToken);

            if (project == null)
                return ServiceResult<ProjectDetailDto>.NotFound("Không tìm thấy dự án.");

            string? owningEntityName = null;
            if (project.OwningEntityId.HasValue)
            {
                owningEntityName = project.SourceType switch
                {
                    ProjectSourceType.Company => (await _dbContext.Companies.AsNoTracking()
                        .FirstOrDefaultAsync(c => c.Id == project.OwningEntityId.Value, cancellationToken))?.CompanyName,
                    ProjectSourceType.School => (await _dbContext.Schools.AsNoTracking()
                        .FirstOrDefaultAsync(s => s.Id == project.OwningEntityId.Value, cancellationToken))?.SchoolName,
                    _ => null
                };
            }

            var accountIds = project.Members.Select(m => m.AccountId).Distinct().ToList();
            var profiles = await ResolveAccountProfiles(accountIds, cancellationToken);

            var hasActiveExchangeContract = await _dbContext.ExchangeContracts
                .AnyAsync(e => e.ProjectId == project.Id && e.ContractStatus == ExchangeStatus.Active, cancellationToken);

            var members = project.Members.Select(m =>
            {
                profiles.TryGetValue(m.AccountId, out var profile);
                return new ProjectMemberDetailDto
                {
                    AccountId = m.AccountId,
                    Role = m.Role,
                    IsActive = m.IsActive,
                    JoinedAt = m.JoinedAt,
                    LeftAt = m.LeftAt,
                    AccountType = profile.AccountType ?? "Unknown",
                    Email = profile.Email ?? string.Empty,
                    FullName = profile.FullName ?? string.Empty
                };
            }).OrderByDescending(m => m.IsActive).ThenBy(m => m.JoinedAt).ToList();

            var dto = new ProjectDetailDto
            {
                Id = project.Id,
                Title = project.Title,
                Description = project.Description,
                SourceType = project.SourceType,
                ProjectStatus = project.ProjectStatus,
                IsExchangeProject = project.IsExchangeProject,
                OwningEntityId = project.OwningEntityId,
                OwningEntityName = owningEntityName,
                CreatedAtUtc = project.CreatedAtUtc,
                Members = members,
                HasActiveExchangeContract = hasActiveExchangeContract
            };

            return ServiceResult<ProjectDetailDto>.Success(dto, "Lấy chi tiết dự án thành công");
        }

        /// <summary>
        /// ProjectMember.AccountId tham chiếu đa hình tới User/Staff/Student/Teacher (mục 3, mục 8 - vấn đề mở:
        /// không thể dùng FK thông thường). Tra cứu tuần tự qua từng bảng vì mỗi AccountId chỉ khớp đúng 1 bảng.
        /// </summary>
        private async Task<Dictionary<Guid, (string AccountType, string Email, string FullName)>> ResolveAccountProfiles(
            List<Guid> accountIds, CancellationToken cancellationToken)
        {
            var result = new Dictionary<Guid, (string, string, string)>();
            if (accountIds.Count == 0) return result;

            var users = await _dbContext.Users.AsNoTracking().Include(u => u.Profile)
                .Where(u => accountIds.Contains(u.Id)).ToListAsync(cancellationToken);
            foreach (var u in users)
                result[u.Id] = ("User", u.Profile.Email, $"{u.Profile.FirstName} {u.Profile.LastName}".Trim());

            var remaining = accountIds.Except(result.Keys).ToList();
            if (remaining.Count > 0)
            {
                var staffs = await _dbContext.Staffs.AsNoTracking().Include(s => s.Profile)
                    .Where(s => remaining.Contains(s.Id)).ToListAsync(cancellationToken);
                foreach (var s in staffs)
                    result[s.Id] = ("Staff", s.Profile.Email, $"{s.Profile.FirstName} {s.Profile.LastName}".Trim());
            }

            remaining = accountIds.Except(result.Keys).ToList();
            if (remaining.Count > 0)
            {
                var students = await _dbContext.Students.AsNoTracking().Include(s => s.Profile)
                    .Where(s => remaining.Contains(s.Id)).ToListAsync(cancellationToken);
                foreach (var s in students)
                    result[s.Id] = ("Student", s.Profile.Email, $"{s.Profile.FirstName} {s.Profile.LastName}".Trim());
            }

            remaining = accountIds.Except(result.Keys).ToList();
            if (remaining.Count > 0)
            {
                var teachers = await _dbContext.Teachers.AsNoTracking().Include(t => t.Profile)
                    .Where(t => remaining.Contains(t.Id)).ToListAsync(cancellationToken);
                foreach (var t in teachers)
                    result[t.Id] = ("Teacher", t.Profile.Email, $"{t.Profile.FirstName} {t.Profile.LastName}".Trim());
            }

            return result;
        }
    }
}