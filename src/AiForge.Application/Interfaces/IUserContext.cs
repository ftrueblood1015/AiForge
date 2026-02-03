namespace AiForge.Application.Interfaces;

/// <summary>
/// Provides access to the current authenticated user's context.
/// Implemented in the API layer using HttpContext.
/// </summary>
public interface IUserContext
{
    /// <summary>
    /// The current user's ID, or null if not authenticated.
    /// </summary>
    Guid? UserId { get; }

    /// <summary>
    /// Whether the current request is authenticated via a service account (API key).
    /// Service accounts may bypass certain user-specific filtering.
    /// </summary>
    bool IsServiceAccount { get; }

    /// <summary>
    /// Whether the current user is a system admin.
    /// </summary>
    bool IsAdmin { get; }

    /// <summary>
    /// The current user's email, if available.
    /// </summary>
    string? Email { get; }
}
