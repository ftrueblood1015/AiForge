using AiForge.Domain.Enums;

namespace AiForge.Domain.Entities;

public class SessionState
{
    public Guid Id { get; set; }
    public string SessionId { get; set; } = string.Empty;

    // Optional context references
    public Guid? TicketId { get; set; }
    public Guid? QueueId { get; set; }

    // Session state
    public SessionPhase CurrentPhase { get; set; }
    public string? WorkingSummary { get; set; }
    public string? LastCheckpoint { get; set; }

    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }

    // Navigation properties
    public Ticket? Ticket { get; set; }
    public WorkQueue? Queue { get; set; }
}
