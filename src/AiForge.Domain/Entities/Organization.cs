namespace AiForge.Domain.Entities;

public class Organization
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;           // URL-friendly unique identifier
    public DateTime CreatedAt { get; set; }
    public Guid CreatedByUserId { get; set; }

    // Navigation
    public ICollection<OrganizationMember> Members { get; set; } = new List<OrganizationMember>();
    public ICollection<Project> Projects { get; set; } = new List<Project>();
}
