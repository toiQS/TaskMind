using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TaskMind.ApplicationLayer.Commons;
using TaskMind.DomainLayer.TaskMind.Domain.Enums;

namespace TaskMind.InfrastructorLayer.Security;

public sealed class JwtAuthenticationTokenService : IAuthenticationTokenService
{
    private readonly JwtOptions _options;
    private readonly SigningCredentials _signingCredentials;

    public JwtAuthenticationTokenService(IOptions<JwtOptions> options)
    {
        _options = options.Value;
        _options.Validate();
        _signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey)),
            SecurityAlgorithms.HmacSha256);
    }

    public AuthenticationTokenPair CreateTokenPair(Guid accountId, string email, AccountRole role)
    {
        var issuedAtUtc = DateTimeOffset.UtcNow;
        var accessExpiryUtc = issuedAtUtc.AddMinutes(_options.AccessTokenMinutes);
        var refreshExpiryUtc = issuedAtUtc.AddDays(_options.RefreshTokenDays);
        var claims = new[]
        {
            new System.Security.Claims.Claim(JwtRegisteredClaimNames.Sub, accountId.ToString()),
            new System.Security.Claims.Claim(JwtRegisteredClaimNames.Email, email),
            new System.Security.Claims.Claim("role", role.ToString()),
            new System.Security.Claims.Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        var token = new JwtSecurityToken(
            _options.Issuer,
            _options.Audience,
            claims,
            issuedAtUtc.UtcDateTime,
            accessExpiryUtc.UtcDateTime,
            _signingCredentials);

        return new AuthenticationTokenPair(
            new JwtSecurityTokenHandler().WriteToken(token),
            accessExpiryUtc,
            CreateOpaqueToken(),
            refreshExpiryUtc);
    }

    public string CreateOpaqueToken() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(64))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');

    public string HashToken(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
