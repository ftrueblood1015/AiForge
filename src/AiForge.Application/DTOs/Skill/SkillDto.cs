namespace AiForge.Application.DTOs.Skill;

public class SkillDto
{
    public Guid Id { get; set; }
    public string SkillKey { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Content { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public Guid? OrganizationId { get; set; }
    public Guid? ProjectId { get; set; }
    public string Scope { get; set; } = string.Empty;  // "Organization" or "Project"
    public bool IsPublished { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}

public class SkillListItemDto
{
    public Guid Id { get; set; }
    public string SkillKey { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Scope { get; set; } = string.Empty;
    public bool IsPublished { get; set; }
}

public class CreateSkillRequest
{
    public string SkillKey { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Content { get; set; } = string.Empty;
    public string Category { get; set; } = "Custom";  // Workflow, Analysis, Documentation, Generation, Testing, Custom
    public Guid? OrganizationId { get; set; }
    public Guid? ProjectId { get; set; }
}

public class UpdateSkillRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Content { get; set; }
    public string? Category { get; set; }
    public bool? IsPublished { get; set; }
}

public class SkillListResponse
{
    public List<SkillListItemDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
}
