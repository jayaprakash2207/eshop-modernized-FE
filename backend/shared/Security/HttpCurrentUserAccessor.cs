using System.Security.Claims;
using BuildingBlocks.Security;
using Microsoft.AspNetCore.Http;

namespace Security;

public sealed class HttpCurrentUserAccessor : ICurrentUserAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpCurrentUserAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public CurrentUser? GetCurrentUser()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        var id = user.FindFirstValue(ClaimTypes.NameIdentifier);
        var username = user.Identity?.Name;
        var role = user.FindFirstValue(ClaimTypes.Role);

        if (!Guid.TryParse(id, out var userId) || string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(role))
        {
            return null;
        }

        return new CurrentUser(userId, username, role);
    }
}
