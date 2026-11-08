namespace TaskMind.ApplicationLayer.Features.Auths.Dtos;

public sealed class LoginResultDto
{
    public Guid AccountId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public string AccessToken { get; init; } = string.Empty;
    public DateTimeOffset AccessTokenExpiresAtUtc { get; init; }
    public string RefreshToken { get; init; } = string.Empty;
    public DateTimeOffset RefreshTokenExpiresAtUtc { get; init; }
}

public sealed class RegisteredAccountDto
{
    public Guid AccountId { get; init; }
    public string Email { get; init; } = string.Empty;
    public bool IsVerified { get; init; }
}

public sealed class CurrentAccountDto
{
    public Guid AccountId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public bool IsVerified { get; init; }
    public string NickName { get; init; } = string.Empty;
    public string ImageUrl { get; init; } = string.Empty;
    public string Bio { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public ProfileAddressDto Address { get; init; } = new();
}

public sealed class ProfileAddressDto
{
    public string Street { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string Country { get; init; } = string.Empty;
}

public sealed class PublicUserProfileDto
{
    public Guid UserId { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string NickName { get; init; } = string.Empty;
    public string ImageUrl { get; init; } = string.Empty;
    public string Bio { get; init; } = string.Empty;
    public IReadOnlyCollection<PublicUserSkillDto> Skills { get; init; } = Array.Empty<PublicUserSkillDto>();
}

public sealed class PublicUserSkillDto
{
    public Guid SkillId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string Level { get; init; } = string.Empty;
    public int EndorsementCount { get; init; }
}
