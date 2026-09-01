using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Admins.Dtos;
using TaskMind.Applications.Commons;

namespace TaskMind.Applications.Admins.Features.Jobs
{
    public class GetJobPostingDetailQuery : ServiceResult<JobPostingDetailDto>
    {
        public Guid JobPostingId { get; }

        public GetJobPostingDetailQuery(Guid jobPostingId)
        {
            JobPostingId = jobPostingId;
        }
    }

    public class GetJobPostingDetailHandler
    {
        private readonly IApplicationDbContext _dbContext;

        public GetJobPostingDetailHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ServiceResult<JobPostingDetailDto>> Handle(GetJobPostingDetailQuery query, CancellationToken cancellationToken)
        {
            var posting = await _dbContext.JobPostings
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == query.JobPostingId, cancellationToken);

            if (posting == null)
                return ServiceResult<JobPostingDetailDto>.NotFound("Không tìm thấy tin tuyển dụng.");

            var company = await _dbContext.Companies
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == posting.CompanyId, cancellationToken);

            var requiredSkills = await _dbContext.Skills
                .AsNoTracking()
                .Where(s => posting.RequiredSkillIds.Contains(s.Id))
                .Select(s => new RequiredSkillDto { SkillId = s.Id, SkillName = s.SkillName })
                .ToListAsync(cancellationToken);

            var applications = await _dbContext.JobApplications
    .AsNoTracking()
    .Where(a => a.JobPostingId == posting.Id)   // đã xoá dòng .Include(a => a.GetType())
    .OrderByDescending(a => a.AppliedAtUtc)
    .Take(10)
    .ToListAsync(cancellationToken);

            var userIds = applications.Select(a => a.UserId).Distinct().ToList();
            var users = await _dbContext.Users
                .AsNoTracking()
                .Include(u => u.Profile)
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, cancellationToken);

            var recentApplications = applications.Select(a => new JobApplicationListItemDto
            {
                Id = a.Id,
                JobPostingId = a.JobPostingId,
                JobPostingTitle = posting.Title,
                UserId = a.UserId,
                UserEmail = users.TryGetValue(a.UserId, out var u) ? u.Profile.Email : string.Empty,
                UserFullName = users.TryGetValue(a.UserId, out var u2) ? $"{u2.Profile.FirstName} {u2.Profile.LastName}".Trim() : string.Empty,
                ApplicationStatus = a.ApplicationStatus,
                AppliedAtUtc = a.AppliedAtUtc
            }).ToList();

            var dto = new JobPostingDetailDto
            {
                Id = posting.Id,
                CompanyId = posting.CompanyId,
                CompanyName = company?.CompanyName ?? string.Empty,
                Title = posting.Title,
                PostingStatus = posting.PostingStatus,
                RequiredSkills = requiredSkills,
                ApplicationCount = await _dbContext.JobApplications.CountAsync(a => a.JobPostingId == posting.Id, cancellationToken),
                RecentApplications = recentApplications
            };

            return ServiceResult<JobPostingDetailDto>.Success(dto, "Lấy chi tiết tin tuyển dụng thành công");
        }
    }
}