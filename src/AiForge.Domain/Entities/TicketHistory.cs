namespace AiForge.Domain.Entities;

public class TicketHistory
{
    public Guid Id { get; set; }
    public Guid TicketId { get; set; }
    public string Field { get; set; } = string.Empty;            // "Status", "Priority", etc.
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string? ChangedBy { get; set; }                       // "user" or session ID
    public DateTime ChangedAt { get; set; }

    // Navigation
    public Ticket Ticket { get; set; } = null!;
}
