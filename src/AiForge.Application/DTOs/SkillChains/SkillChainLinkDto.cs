namespace AiForge.Application.DTOs.SkillChains;

public class SkillChainLinkDto
{
    public Guid Id { get; set; }
    public Guid SkillChainId { get; set; }
    public int Position { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid SkillId { get; set; }
    public string? SkillName { get; set; }
    public string? SkillKey { get; set; }
    public Guid? AgentId { get; set; }
    public string? AgentName { get; set; }
    public string? AgentKey { get; set; }
    public int MaxRetries { get; set; }
    public string OnSuccessTransition { get; set; } = string.Empty;
    public Guid? OnSuccessTargetLinkId { get; set; }
    public string? OnSuccessTargetLinkName { get; set; }
    public string OnFailureTransition { get; set; } = string.Empty;
    public Guid? OnFailureTargetLinkId { get; set; }
    public string? OnFailureTargetLinkName { get; set; }
    public string? LinkConfig { get; set; }
}
