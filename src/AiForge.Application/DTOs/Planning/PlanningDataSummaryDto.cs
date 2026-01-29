namespace AiForge.Application.DTOs.Planning;

/// <summary>
/// Summary response for planning data, optimized for reduced token consumption.
/// Returns summaries and recent items instead of full history.
/// </summary>
public class PlanningDataSummaryDto
{
    public string? ProgressSummary { get; set; }
    public string? DecisionSummary { get; set; }
    public OutcomeStatisticsDto? OutcomeStatistics { get; set; }
    public int TotalProgressEntries { get; set; }
    public int TotalReasoningLogs { get; set; }
    public int TotalSessions { get; set; }
    public ProgressEntryDto? LastProgressEntry { get; set; }
    public ReasoningLogDto? LastDecision { get; set; }
    public List<ProgressEntryDto> RecentProgress { get; set; } = new();
    public List<ReasoningLogDto> RecentDecisions { get; set; } = new();
    public DateTime? SummaryUpdatedAt { get; set; }
    public bool FullHistoryAvailable { get; set; } = true;
}

/// <summary>
/// Breakdown of progress entry outcomes for quick status assessment.
/// </summary>
public class OutcomeStatisticsDto
{
    public int Success { get; set; }
    public int Failure { get; set; }
    public int Partial { get; set; }
    public int Blocked { get; set; }

    public int Total => Success + Failure + Partial + Blocked;
}
