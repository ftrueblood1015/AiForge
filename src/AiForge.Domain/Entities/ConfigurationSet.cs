namespace AiForge.Domain.Entities;

public class ConfigurationSet
{
    public Guid Id { get; set; }
    public string SetKey { get; set; } = string.Empty;  // Unique key within scope
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? AgentIds { get; set; }     // JSON array of agent GUIDs
    public string? SkillIds { get; set; }     // JSON array of skill GUIDs
    public string? TemplateIds { get; set; }  // JSON array of template GUIDs
    public string? Version { get; set; }      // Semantic version (e.g., "1.0.0")

    // Scoping: exactly one of these must be set
    public Guid? OrganizationId { get; set; }  // Org-level (shared across projects)
    public Guid? ProjectId { get; set; }       // Project-level (specific to one project)

    // Audit fields
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // Navigation
    public Project? Project { get; set; }
}
