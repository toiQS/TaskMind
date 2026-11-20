using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using TaskMind.ApplicationLayer.Commons;

namespace TaskMind.InfrastructorLayer.Datas
{
    public class CurrentSessionProvider : ICurrentSessionProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentSessionProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid? GetUserId()
        {
            var principal = _httpContextAccessor.HttpContext?.User;
            var claimValue = principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? principal?.FindFirst("sub")?.Value;
            return Guid.TryParse(claimValue, out var accountId) ? accountId : null;
        }
    }
}
