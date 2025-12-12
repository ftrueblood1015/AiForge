namespace AiForge.Domain.Entities;

public class ReasoningLog
{
    public Guid Id { get; set; }
    public Guid TicketId { get; set; }
    public string? SessionId { get; set; }
    public string DecisionPoint { get; set; } = string.Empty;    // What decision was made
    public string? OptionsConsidered { get; set; }               // JSON array
    public string ChosenOption { get; set; } = string.Empty;
    public string Rationale { get; set; } = string.Empty;
    public int? ConfidencePercent { get; set; }                  // 0-100
    public DateTime CreatedAt { get; set; }

    // Navigation
    public Ticket Ticket { get; set; } = null!;
}
