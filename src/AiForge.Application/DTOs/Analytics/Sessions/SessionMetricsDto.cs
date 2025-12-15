namespace AiForge.Application.DTOs.Analytics.Sessions;

public class SessionMetricsDto
{
    public Guid Id { get; set; }
    public Guid TicketId { get; set; }
    public string TicketKey { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public DateTime SessionStartedAt { get; set; }
    public DateTime? SessionEndedAt { get; set; }
    public int? DurationMinutes { get; set; }
    public int? InputTokens { get; set; }
    public int? OutputTokens { get; set; }
    public int? TotalTokens { get; set; }
    public int DecisionsLogged { get; set; }
    public int ProgressEntriesLogged { get; set; }
    public int FilesModified { get; set; }
    public bool HandoffCreated { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class LogSessionMetricsRequest
{
    public Guid TicketId { get; set; }
    public string SessionId { get; set; } = string.Empty;
    public DateTime? SessionStartedAt { get; set; }
    public DateTime? SessionEndedAt { get; set; }
    public int? DurationMinutes { get; set; }
    public int? InputTokens { get; set; }
    public int? OutputTokens { get; set; }
    public int? TotalTokens { get; set; }
    public int? DecisionsLogged { get; set; }
    public int? ProgressEntriesLogged { get; set; }
    public int? FilesModified { get; set; }
    public bool? HandoffCreated { get; set; }
    public string? Notes { get; set; }
}

public class UpdateSessionMetricsRequest
{
    public Guid? Id { get; set; }
    public string? SessionId { get; set; }
    public DateTime? SessionEndedAt { get; set; }
    public int? DurationMinutes { get; set; }
    public int? InputTokens { get; set; }
    public int? OutputTokens { get; set; }
    public int? TotalTokens { get; set; }
    public int? DecisionsLogged { get; set; }
    public int? ProgressEntriesLogged { get; set; }
    public int? FilesModified { get; set; }
    public bool? HandoffCreated { get; set; }
    public string? Notes { get; set; }
}
