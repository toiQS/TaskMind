// ITokenService.cs
using TaskMind.Domain.Enums;

namespace TaskMind.Applications.Auths.Interfaces
{
    /// <summary>
    /// Sinh access/refresh token cho phiên đăng nhập.
    /// TODO: implementation mặc định (GuidTokenService) chỉ là placeholder, KHÔNG phải JWT thật.
    /// Cần thay bằng implementation JWT thật ở tầng Infrastructure trước khi lên production.
    /// </summary>
    public interface ITokenService
    {
        string GenerateAccessToken(Guid accountId, string email, AccountRole role);
        string GenerateRefreshToken();
    }
}