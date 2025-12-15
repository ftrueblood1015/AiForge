namespace AiForge.Application.DTOs.Analytics.Confidence;

public class TicketConfidenceSummaryDto
{
    public Guid TicketId { get; set; }
    public string TicketKey { get; set; } = string.Empty;
    public string TicketTitle { get; set; } = string.Empty;
    public string TicketStatus { get; set; } = string.Empty;
    public double AverageConfidence { get; set; }
    public int TotalDecisions { get; set; }
    public int LowConfidenceDecisions { get; set; }
    public int? LowestConfidence { get; set; }
    public DateTime? LastDecisionAt { get; set; }
}

public class TicketConfidenceSummaryRequest
{
    public Guid? ProjectId { get; set; }
    public int ConfidenceThreshold { get; set; } = 50;
    public int? Limit { get; set; } = 20;
}
