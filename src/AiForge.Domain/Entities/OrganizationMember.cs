using AiForge.Domain.Enums;

namespace AiForge.Domain.Entities;

public class OrganizationMember
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid UserId { get; set; }
    public OrganizationRole Role { get; set; }
    public DateTime JoinedAt { get; set; }

    // Navigation
    public Organization Organization { get; set; } = null!;
}
