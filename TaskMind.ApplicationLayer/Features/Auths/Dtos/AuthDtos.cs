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
    public string FullName { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public bool IsVerified { get; init; }
}
