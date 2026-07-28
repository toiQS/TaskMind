using TaskMind.Applications.Admins.Dtos;
using TaskMind.Domain.Entities;

namespace TaskMind.Applications.Admins.Mapping
{
    public static class UserMapper
    {
        public static UserDto ToDto(User user) => new UserDto
        {
            Id = user.Id,
            FullName = $"{user.Profile.FirstName} {user.Profile.LastName}".Trim(),
            Email = user.Profile.Email,
            PhoneNumber = user.Profile.PhoneNumber,
            AvatarUrl = user.Profile.ImageUrl,
            Role = user.Role.ToString(),
            Status = user.Status.ToString(),
            IsVerified = user.IsVerified,
            JoinedDateUtc = user.CreatedAtUtc.UtcDateTime
        };

        public static UserDetailDto ToDetailDto(User user) => new UserDetailDto
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
            Bio = user.Profile.Bio,
            Address = $"{user.Profile.Address.Street}, {user.Profile.Address.City}, {user.Profile.Address.Country}"
                .Trim().Trim(',').Trim()
        };
    }
}