namespace AiForge.Application.DTOs.SkillChains;

public class CreateSkillChainLinkRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid SkillId { get; set; }
    public Guid? AgentId { get; set; }
    public int MaxRetries { get; set; } = 3;
    public string OnSuccessTransition { get; set; } = "NextLink";
    public Guid? OnSuccessTargetLinkId { get; set; }
    public string OnFailureTransition { get; set; } = "Retry";
    public Guid? OnFailureTargetLinkId { get; set; }
    public string? LinkConfig { get; set; }
    public int? Position { get; set; }  // Optional: insert at specific position
}

public class UpdateSkillChainLinkRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public Guid? SkillId { get; set; }
    public Guid? AgentId { get; set; }
    public int? MaxRetries { get; set; }
    public string? OnSuccessTransition { get; set; }
    public Guid? OnSuccessTargetLinkId { get; set; }
    public string? OnFailureTransition { get; set; }
    public Guid? OnFailureTargetLinkId { get; set; }
    public string? LinkConfig { get; set; }
}

public class ReorderLinksRequest
{
    public List<Guid> LinkIdsInOrder { get; set; } = new();
}
