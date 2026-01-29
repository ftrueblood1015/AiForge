namespace AiForge.Domain.Entities;

/// <summary>
/// Defines a reusable skill chain workflow template
/// </summary>
public class SkillChain
{
    public Guid Id { get; set; }
    public string ChainKey { get; set; } = string.Empty;  // Unique identifier, e.g., "feature-dev-workflow"
    public string Name { get; set; } = string.Empty;       // Display name
    public string? Description { get; set; }               // Purpose description
    public string? InputSchema { get; set; }               // JSON schema for required inputs
    public int MaxTotalFailures { get; set; } = 5;         // Escalate to human after this many total failures

    // Scoping: exactly one of these must be set
    public Guid? OrganizationId { get; set; }              // Org-level (shared across projects)
    public Guid? ProjectId { get; set; }                   // Project-level (specific to one project)

    public bool IsPublished { get; set; }                  // Available for use

    // Audit fields
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // Navigation
    public Project? Project { get; set; }
    public ICollection<SkillChainLink> Links { get; set; } = new List<SkillChainLink>();
    public ICollection<SkillChainExecution> Executions { get; set; } = new List<SkillChainExecution>();
}
