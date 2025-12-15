namespace AiForge.Application.DTOs.Analytics.Sessions;

public class ProjectSessionAnalyticsDto
{
    public Guid ProjectId { get; set; }
    public string ProjectKey { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;

    public int TotalSessions { get; set; }
    public int TotalDurationMinutes { get; set; }
    public int? AverageDurationMinutes { get; set; }

    public int TotalInputTokens { get; set; }
    public int TotalOutputTokens { get; set; }
    public int TotalTokens { get; set; }

    public int TotalDecisions { get; set; }
    public int TotalProgressEntries { get; set; }
    public int TotalFilesModified { get; set; }
    public int HandoffsCreated { get; set; }

    public int TicketsWorkedOn { get; set; }

    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class ProjectSessionAnalyticsRequest
{
    public Guid ProjectId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
