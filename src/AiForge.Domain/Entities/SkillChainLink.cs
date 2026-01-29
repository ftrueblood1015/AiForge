using AiForge.Domain.Enums;

namespace AiForge.Domain.Entities;

/// <summary>
/// Defines a single step (link) within a skill chain
/// </summary>
public class SkillChainLink
{
    public Guid Id { get; set; }
    public Guid SkillChainId { get; set; }                 // Parent chain
    public int Position { get; set; }                       // Order in chain (0-based)
    public string Name { get; set; } = string.Empty;        // Step name, e.g., "Plan Feature"
    public string? Description { get; set; }                // Step purpose
    public Guid SkillId { get; set; }                       // Required: skill to execute
    public Guid? AgentId { get; set; }                      // Optional: agent to use
    public int MaxRetries { get; set; } = 3;                // Max attempts for this link

    // Transition configuration
    public TransitionType OnSuccessTransition { get; set; } = TransitionType.NextLink;
    public Guid? OnSuccessTargetLinkId { get; set; }        // Target if GoToLink on success
    public TransitionType OnFailureTransition { get; set; } = TransitionType.Retry;
    public Guid? OnFailureTargetLinkId { get; set; }        // Target if GoToLink on failure (for loops)

    public string? LinkConfig { get; set; }                 // JSON: additional link-specific config

    // Navigation
    public SkillChain SkillChain { get; set; } = null!;
    public Skill Skill { get; set; } = null!;
    public Agent? Agent { get; set; }
    public SkillChainLink? OnSuccessTargetLink { get; set; }
    public SkillChainLink? OnFailureTargetLink { get; set; }
    public ICollection<SkillChainLinkExecution> LinkExecutions { get; set; } = new List<SkillChainLinkExecution>();
}
