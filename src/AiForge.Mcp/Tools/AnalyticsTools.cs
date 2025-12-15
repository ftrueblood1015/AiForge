using System.ComponentModel;
using System.Text.Json;
using AiForge.Application.DTOs.Analytics.Confidence;
using AiForge.Application.DTOs.Analytics.Dashboard;
using AiForge.Application.DTOs.Analytics.Patterns;
using AiForge.Application.DTOs.Analytics.Sessions;
using AiForge.Application.Services;
using ModelContextProtocol.Server;

namespace AiForge.Mcp.Tools;

[McpServerToolType]
public class AnalyticsTools
{
    private readonly IAnalyticsService _analyticsService;
    private readonly ITicketService _ticketService;
    private readonly IProjectService _projectService;
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public AnalyticsTools(
        IAnalyticsService analyticsService,
        ITicketService ticketService,
        IProjectService projectService)
    {
        _analyticsService = analyticsService;
        _ticketService = ticketService;
        _projectService = projectService;
    }

    #region Session Metrics

    [McpServerTool(Name = "log_session_metrics"), Description("Log session metrics including tokens, duration, and activity counts")]
    public async Task<string> LogSessionMetrics(
        [Description("Ticket key (e.g., DEMO-1) or ID")] string ticketKeyOrId,
        [Description("Claude session identifier")] string sessionId,
        [Description("Session duration in minutes")] int? durationMinutes = null,
        [Description("Input tokens used")] int? inputTokens = null,
        [Description("Output tokens used")] int? outputTokens = null,
        [Description("Total tokens used")] int? totalTokens = null,
        [Description("Number of decisions logged")] int? decisionsLogged = null,
        [Description("Number of progress entries logged")] int? progressEntriesLogged = null,
        [Description("Number of files modified")] int? filesModified = null,
        [Description("Whether a handoff was created")] bool? handoffCreated = null,
        [Description("Additional notes")] string? notes = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var ticketId = await ResolveTicketIdAsync(ticketKeyOrId, cancellationToken);
            if (ticketId == null)
                return JsonSerializer.Serialize(new { error = $"Ticket '{ticketKeyOrId}' not found" });

            var request = new LogSessionMetricsRequest
            {
                TicketId = ticketId.Value,
                SessionId = sessionId,
                SessionStartedAt = DateTime.UtcNow,
                DurationMinutes = durationMinutes,
                InputTokens = inputTokens,
                OutputTokens = outputTokens,
                TotalTokens = totalTokens,
                DecisionsLogged = decisionsLogged,
                ProgressEntriesLogged = progressEntriesLogged,
                FilesModified = filesModified,
                HandoffCreated = handoffCreated,
                Notes = notes
            };

            var metrics = await _analyticsService.LogSessionMetricsAsync(request, cancellationToken);

            return JsonSerializer.Serialize(new
            {
                success = true,
                metrics.Id,
                metrics.TicketId,
                metrics.SessionId,
                metrics.TotalTokens,
                metrics.DurationMinutes,
                message = "Session metrics logged"
            }, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool(Name = "update_session_metrics"), Description("Update existing session metrics (e.g., when session ends)")]
    public async Task<string> UpdateSessionMetrics(
        [Description("Session ID to update")] string sessionId,
        [Description("Session duration in minutes")] int? durationMinutes = null,
        [Description("Input tokens used")] int? inputTokens = null,
        [Description("Output tokens used")] int? outputTokens = null,
        [Description("Total tokens used")] int? totalTokens = null,
        [Description("Number of decisions logged")] int? decisionsLogged = null,
        [Description("Number of progress entries logged")] int? progressEntriesLogged = null,
        [Description("Number of files modified")] int? filesModified = null,
        [Description("Whether a handoff was created")] bool? handoffCreated = null,
        [Description("Additional notes")] string? notes = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new UpdateSessionMetricsRequest
            {
                SessionId = sessionId,
                SessionEndedAt = DateTime.UtcNow,
                DurationMinutes = durationMinutes,
                InputTokens = inputTokens,
                OutputTokens = outputTokens,
                TotalTokens = totalTokens,
                DecisionsLogged = decisionsLogged,
                ProgressEntriesLogged = progressEntriesLogged,
                FilesModified = filesModified,
                HandoffCreated = handoffCreated,
                Notes = notes
            };

            var metrics = await _analyticsService.UpdateSessionMetricsAsync(request, cancellationToken);
            if (metrics == null)
                return JsonSerializer.Serialize(new { error = $"Session metrics with sessionId '{sessionId}' not found" });

            return JsonSerializer.Serialize(new
            {
                success = true,
                metrics.Id,
                metrics.SessionId,
                metrics.TotalTokens,
                metrics.DurationMinutes,
                message = "Session metrics updated"
            }, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    #endregion

    #region Confidence Reports

    [McpServerTool(Name = "get_confidence_report"), Description("Get low confidence decisions that may need human review")]
    public async Task<string> GetConfidenceReport(
        [Description("Project key (e.g., DEMO) to filter by")] string? projectKey = null,
        [Description("Confidence threshold (0-100), decisions below this are returned")] int confidenceThreshold = 50,
        [Description("Only include decisions since this date (ISO format)")] string? since = null,
        [Description("Maximum number of results")] int limit = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Guid? projectId = null;
            if (!string.IsNullOrEmpty(projectKey))
            {
                projectId = await ResolveProjectIdAsync(projectKey, cancellationToken);
                if (projectId == null)
                    return JsonSerializer.Serialize(new { error = $"Project '{projectKey}' not found" });
            }

            var request = new LowConfidenceDecisionRequest
            {
                ProjectId = projectId,
                ConfidenceThreshold = confidenceThreshold,
                Since = string.IsNullOrEmpty(since) ? null : DateTime.Parse(since),
                Limit = limit
            };

            var decisions = await _analyticsService.GetLowConfidenceDecisionsAsync(request, cancellationToken);

            return JsonSerializer.Serialize(new
            {
                count = decisions.Count,
                confidenceThreshold,
                decisions = decisions.Select(d => new
                {
                    d.TicketKey,
                    d.DecisionPoint,
                    d.ChosenOption,
                    d.ConfidencePercent,
                    d.CreatedAt
                })
            }, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool(Name = "get_low_confidence_decisions"), Description("Get decisions with low confidence for review")]
    public async Task<string> GetLowConfidenceDecisions(
        [Description("Project key (e.g., DEMO) to filter by")] string? projectKey = null,
        [Description("Confidence threshold (0-100)")] int confidenceThreshold = 50,
        CancellationToken cancellationToken = default)
    {
        return await GetConfidenceReport(projectKey, confidenceThreshold, null, 50, cancellationToken);
    }

    #endregion

    #region Pattern Insights

    [McpServerTool(Name = "get_file_hotspots"), Description("Get files that are modified most frequently (code hotspots)")]
    public async Task<string> GetFileHotspots(
        [Description("Project key (e.g., DEMO) to filter by")] string? projectKey = null,
        [Description("Number of top files to return")] int topN = 20,
        [Description("Only include changes since this date (ISO format)")] string? since = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Guid? projectId = null;
            if (!string.IsNullOrEmpty(projectKey))
            {
                projectId = await ResolveProjectIdAsync(projectKey, cancellationToken);
                if (projectId == null)
                    return JsonSerializer.Serialize(new { error = $"Project '{projectKey}' not found" });
            }

            var request = new HotFileRequest
            {
                ProjectId = projectId,
                TopN = topN,
                Since = string.IsNullOrEmpty(since) ? null : DateTime.Parse(since)
            };

            var hotFiles = await _analyticsService.GetHotFilesAsync(request, cancellationToken);

            return JsonSerializer.Serialize(new
            {
                count = hotFiles.Count,
                hotFiles = hotFiles.Select(f => new
                {
                    f.FilePath,
                    f.ModificationCount,
                    f.TicketCount,
                    f.TotalLinesAdded,
                    f.TotalLinesRemoved,
                    f.LastModified,
                    f.RecentTicketKeys
                })
            }, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool(Name = "get_recurring_issues"), Description("Identify patterns in bug titles to find recurring issues")]
    public async Task<string> GetRecurringIssues(
        [Description("Project key (e.g., DEMO) to filter by")] string? projectKey = null,
        [Description("Minimum occurrences to be considered recurring")] int minOccurrences = 2,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Guid? projectId = null;
            if (!string.IsNullOrEmpty(projectKey))
            {
                projectId = await ResolveProjectIdAsync(projectKey, cancellationToken);
                if (projectId == null)
                    return JsonSerializer.Serialize(new { error = $"Project '{projectKey}' not found" });
            }

            var request = new RecurringIssueRequest
            {
                ProjectId = projectId,
                MinOccurrences = minOccurrences
            };

            var issues = await _analyticsService.GetRecurringIssuesAsync(request, cancellationToken);

            return JsonSerializer.Serialize(new
            {
                patternCount = issues.Count,
                patterns = issues.Select(i => new
                {
                    i.Pattern,
                    i.OccurrenceCount,
                    relatedTickets = i.RelatedTickets.Take(5).Select(t => t.TicketKey)
                })
            }, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool(Name = "get_pattern_insights"), Description("Get pattern analysis (hot files, correlations, recurring issues, or debt)")]
    public async Task<string> GetPatternInsights(
        [Description("Insight type: hot_files, correlations, recurring, debt")] string insightType,
        [Description("Project key (e.g., DEMO) to filter by")] string? projectKey = null,
        [Description("File path (required for correlations)")] string? filePath = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return insightType.ToLower() switch
            {
                "hot_files" => await GetFileHotspots(projectKey, 20, null, cancellationToken),
                "recurring" => await GetRecurringIssues(projectKey, 2, cancellationToken),
                "debt" => await GetDebtPatternSummary(projectKey, cancellationToken),
                "correlations" when !string.IsNullOrEmpty(filePath) => await GetFileCorrelations(filePath, cancellationToken),
                "correlations" => JsonSerializer.Serialize(new { error = "filePath is required for correlations insight" }),
                _ => JsonSerializer.Serialize(new { error = $"Unknown insight type: {insightType}. Valid types: hot_files, correlations, recurring, debt" })
            };
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    private async Task<string> GetFileCorrelations(string filePath, CancellationToken cancellationToken)
    {
        var request = new FileCorrelationRequest
        {
            FilePath = filePath,
            MinCooccurrence = 2,
            TopN = 10
        };

        var correlations = await _analyticsService.GetCorrelatedFilesAsync(request, cancellationToken);

        return JsonSerializer.Serialize(new
        {
            correlations.FilePath,
            correlatedFileCount = correlations.CorrelatedFiles.Count,
            correlatedFiles = correlations.CorrelatedFiles.Select(f => new
            {
                f.FilePath,
                f.CooccurrenceCount,
                f.CorrelationStrength
            })
        }, JsonOptions);
    }

    private async Task<string> GetDebtPatternSummary(string? projectKey, CancellationToken cancellationToken)
    {
        Guid? projectId = null;
        if (!string.IsNullOrEmpty(projectKey))
        {
            projectId = await ResolveProjectIdAsync(projectKey, cancellationToken);
            if (projectId == null)
                return JsonSerializer.Serialize(new { error = $"Project '{projectKey}' not found" });
        }

        var request = new DebtPatternRequest { ProjectId = projectId };
        var summary = await _analyticsService.GetDebtPatternSummaryAsync(request, cancellationToken);

        return JsonSerializer.Serialize(new
        {
            summary.TotalDebtItems,
            summary.OpenDebtItems,
            summary.ResolvedDebtItems,
            byCategory = summary.ByCategory.Select(c => new { c.Category, c.Count, c.OpenCount }),
            bySeverity = summary.BySeverity.Select(s => new { s.Severity, s.Count, s.OpenCount }),
            topHotspots = summary.TopHotspots.Take(5).Select(h => new { h.FilePath, h.DebtItemCount })
        }, JsonOptions);
    }

    #endregion

    #region Analytics Summary

    [McpServerTool(Name = "get_ticket_analytics"), Description("Get session analytics for a specific ticket")]
    public async Task<string> GetTicketAnalytics(
        [Description("Ticket key (e.g., DEMO-1) or ID")] string ticketKeyOrId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var ticketId = await ResolveTicketIdAsync(ticketKeyOrId, cancellationToken);
            if (ticketId == null)
                return JsonSerializer.Serialize(new { error = $"Ticket '{ticketKeyOrId}' not found" });

            var analytics = await _analyticsService.GetTicketSessionAnalyticsAsync(ticketId.Value, cancellationToken);

            return JsonSerializer.Serialize(new
            {
                analytics.TicketKey,
                analytics.TicketTitle,
                analytics.TotalSessions,
                analytics.TotalDurationMinutes,
                analytics.TotalTokens,
                analytics.TotalDecisions,
                analytics.TotalProgressEntries,
                analytics.TotalFilesModified,
                analytics.HandoffsCreated
            }, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool(Name = "get_productivity_summary"), Description("Get productivity metrics for completed work")]
    public async Task<string> GetProductivitySummary(
        [Description("Project key (e.g., DEMO) to filter by")] string? projectKey = null,
        [Description("Start date (ISO format)")] string? startDate = null,
        [Description("End date (ISO format)")] string? endDate = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Guid? projectId = null;
            if (!string.IsNullOrEmpty(projectKey))
            {
                projectId = await ResolveProjectIdAsync(projectKey, cancellationToken);
                if (projectId == null)
                    return JsonSerializer.Serialize(new { error = $"Project '{projectKey}' not found" });
            }

            var request = new ProductivityMetricsRequest
            {
                ProjectId = projectId,
                StartDate = string.IsNullOrEmpty(startDate) ? null : DateTime.Parse(startDate),
                EndDate = string.IsNullOrEmpty(endDate) ? null : DateTime.Parse(endDate)
            };

            var metrics = await _analyticsService.GetProductivityMetricsAsync(request, cancellationToken);

            return JsonSerializer.Serialize(new
            {
                metrics.TotalTicketsCompleted,
                metrics.TotalSessions,
                metrics.TotalDurationMinutes,
                metrics.TotalTokensUsed,
                averages = new
                {
                    sessionsPerTicket = Math.Round(metrics.AverageSessionsPerTicket, 2),
                    minutesPerTicket = Math.Round(metrics.AverageMinutesPerTicket, 2),
                    tokensPerTicket = Math.Round(metrics.AverageTokensPerTicket, 0),
                    decisionsPerTicket = Math.Round(metrics.AverageDecisionsPerTicket, 2)
                },
                byTicketType = metrics.ByTicketType
            }, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool(Name = "get_analytics_summary"), Description("Get aggregated analytics dashboard data")]
    public async Task<string> GetAnalyticsSummary(
        [Description("Project key (e.g., DEMO) to filter by")] string? projectKey = null,
        [Description("Start date (ISO format)")] string? startDate = null,
        [Description("End date (ISO format)")] string? endDate = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Guid? projectId = null;
            if (!string.IsNullOrEmpty(projectKey))
            {
                projectId = await ResolveProjectIdAsync(projectKey, cancellationToken);
                if (projectId == null)
                    return JsonSerializer.Serialize(new { error = $"Project '{projectKey}' not found" });
            }

            var request = new AnalyticsDashboardRequest
            {
                ProjectId = projectId,
                StartDate = string.IsNullOrEmpty(startDate) ? null : DateTime.Parse(startDate),
                EndDate = string.IsNullOrEmpty(endDate) ? null : DateTime.Parse(endDate),
                RecentActivityLimit = 5,
                TopHotFilesLimit = 5,
                LowConfidenceLimit = 5
            };

            var dashboard = await _analyticsService.GetDashboardAsync(request, cancellationToken);

            return JsonSerializer.Serialize(new
            {
                overview = new
                {
                    dashboard.TotalTickets,
                    dashboard.TicketsInProgress,
                    dashboard.TicketsCompleted,
                    dashboard.TotalSessions,
                    dashboard.TotalDecisions
                },
                confidence = new
                {
                    averageConfidence = Math.Round(dashboard.OverallAverageConfidence, 1),
                    dashboard.LowConfidenceDecisionCount,
                    needsReview = dashboard.RecentLowConfidenceDecisions.Select(d => new
                    {
                        d.TicketKey,
                        d.DecisionPoint,
                        d.ConfidencePercent
                    })
                },
                patterns = new
                {
                    dashboard.OpenTechnicalDebtCount,
                    topHotFiles = dashboard.TopHotFiles.Select(f => new
                    {
                        f.FilePath,
                        f.ModificationCount
                    })
                },
                sessions = new
                {
                    dashboard.TotalTokensUsed,
                    dashboard.TotalMinutesWorked,
                    dashboard.HandoffsCreated
                },
                dashboard.GeneratedAt
            }, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    #endregion

    #region Helpers

    private async Task<Guid?> ResolveTicketIdAsync(string ticketKeyOrId, CancellationToken cancellationToken)
    {
        if (Guid.TryParse(ticketKeyOrId, out var id))
            return id;

        var ticket = await _ticketService.GetByKeyAsync(ticketKeyOrId.ToUpperInvariant(), cancellationToken);
        return ticket?.Id;
    }

    private async Task<Guid?> ResolveProjectIdAsync(string projectKeyOrId, CancellationToken cancellationToken)
    {
        if (Guid.TryParse(projectKeyOrId, out var id))
            return id;

        var project = await _projectService.GetByKeyAsync(projectKeyOrId.ToUpperInvariant(), cancellationToken);
        return project?.Id;
    }

    #endregion
}
