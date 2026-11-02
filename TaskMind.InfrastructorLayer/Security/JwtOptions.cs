using System.Text;

namespace TaskMind.InfrastructorLayer.Security;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string SigningKey { get; set; } = string.Empty;
    public int AccessTokenMinutes { get; set; } = 15;
    public int RefreshTokenDays { get; set; } = 14;

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Issuer))
            throw new InvalidOperationException("JWT issuer must be configured in Jwt:Issuer.");
        if (string.IsNullOrWhiteSpace(Audience))
            throw new InvalidOperationException("JWT audience must be configured in Jwt:Audience.");
        if (Encoding.UTF8.GetByteCount(SigningKey) < 32)
            throw new InvalidOperationException("Jwt:SigningKey must contain at least 32 UTF-8 bytes.");
        if (AccessTokenMinutes is < 1 or > 60)
            throw new InvalidOperationException("Jwt:AccessTokenMinutes must be between 1 and 60.");
        if (RefreshTokenDays is < 1 or > 30)
            throw new InvalidOperationException("Jwt:RefreshTokenDays must be between 1 and 30.");
    }
}
