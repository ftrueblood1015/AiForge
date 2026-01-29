namespace AiForge.Application.DTOs.SkillChains;

/// <summary>
/// Configuration options for automatic session state management during chain execution.
/// </summary>
public class ChainSessionStateOptions
{
    /// <summary>
    /// Whether to automatically save session state after link outcomes are recorded.
    /// Default: true
    /// </summary>
    public bool AutoSaveOnLinkComplete { get; set; } = true;

    /// <summary>
    /// Whether to attempt loading existing session state when starting execution.
    /// If a previous session exists for this ticket, its context can be used.
    /// Default: true
    /// </summary>
    public bool AutoLoadOnStart { get; set; } = true;

    /// <summary>
    /// Whether to automatically clear session state when the chain completes successfully.
    /// Default: true
    /// </summary>
    public bool AutoClearOnComplete { get; set; } = true;

    /// <summary>
    /// Whether to save session state when execution is paused or requires intervention.
    /// Default: true
    /// </summary>
    public bool AutoSaveOnPause { get; set; } = true;

    /// <summary>
    /// Whether to save session state when execution is cancelled.
    /// Default: true
    /// </summary>
    public bool AutoSaveOnCancel { get; set; } = true;

    /// <summary>
    /// Number of hours until saved session state expires.
    /// Default: 24
    /// </summary>
    public int SessionExpiryHours { get; set; } = 24;

    /// <summary>
    /// Optional custom session ID. If not provided, uses "chain-exec-{executionId}".
    /// </summary>
    public string? SessionId { get; set; }

    /// <summary>
    /// Default options with all auto behaviors enabled.
    /// </summary>
    public static ChainSessionStateOptions Default => new();

    /// <summary>
    /// Options with all auto behaviors disabled.
    /// </summary>
    public static ChainSessionStateOptions Disabled => new()
    {
        AutoSaveOnLinkComplete = false,
        AutoLoadOnStart = false,
        AutoClearOnComplete = false,
        AutoSaveOnPause = false,
        AutoSaveOnCancel = false
    };
}
