namespace AiForge.Application.DTOs.SkillChains;

public class SkillChainExecutionDto
{
    public Guid Id { get; set; }
    public Guid SkillChainId { get; set; }
    public string? ChainKey { get; set; }
    public string? ChainName { get; set; }
    public Guid? TicketId { get; set; }
    public string? TicketKey { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid? CurrentLinkId { get; set; }
    public string? CurrentLinkName { get; set; }
    public int? CurrentLinkPosition { get; set; }
    public string? InputValues { get; set; }
    public string? ExecutionContext { get; set; }
    public int TotalFailureCount { get; set; }
    public bool RequiresHumanIntervention { get; set; }
    public string? InterventionReason { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? StartedBy { get; set; }
    public string? CompletedBy { get; set; }
    public List<SkillChainLinkExecutionDto> LinkExecutions { get; set; } = new();

    // Session state integration
    public string? SessionId { get; set; }
    public bool SessionStateEnabled { get; set; }
    public string? SessionPhase { get; set; }
    public DateTime? SessionStateUpdatedAt { get; set; }
}

public class SkillChainExecutionSummaryDto
{
    public Guid Id { get; set; }
    public Guid SkillChainId { get; set; }
    public string? ChainName { get; set; }
    public Guid? TicketId { get; set; }
    public string? TicketKey { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? CurrentLinkName { get; set; }
    public int TotalFailureCount { get; set; }
    public bool RequiresHumanIntervention { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
