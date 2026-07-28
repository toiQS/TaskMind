using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Admins.Dtos;
using TaskMind.Applications.Admins.Mapping;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Enums;

namespace TaskMind.Applications.Admins.Features.Users
{
    public class GetUsersQuery : IRequest<List<UserDto>>
    {
        public string? SearchText { get; set; }

        /// <summary>"All" | "Active" | "Locked" | "Banned"</summary>
        public string StatusFilter { get; set; } = "All";
    }

    public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, List<UserDto>>
    {
        private readonly IApplicationDbContext _db;

        public GetUsersQueryHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
        {
            var query = _db.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.StatusFilter) && request.StatusFilter != "All")
            {
                var status = MapUiStatusToEntityStatus(request.StatusFilter);
                query = query.Where(u => u.Status == status);
            }

            var users = await query.ToListAsync(cancellationToken);

            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                var s = request.SearchText.Trim();
                users = users.Where(u =>
                        $"{u.Profile.FirstName} {u.Profile.LastName}".Contains(s, StringComparison.OrdinalIgnoreCase) ||
                        u.Profile.Email.Contains(s, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            if (users.Count == 0)
                return new List<UserDto>();

            var userIds = users.Select(u => u.Id).ToList();

            // Số kỹ năng đã khai báo (SkillProfile.Records) theo từng UserId.
            var skillCounts = await _db.SkillProfiles
                .Where(sp => userIds.Contains(sp.UserId))
                .Select(sp => new { sp.UserId, Count = sp.Records.Count })
                .ToListAsync(cancellationToken);
            var skillCountMap = skillCounts.ToDictionary(x => x.UserId, x => x.Count);

            // Số dự án đang/đã tham gia (qua ProjectMember.AccountId).
            var projectCounts = await _db.Projects
                .SelectMany(p => p.Members, (p, m) => new { m.AccountId })
                .Where(x => userIds.Contains(x.AccountId))
                .GroupBy(x => x.AccountId)
                .Select(g => new { UserId = g.Key, Count = g.Count() })
                .ToListAsync(cancellationToken);
            var projectCountMap = projectCounts.ToDictionary(x => x.UserId, x => x.Count);

            return users
                .Select(u => UserMapper.ToDto(
                    u,
                    skillCountMap.GetValueOrDefault(u.Id),
                    projectCountMap.GetValueOrDefault(u.Id)))
                .ToList();
        }

        internal static EntityStatus MapUiStatusToEntityStatus(string uiStatus) => uiStatus switch
        {
            "Locked" => EntityStatus.Paused,
            "Banned" => EntityStatus.Blocked,
            _ => EntityStatus.Active
        };
    }
}
