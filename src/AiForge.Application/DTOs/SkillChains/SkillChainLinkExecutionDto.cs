namespace AiForge.Application.DTOs.SkillChains;

public class SkillChainLinkExecutionDto
{
    public Guid Id { get; set; }
    public Guid SkillChainExecutionId { get; set; }
    public Guid SkillChainLinkId { get; set; }
    public string? LinkName { get; set; }
    public int? LinkPosition { get; set; }
    public int AttemptNumber { get; set; }
    public string Outcome { get; set; } = string.Empty;
    public string? Input { get; set; }
    public string? Output { get; set; }
    public string? ErrorDetails { get; set; }
    public string? TransitionTaken { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? ExecutedBy { get; set; }
}
