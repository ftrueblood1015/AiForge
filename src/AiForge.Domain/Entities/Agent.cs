using AiForge.Domain.Enums;

namespace AiForge.Domain.Entities;

public class Agent
{
    public Guid Id { get; set; }
    public string AgentKey { get; set; } = string.Empty;  // Unique within scope (e.g., "engineer", "reviewer")
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? SystemPrompt { get; set; }
    public string? Instructions { get; set; }  // Rich markdown instructions
    public AgentType AgentType { get; set; } = AgentType.Claude;
    public string? Capabilities { get; set; }  // JSON array of capabilities
    public AgentStatus Status { get; set; } = AgentStatus.Idle;

    // Scoping: exactly one of these must be set
    public Guid? OrganizationId { get; set; }  // Org-level (shared across projects)
    public Guid? ProjectId { get; set; }       // Project-level (specific to one project)

    public bool IsEnabled { get; set; } = true;

    // Audit fields
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // Navigation
    public Project? Project { get; set; }
    public ICollection<SkillChainLink> ChainLinks { get; set; } = new List<SkillChainLink>();
}
