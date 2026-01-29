using AiForge.Domain.Enums;

namespace AiForge.Domain.Entities;

/// <summary>
/// Records a single execution attempt of a chain link
/// </summary>
public class SkillChainLinkExecution
{
    public Guid Id { get; set; }
    public Guid SkillChainExecutionId { get; set; }         // Parent execution
    public Guid SkillChainLinkId { get; set; }              // Link being executed
    public int AttemptNumber { get; set; }                  // 1, 2, 3... for retries
    public LinkExecutionOutcome Outcome { get; set; } = LinkExecutionOutcome.Pending;
    public string? Input { get; set; }                      // JSON: input provided to this execution
    public string? Output { get; set; }                     // JSON: output/result from execution
    public string? ErrorDetails { get; set; }               // Error message if failed
    public TransitionType? TransitionTaken { get; set; }    // Which transition was actually taken

    // Timing
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? ExecutedBy { get; set; }                 // Agent key or session ID

    // Navigation
    public SkillChainExecution SkillChainExecution { get; set; } = null!;
    public SkillChainLink SkillChainLink { get; set; } = null!;
}
