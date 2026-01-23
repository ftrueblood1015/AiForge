using AiForge.Domain.Enums;

namespace AiForge.Domain.Entities;

public class Skill
{
    public Guid Id { get; set; }
    public string SkillKey { get; set; } = string.Empty;  // Unique key / slash command (e.g., "commit", "review-pr")
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Content { get; set; } = string.Empty;  // Full markdown content/instructions
    public SkillCategory Category { get; set; } = SkillCategory.Custom;

    // Scoping: exactly one of these must be set
    public Guid? OrganizationId { get; set; }  // Org-level (shared across projects)
    public Guid? ProjectId { get; set; }       // Project-level (specific to one project)

    public bool IsPublished { get; set; }

    // Audit fields
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // Navigation
    public Project? Project { get; set; }
}
