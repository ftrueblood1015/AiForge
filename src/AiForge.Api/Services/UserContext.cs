using System.Security.Claims;
using AiForge.Application.Interfaces;

namespace AiForge.Api.Services;

/// <summary>
/// Provides access to the current authenticated user's context from HttpContext.
/// </summary>
public class UserContext : IUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId
    {
        get
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }

    public bool IsServiceAccount
    {
        get
        {
            var isServiceAccountClaim = _httpContextAccessor.HttpContext?.User.FindFirst("IsServiceAccount")?.Value;
            return bool.TryParse(isServiceAccountClaim, out var isServiceAccount) && isServiceAccount;
        }
    }

    public bool IsAdmin
    {
        get
        {
            return _httpContextAccessor.HttpContext?.User.IsInRole("Admin") ?? false;
        }
    }

    public string? Email
    {
        get
        {
            return _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Email)?.Value;
        }
    }
}
