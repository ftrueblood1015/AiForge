namespace AiForge.Application.DTOs.SkillChains;

public class StartChainExecutionRequest
{
    public Guid SkillChainId { get; set; }
    public Guid? TicketId { get; set; }
    public string? InputValues { get; set; }  // JSON object matching InputSchema
    public string? StartedBy { get; set; }

    /// <summary>
    /// Options for automatic session state management during execution.
    /// If null, defaults to ChainSessionStateOptions.Default (all auto behaviors enabled).
    /// </summary>
    public ChainSessionStateOptions? SessionStateOptions { get; set; }
}

public class RecordLinkOutcomeRequest
{
    public Guid LinkId { get; set; }
    public string Outcome { get; set; } = string.Empty;  // Success, Failure
    public string? Output { get; set; }  // JSON output from execution
    public string? ErrorDetails { get; set; }
    public string? ExecutedBy { get; set; }
}

public class ResumeExecutionRequest
{
    public string? ResumedBy { get; set; }
    public string? AdditionalContext { get; set; }  // JSON to merge into ExecutionContext
}

public class ResolveInterventionRequest
{
    public string Resolution { get; set; } = string.Empty;  // Description of resolution
    public string NextAction { get; set; } = "Retry";  // Retry, GoToLink, Cancel
    public Guid? TargetLinkId { get; set; }  // Required if NextAction is GoToLink
    public string? ResolvedBy { get; set; }
}
