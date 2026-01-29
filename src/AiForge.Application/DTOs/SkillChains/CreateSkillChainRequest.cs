namespace AiForge.Application.DTOs.SkillChains;

public class CreateSkillChainRequest
{
    public string ChainKey { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? InputSchema { get; set; }
    public int MaxTotalFailures { get; set; } = 5;
    public Guid? OrganizationId { get; set; }
    public Guid? ProjectId { get; set; }
}

public class UpdateSkillChainRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? InputSchema { get; set; }
    public int? MaxTotalFailures { get; set; }
    public bool? IsPublished { get; set; }
}
