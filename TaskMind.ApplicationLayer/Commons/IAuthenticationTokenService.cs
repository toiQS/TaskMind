using TaskMind.DomainLayer.TaskMind.Domain.Enums;

namespace TaskMind.ApplicationLayer.Commons;

public sealed record AuthenticationTokenPair(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAtUtc,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAtUtc);

public interface IAuthenticationTokenService
{
    AuthenticationTokenPair CreateTokenPair(Guid accountId, string email, AccountRole role);
    string CreateOpaqueToken();
    string HashToken(string token);
}