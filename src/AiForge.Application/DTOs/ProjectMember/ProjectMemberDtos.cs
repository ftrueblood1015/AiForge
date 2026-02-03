using AiForge.Domain.Enums;

namespace AiForge.Application.DTOs.ProjectMember;

public class ProjectMemberDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public ProjectRole Role { get; set; }
    public DateTime AddedAt { get; set; }
    public Guid? AddedByUserId { get; set; }
    public string? AddedByUserName { get; set; }
}

public class AddProjectMemberRequest
{
    public string Email { get; set; } = string.Empty;
    public ProjectRole Role { get; set; } = ProjectRole.Member;
}

public class UpdateProjectMemberRoleRequest
{
    public ProjectRole Role { get; set; }
}

/// <summary>
/// Lightweight DTO for user search results when adding project members
/// </summary>
public class UserSearchResultDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
}
