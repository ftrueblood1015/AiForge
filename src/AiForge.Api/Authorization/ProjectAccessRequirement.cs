using Microsoft.AspNetCore.Authorization;

namespace AiForge.Api.Authorization;

/// <summary>
/// Requirement that the user has at least a specified role on the project.
/// </summary>
public class ProjectAccessRequirement : IAuthorizationRequirement
{
    public AiForge.Domain.Enums.ProjectRole MinimumRole { get; }

    public ProjectAccessRequirement(AiForge.Domain.Enums.ProjectRole minimumRole = AiForge.Domain.Enums.ProjectRole.Viewer)
    {
        MinimumRole = minimumRole;
    }
}

/// <summary>
/// Policy names for project access authorization.
/// </summary>
public static class AuthorizationPolicies
{
    /// <summary>Requires user to be a system admin.</summary>
    public const string RequireAdmin = "RequireAdmin";

    /// <summary>Requires user to have any access to the project (Viewer, Member, or Owner).</summary>
    public const string RequireProjectAccess = "RequireProjectAccess";

    /// <summary>Requires user to be at least a Member on the project.</summary>
    public const string RequireProjectMember = "RequireProjectMember";

    /// <summary>Requires user to be an Owner on the project.</summary>
    public const string RequireProjectOwner = "RequireProjectOwner";
}
