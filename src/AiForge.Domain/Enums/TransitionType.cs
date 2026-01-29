namespace AiForge.Domain.Enums;

/// <summary>
/// Defines how a skill chain link transitions to the next step
/// </summary>
public enum TransitionType
{
    /// <summary>
    /// Proceed to the next link by position order
    /// </summary>
    NextLink = 0,

    /// <summary>
    /// Jump to a specific link (for loops and conditional flows)
    /// </summary>
    GoToLink = 1,

    /// <summary>
    /// Chain execution is complete (success path)
    /// </summary>
    Complete = 2,

    /// <summary>
    /// Retry the current link (failure path)
    /// </summary>
    Retry = 3,

    /// <summary>
    /// Require human intervention (failure path)
    /// </summary>
    Escalate = 4
}
