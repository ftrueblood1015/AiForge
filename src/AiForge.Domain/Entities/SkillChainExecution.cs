using AiForge.Domain.Enums;

namespace AiForge.Domain.Entities;

/// <summary>
/// Represents a runtime instance of a skill chain execution
/// </summary>
public class SkillChainExecution
{
    public Guid Id { get; set; }
    public Guid SkillChainId { get; set; }                  // Chain being executed
    public Guid? TicketId { get; set; }                     // Optional: associated ticket
    public ChainExecutionStatus Status { get; set; } = ChainExecutionStatus.Pending;
    public Guid? CurrentLinkId { get; set; }                // Current position in chain
    public string? InputValues { get; set; }                // JSON: user-provided input values
    public string? ExecutionContext { get; set; }           // JSON: accumulated context/outputs
    public int TotalFailureCount { get; set; }              // Cumulative failures across all links
    public bool RequiresHumanIntervention { get; set; }     // Escalation flag
    public string? InterventionReason { get; set; }         // Why human intervention is needed

    // Timing
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? StartedBy { get; set; }                  // Session/user that started
    public string? CompletedBy { get; set; }                // Session/user that completed (or "system")

    // Navigation
    public SkillChain SkillChain { get; set; } = null!;
    public Ticket? Ticket { get; set; }
    public SkillChainLink? CurrentLink { get; set; }
    public ICollection<SkillChainLinkExecution> LinkExecutions { get; set; } = new List<SkillChainLinkExecution>();
}
