namespace AiForge.Application.DTOs.Analytics.Confidence;

public class LowConfidenceDecisionDto
{
    public Guid ReasoningLogId { get; set; }
    public Guid TicketId { get; set; }
    public string TicketKey { get; set; } = string.Empty;
    public string TicketTitle { get; set; } = string.Empty;
    public string DecisionPoint { get; set; } = string.Empty;
    public string ChosenOption { get; set; } = string.Empty;
    public string Rationale { get; set; } = string.Empty;
    public int ConfidencePercent { get; set; }
    public string? SessionId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class LowConfidenceDecisionRequest
{
    public int ConfidenceThreshold { get; set; } = 50;
    public Guid? ProjectId { get; set; }
    public DateTime? Since { get; set; }
    public int? Limit { get; set; } = 50;
}
