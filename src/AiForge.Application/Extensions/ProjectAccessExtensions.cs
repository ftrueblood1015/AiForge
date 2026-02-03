using AiForge.Application.Interfaces;
using AiForge.Application.Services;

namespace AiForge.Application.Extensions;

/// <summary>
/// Extension methods for project access filtering.
/// Provides consistent access control logic across services.
/// </summary>
public static class ProjectAccessExtensions
{
    /// <summary>
    /// Gets the set of project IDs accessible to the current user.
    /// Returns null if no filtering should be applied (service account or admin).
    /// Returns empty set if user has no access to any projects.
    /// </summary>
    /// <param name="service">The project member service</param>
    /// <param name="userContext">The current user context</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>
    /// null - No filtering needed (service account or admin)
    /// Empty HashSet - User has no project access
    /// Populated HashSet - User's accessible project IDs
    /// </returns>
    public static async Task<HashSet<Guid>?> GetAccessibleProjectIdsOrNullAsync(
        this IProjectMemberService service,
        IUserContext userContext,
        CancellationToken ct = default)
    {
        // Service accounts bypass all project filtering
        if (userContext.IsServiceAccount)
            return null;

        // Admins bypass all project filtering
        if (userContext.IsAdmin)
            return null;

        // Unauthenticated users have no access
        if (!userContext.UserId.HasValue)
            return new HashSet<Guid>();

        // Get the user's accessible projects
        var projectIds = await service.GetAccessibleProjectIdsAsync(userContext.UserId.Value, ct);
        return projectIds.ToHashSet();
    }

    /// <summary>
    /// Checks if filtering should be applied based on user context.
    /// </summary>
    /// <param name="accessibleProjects">Result from GetAccessibleProjectIdsOrNullAsync</param>
    /// <returns>True if filtering should be applied</returns>
    public static bool ShouldFilter(this HashSet<Guid>? accessibleProjects)
        => accessibleProjects != null;

    /// <summary>
    /// Checks if the user has access to a specific project.
    /// </summary>
    /// <param name="accessibleProjects">Result from GetAccessibleProjectIdsOrNullAsync</param>
    /// <param name="projectId">Project ID to check</param>
    /// <returns>True if user has access (or no filtering applies)</returns>
    public static bool HasAccess(this HashSet<Guid>? accessibleProjects, Guid projectId)
        => accessibleProjects == null || accessibleProjects.Contains(projectId);
}
