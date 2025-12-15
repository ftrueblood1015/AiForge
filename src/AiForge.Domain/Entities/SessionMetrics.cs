namespace AiForge.Domain.Entities;

public class SessionMetrics
{
    public Guid Id { get; set; }
    public Guid TicketId { get; set; }

    public string SessionId { get; set; } = string.Empty;
    public DateTime SessionStartedAt { get; set; }
    public DateTime? SessionEndedAt { get; set; }
    public int? DurationMinutes { get; set; }

    // Token tracking (reported by Claude)
    public int? InputTokens { get; set; }
    public int? OutputTokens { get; set; }
    public int? TotalTokens { get; set; }

    // Activity counts
    public int DecisionsLogged { get; set; }
    public int ProgressEntriesLogged { get; set; }
    public int FilesModified { get; set; }
    public bool HandoffCreated { get; set; }

    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation
    public Ticket Ticket { get; set; } = null!;
}
