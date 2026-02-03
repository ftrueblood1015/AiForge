namespace AiForge.Application.DTOs.SkillChains;

public class SkillChainDto
{
    public Guid Id { get; set; }
    public string ChainKey { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? InputSchema { get; set; }
    public int MaxTotalFailures { get; set; }
    public Guid? OrganizationId { get; set; }
    public Guid? ProjectId { get; set; }
    public string Scope { get; set; } = string.Empty;  // "Organization" or "Project"
    public bool IsPublished { get; set; }
    public List<SkillChainLinkDto> Links { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}

public class SkillChainSummaryDto
{
    public Guid Id { get; set; }
    public string ChainKey { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Scope { get; set; } = string.Empty;
    public string? ProjectName { get; set; }
    public bool IsPublished { get; set; }
    public int LinkCount { get; set; }
    public int ExecutionCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
