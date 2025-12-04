using BackEnd.Domain.Interfaces;
using System.Security.Claims;

namespace BackEnd.Service.Implementations
{
    public class CurrentUserServiceImpl(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        public string GetUserId()
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            ArgumentNullException.ThrowIfNull(userId, nameof(userId));
            return userId;
        }
    }
}
