namespace AiForge.Application.DTOs.Analytics.Confidence;

public class ConfidenceTrendDto
{
    public List<ConfidenceTrendPoint> DataPoints { get; set; } = new();
    public double OverallAverageConfidence { get; set; }
    public int TotalDecisions { get; set; }
    public int LowConfidenceCount { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class ConfidenceTrendPoint
{
    public DateTime Date { get; set; }
    public double AverageConfidence { get; set; }
    public int DecisionCount { get; set; }
    public int LowConfidenceCount { get; set; }
}

public class ConfidenceTrendRequest
{
    public Guid? ProjectId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Granularity { get; set; } = "day"; // day, week, month
}
