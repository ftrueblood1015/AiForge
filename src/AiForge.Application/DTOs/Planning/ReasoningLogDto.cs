namespace AiForge.Application.DTOs.Planning;

public class ReasoningLogDto
{
    public Guid Id { get; set; }
    public Guid TicketId { get; set; }
    public string? SessionId { get; set; }
    public string DecisionPoint { get; set; } = string.Empty;
    public List<string> OptionsConsidered { get; set; } = new();
    public string ChosenOption { get; set; } = string.Empty;
    public string Rationale { get; set; } = string.Empty;
    public int? ConfidencePercent { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateReasoningLogRequest
{
    public Guid TicketId { get; set; }
    public string? SessionId { get; set; }
    public string DecisionPoint { get; set; } = string.Empty;
    public List<string>? OptionsConsidered { get; set; }
    public string ChosenOption { get; set; } = string.Empty;
    public string Rationale { get; set; } = string.Empty;
    public int? ConfidencePercent { get; set; }
}
