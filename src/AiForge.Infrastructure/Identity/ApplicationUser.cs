using AiForge.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace AiForge.Infrastructure.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
    public string DisplayName { get; set; } = string.Empty;
    public Guid? DefaultOrganizationId { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation
    public Organization? DefaultOrganization { get; set; }
    public ICollection<OrganizationMember> OrganizationMemberships { get; set; } = new List<OrganizationMember>();
    public ICollection<ProjectMember> ProjectMemberships { get; set; } = new List<ProjectMember>();
    public ICollection<ApiKey> ApiKeys { get; set; } = new List<ApiKey>();
}
