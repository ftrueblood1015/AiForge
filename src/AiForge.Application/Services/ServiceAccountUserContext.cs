using AiForge.Application.Interfaces;

namespace AiForge.Application.Services;

/// <summary>
/// A user context implementation for service accounts (like the MCP server)
/// that don't have an HTTP context. Always returns IsServiceAccount = true
/// to bypass user-specific access control.
/// </summary>
public class ServiceAccountUserContext : IUserContext
{
    public Guid? UserId => null;

    public bool IsServiceAccount => true;

    public bool IsAdmin => false;

    public string? Email => null;
}
