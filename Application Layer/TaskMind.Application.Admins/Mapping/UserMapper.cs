using TaskMind.Applications.Admins.Dtos;
using TaskMind.Domain.Entities;

namespace TaskMind.Applications.Admins.Mapping
{
    public static class UserMapper
    {
        public static UserDto ToDto(User user, int skillCount = 0, int projectCount = 0) => new UserDto
        {
            Id = user.Id,
            FullName = $"{user.Profile.FirstName} {user.Profile.LastName}".Trim(),
            Email = user.Profile.Email,
            PhoneNumber = user.Profile.PhoneNumber,
            AvatarUrl = user.Profile.ImageUrl,
            Role = user.Role.ToString(),
            Status = user.Status.ToString(),
            IsVerified = user.IsVerified,
            JoinedDateUtc = user.CreatedAtUtc.UtcDateTime,
            SkillCount = skillCount,
            ProjectCount = projectCount
        };

        public static UserDetailDto ToDetailDto(
            User user,
            int skillCount = 0,
            int projectCount = 0,
            List<UserSkillItemDto>? skills = null,
            List<UserProjectHistoryItemDto>? projectHistory = null,
            List<AuditLogEntryDto>? auditLogs = null) => new UserDetailDto
        {
            Id = user.Id,
            FullName = $"{user.Profile.FirstName} {user.Profile.LastName}".Trim(),
            Email = user.Profile.Email,
            PhoneNumber = user.Profile.PhoneNumber,
            AvatarUrl = user.Profile.ImageUrl,
            Role = user.Role.ToString(),
            Status = user.Status.ToString(),
            IsVerified = user.IsVerified,
            JoinedDateUtc = user.CreatedAtUtc.UtcDateTime,
            SkillCount = skillCount,
            ProjectCount = projectCount,
            Bio = user.Profile.Bio,
            Address = $"{user.Profile.Address.Street}, {user.Profile.Address.City}, {user.Profile.Address.Country}"
                .Trim().Trim(',').Trim(),
            LastActiveDateUtc = user.UpdatedAtUtc?.UtcDateTime,
            Skills = skills ?? new List<UserSkillItemDto>(),
            ProjectHistory = projectHistory ?? new List<UserProjectHistoryItemDto>(),
            AuditLogs = auditLogs ?? new List<AuditLogEntryDto>()
        };
    }
}
