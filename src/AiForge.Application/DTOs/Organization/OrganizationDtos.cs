using AiForge.Domain.Enums;

namespace AiForge.Application.DTOs.Organization;

public class OrganizationDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public Guid CreatedByUserId { get; set; }
    public int MemberCount { get; set; }
    public int ProjectCount { get; set; }
}

public class OrganizationMemberDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public OrganizationRole Role { get; set; }
    public DateTime JoinedAt { get; set; }
}

public class CreateOrganizationRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Slug { get; set; }
}

public class UpdateOrganizationRequest
{
    public string? Name { get; set; }
}

public class AddOrganizationMemberRequest
{
    public string Email { get; set; } = string.Empty;
    public OrganizationRole Role { get; set; } = OrganizationRole.Member;
}

public class UpdateMemberRoleRequest
{
    public OrganizationRole Role { get; set; }
}
