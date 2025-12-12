using AiForge.Domain.Enums;

namespace AiForge.Domain.Entities;

public class ProgressEntry
{
    public Guid Id { get; set; }
    public Guid TicketId { get; set; }
    public string? SessionId { get; set; }
    public string Content { get; set; } = string.Empty;          // What was done/attempted
    public ProgressOutcome Outcome { get; set; }
    public string? FilesAffected { get; set; }                   // JSON array
    public string? ErrorDetails { get; set; }                    // If failed
    public DateTime CreatedAt { get; set; }

    // Navigation
    public Ticket Ticket { get; set; } = null!;
}
