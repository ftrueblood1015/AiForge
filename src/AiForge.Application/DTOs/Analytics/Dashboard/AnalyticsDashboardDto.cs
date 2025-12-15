using AiForge.Application.DTOs.Analytics.Confidence;
using AiForge.Application.DTOs.Analytics.Patterns;
using AiForge.Application.DTOs.Analytics.Sessions;

namespace AiForge.Application.DTOs.Analytics.Dashboard;

public class AnalyticsDashboardDto
{
    // Overview metrics
    public int TotalTickets { get; set; }
    public int TicketsInProgress { get; set; }
    public int TicketsCompleted { get; set; }
    public int TotalSessions { get; set; }
    public int TotalDecisions { get; set; }

    // Confidence summary
    public double OverallAverageConfidence { get; set; }
    public int LowConfidenceDecisionCount { get; set; }
    public List<LowConfidenceDecisionDto> RecentLowConfidenceDecisions { get; set; } = new();

    // Pattern highlights
    public List<AnalyticsHotFileDto> TopHotFiles { get; set; } = new();
    public int OpenTechnicalDebtCount { get; set; }

    // Session summary
    public int TotalTokensUsed { get; set; }
    public int TotalMinutesWorked { get; set; }
    public int HandoffsCreated { get; set; }

    // Recent activity
    public List<RecentActivityDto> RecentActivity { get; set; } = new();

    public DateTime GeneratedAt { get; set; }
}

public class RecentActivityDto
{
    public string ActivityType { get; set; } = string.Empty; // Session, Decision, Handoff, Progress
    public string Description { get; set; } = string.Empty;
    public string? TicketKey { get; set; }
    public DateTime Timestamp { get; set; }
}

public class AnalyticsDashboardRequest
{
    public Guid? ProjectId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int RecentActivityLimit { get; set; } = 10;
    public int TopHotFilesLimit { get; set; } = 5;
    public int LowConfidenceLimit { get; set; } = 5;
}
