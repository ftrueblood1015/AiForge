using System.Security.Claims;
using AiForge.Application.Services;
using Microsoft.AspNetCore.Authorization;

namespace AiForge.Api.Authorization;

/// <summary>
/// Authorization handler that checks if the user has the required role on a project.
/// The project ID is extracted from the route data.
/// </summary>
public class ProjectAccessHandler : AuthorizationHandler<ProjectAccessRequirement>
{
    private readonly IServiceProvider _serviceProvider;

    public ProjectAccessHandler(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ProjectAccessRequirement requirement)
    {
        // Service accounts bypass project access checks
        var isServiceAccountClaim = context.User.FindFirst("IsServiceAccount")?.Value;
        if (bool.TryParse(isServiceAccountClaim, out var isServiceAccount) && isServiceAccount)
        {
            context.Succeed(requirement);
            return;
        }

        // System admins bypass project access checks
        if (context.User.IsInRole("Admin"))
        {
            context.Succeed(requirement);
            return;
        }

        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return; // Not authenticated
        }

        // Get project ID from route data
        if (context.Resource is not HttpContext httpContext)
        {
            return;
        }

        var projectId = GetProjectIdFromRoute(httpContext);
        if (projectId == null)
        {
            // No project ID in route - let it pass (controller will handle)
            context.Succeed(requirement);
            return;
        }

        // Check access using the project member service
        using var scope = _serviceProvider.CreateScope();
        var projectMemberService = scope.ServiceProvider.GetRequiredService<IProjectMemberService>();

        var hasRole = await projectMemberService.HasRoleAsync(projectId.Value, userId, requirement.MinimumRole);
        if (hasRole)
        {
            context.Succeed(requirement);
        }
    }

    private static Guid? GetProjectIdFromRoute(HttpContext httpContext)
    {
        // Try to get projectId from route values
        if (httpContext.Request.RouteValues.TryGetValue("projectId", out var projectIdValue))
        {
            if (Guid.TryParse(projectIdValue?.ToString(), out var projectId))
            {
                return projectId;
            }
        }

        // Try to get id from route (for project-specific endpoints)
        if (httpContext.Request.RouteValues.TryGetValue("id", out var idValue))
        {
            if (Guid.TryParse(idValue?.ToString(), out var id))
            {
                return id;
            }
        }

        return null;
    }
}
