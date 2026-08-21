using System;
using System.Collections.Generic;
using System.Text;
using TaskMind.Domain.Enums;

namespace TaskMind.Applications.Auths.Common
{
    /// <summary>Placeholder — sinh chuỗi ngẫu nhiên, KHÔNG phải JWT thật. Thay bằng JWT thật ở Infrastructure.</summary>
    public class GuidTokenService : ITokenService
    {
        public string GenerateAccessToken(Guid accountId, string email, AccountRole role)
            => $"{Convert.ToBase64String(Guid.NewGuid().ToByteArray())}.{accountId}";

        public string GenerateRefreshToken()
            => Convert.ToBase64String(Guid.NewGuid().ToByteArray());
    }
}
