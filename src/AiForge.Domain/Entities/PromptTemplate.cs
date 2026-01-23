namespace AiForge.Domain.Entities;

public class PromptTemplate
{
    public Guid Id { get; set; }
    public string TemplateKey { get; set; } = string.Empty;  // Unique key within scope
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Content { get; set; } = string.Empty;  // Template content with {{variables}}
    public string? Variables { get; set; }  // JSON schema: [{ name, type, required, default, description }]
    public string? Category { get; set; }   // Grouping category

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
