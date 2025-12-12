namespace AiForge.Domain.Entities;

public class PlanningSession
{
    public Guid Id { get; set; }
    public Guid TicketId { get; set; }
    public string? SessionId { get; set; }                       // Claude session identifier
    public string InitialUnderstanding { get; set; } = string.Empty;
    public string? Assumptions { get; set; }                     // JSON array
    public string? AlternativesConsidered { get; set; }          // JSON array
    public string? ChosenApproach { get; set; }
    public string? Rationale { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    // Navigation
    public Ticket Ticket { get; set; } = null!;
}
