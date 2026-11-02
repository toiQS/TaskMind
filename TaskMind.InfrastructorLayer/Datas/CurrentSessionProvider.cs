using System.Security.Claims;
using TaskMind.ApplicationLayer.Commons;
using Microsoft.AspNetCore.Http;

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
            return Guid.Empty;
        }
    }
}
