namespace AiForge.Domain.Enums;

/// <summary>
/// Outcome of a single link execution attempt
/// </summary>
public enum LinkExecutionOutcome
{
    /// <summary>
    /// Execution not yet complete
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Completed successfully
    /// </summary>
    Success = 1,

    /// <summary>
    /// Failed (may retry depending on configuration)
    /// </summary>
    Failure = 2,

    /// <summary>
    /// Skipped (chain cancelled or jumped past)
    /// </summary>
    Skipped = 3
}
