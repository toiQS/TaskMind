using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Admins.Dtos;
using TaskMind.Applications.Admins.Mapping;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Enums;

namespace TaskMind.Applications.Admins.Features.Users
{
    public class GetUserDetailQuery : IRequest<UserDetailDto>
    {
        public Guid UserId { get; set; }
    }

    public class GetUserDetailQueryHandler : IRequestHandler<GetUserDetailQuery, UserDetailDto>
    {
        private readonly IApplicationDbContext _db;

        public GetUserDetailQueryHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<UserDetailDto> Handle(GetUserDetailQuery request, CancellationToken cancellationToken)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken)
                ?? throw new KeyNotFoundException("Không tìm thấy người dùng.");

            // ----- Hồ sơ kỹ năng cá nhân (SkillProfile, mục 4.3) -----
            var skillProfile = await _db.SkillProfiles
                .FirstOrDefaultAsync(sp => sp.UserId == request.UserId, cancellationToken);

            var skillDtos = new List<UserSkillItemDto>();
            if (skillProfile != null && skillProfile.Records.Count > 0)
            {
                var skillIds = skillProfile.Records.Select(r => r.SkillId).ToList();
                var skillNames = await _db.Skills
                    .Where(s => skillIds.Contains(s.Id))
                    .ToDictionaryAsync(s => s.Id, s => s.SkillName, cancellationToken);

                skillDtos = skillProfile.Records.Select(r => new UserSkillItemDto
                {
                    SkillId = r.SkillId,
                    SkillName = skillNames.GetValueOrDefault(r.SkillId, "(Kỹ năng không xác định)"),
                    Level = r.Level.ToString(),
                    EndorsementCount = r.EndorsementCount
                }).ToList();
            }

            // ----- Lịch sử tham gia dự án (Project + ProjectMember) -----
            var projects = await _db.Projects
                .Where(p => p.Members.Any(m => m.AccountId == request.UserId))
                .ToListAsync(cancellationToken);

            var projectHistory = projects
                .SelectMany(p => p.Members
                    .Where(m => m.AccountId == request.UserId)
                    .Select(m => new UserProjectHistoryItemDto
                    {
                        ProjectId = p.Id,
                        ProjectName = p.Title,
                        ProjectRole = m.Role.ToString(),
                        ProjectSource = p.SourceType.ToString(),
                        StartDateUtc = m.JoinedAt,
                        EndDateUtc = m.LeftAt,
                        IsOngoing = m.IsActive
                    }))
                .OrderByDescending(x => x.StartDateUtc)
                .ToList();

            // ----- Nhật ký hoạt động tài khoản (Audit Log, mục 5.7) -----
            var userIdText = request.UserId.ToString();
            var auditLogs = await _db.AuditTrails
                .Where(a => a.PrimaryKey == userIdText)
                .OrderByDescending(a => a.DateUtc)
                .Take(50)
                .Select(a => new AuditLogEntryDto
                {
                    Id = a.Id,
                    EntityName = a.EntityName,
                    Action = a.TrailType.ToString(),
                    Description = string.Join(", ", a.ChangedColumns),
                    PerformedBy = a.UserId.HasValue ? a.UserId.Value.ToString() : "System",
                    TimestampUtc = a.DateUtc
                })
                .ToListAsync(cancellationToken);

            return UserMapper.ToDetailDto(
                user,
                skillCount: skillDtos.Count,
                projectCount: projectHistory.Count,
                skills: skillDtos,
                projectHistory: projectHistory,
                auditLogs: auditLogs);
        }
    }
}
