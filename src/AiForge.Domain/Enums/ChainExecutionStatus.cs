namespace AiForge.Domain.Enums;

/// <summary>
/// Status of a skill chain execution instance
/// </summary>
public enum ChainExecutionStatus
{
    /// <summary>
    /// Created but not yet started
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Currently executing
    /// </summary>
    Running = 1,

    /// <summary>
    /// Paused (human intervention required or manual pause)
    /// </summary>
    Paused = 2,

    /// <summary>
    /// Successfully finished all links
    /// </summary>
    Completed = 3,

    /// <summary>
    /// Terminally failed (unrecoverable)
    /// </summary>
    Failed = 4,

    /// <summary>
    /// Manually cancelled
    /// </summary>
    Cancelled = 5
}
