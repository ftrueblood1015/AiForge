using System.ComponentModel;
using System.Text.Json;
using AiForge.Application.DTOs.Planning;
using AiForge.Application.Services;
using AiForge.Domain.Enums;
using ModelContextProtocol.Server;

namespace AiForge.Mcp.Tools;

[McpServerToolType]
public class PlanningTools
{
    private readonly IPlanningService _planningService;
    private readonly ITicketService _ticketService;
    private readonly ISummaryService _summaryService;
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public PlanningTools(IPlanningService planningService, ITicketService ticketService, ISummaryService summaryService)
    {
        _planningService = planningService;
        _ticketService = ticketService;
        _summaryService = summaryService;
    }

    [McpServerTool(Name = "start_planning"), Description("Start a planning session for a ticket to document initial understanding and assumptions")]
    public async Task<string> StartPlanning(
        [Description("Ticket key (e.g., DEMO-1) or ID")] string ticketKeyOrId,
        [Description("Your initial understanding of what needs to be done")] string initialUnderstanding,
        [Description("Comma-separated list of assumptions")] string? assumptions = null,
        [Description("Claude session identifier")] string? sessionId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var ticketId = await ResolveTicketIdAsync(ticketKeyOrId, cancellationToken);
            if (ticketId == null)
                return JsonSerializer.Serialize(new { error = $"Ticket '{ticketKeyOrId}' not found" });

            var assumptionsList = string.IsNullOrEmpty(assumptions)
                ? null
                : assumptions.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

            var request = new CreatePlanningSessionRequest
            {
                TicketId = ticketId.Value,
                SessionId = sessionId,
                InitialUnderstanding = initialUnderstanding,
                Assumptions = assumptionsList
            };

            var session = await _planningService.CreateSessionAsync(request, cancellationToken);

            return JsonSerializer.Serialize(new
            {
                success = true,
                session.Id,
                session.TicketId,
                session.InitialUnderstanding,
                session.Assumptions,
                message = "Planning session started"
            }, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool(Name = "log_decision"), Description("Log a decision with rationale for transparency (show your work)")]
    public async Task<string> LogDecision(
        [Description("Ticket key (e.g., DEMO-1) or ID")] string ticketKeyOrId,
        [Description("What decision was made")] string decisionPoint,
        [Description("The option that was chosen")] string chosenOption,
        [Description("Why this option was chosen")] string rationale,
        [Description("Comma-separated list of other options considered")] string? optionsConsidered = null,
        [Description("Confidence level 0-100")] int? confidence = null,
        [Description("Claude session identifier")] string? sessionId = null,
        [Description("Return full entity response instead of minimal confirmation")] bool returnFull = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var (ticketId, ticketKey) = await ResolveTicketAsync(ticketKeyOrId, cancellationToken);
            if (ticketId == null)
                return JsonSerializer.Serialize(new { error = $"Ticket '{ticketKeyOrId}' not found" });

            var optionsList = string.IsNullOrEmpty(optionsConsidered)
                ? null
                : optionsConsidered.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

            var request = new CreateReasoningLogRequest
            {
                TicketId = ticketId.Value,
                SessionId = sessionId,
                DecisionPoint = decisionPoint,
                OptionsConsidered = optionsList,
                ChosenOption = chosenOption,
                Rationale = rationale,
                ConfidencePercent = confidence
            };

            var log = await _planningService.CreateReasoningLogAsync(request, cancellationToken);

            if (returnFull)
            {
                return JsonSerializer.Serialize(new
                {
                    success = true,
                    entity = log,
                    message = "Decision logged"
                }, JsonOptions);
            }

            return JsonSerializer.Serialize(new
            {
                success = true,
                Id = log.Id,
                TicketKey = ticketKey,
                message = "Decision logged"
            }, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool(Name = "log_progress"), Description("Log progress on a task (what was done/attempted)")]
    public async Task<string> LogProgress(
        [Description("Ticket key (e.g., DEMO-1) or ID")] string ticketKeyOrId,
        [Description("What was done or attempted")] string content,
        [Description("Outcome: Success, Failure, Partial, Blocked")] string outcome = "Success",
        [Description("Comma-separated list of files affected")] string? filesAffected = null,
        [Description("Error details if outcome is Failure or Blocked")] string? errorDetails = null,
        [Description("Claude session identifier")] string? sessionId = null,
        [Description("Return full entity response instead of minimal confirmation")] bool returnFull = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var (ticketId, ticketKey) = await ResolveTicketAsync(ticketKeyOrId, cancellationToken);
            if (ticketId == null)
                return JsonSerializer.Serialize(new { error = $"Ticket '{ticketKeyOrId}' not found" });

            var filesList = string.IsNullOrEmpty(filesAffected)
                ? null
                : filesAffected.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

            var request = new CreateProgressEntryRequest
            {
                TicketId = ticketId.Value,
                SessionId = sessionId,
                Content = content,
                Outcome = Enum.Parse<ProgressOutcome>(outcome, true),
                FilesAffected = filesList,
                ErrorDetails = errorDetails
            };

            var entry = await _planningService.CreateProgressEntryAsync(request, cancellationToken);

            if (returnFull)
            {
                return JsonSerializer.Serialize(new
                {
                    success = true,
                    entity = entry,
                    message = "Progress logged"
                }, JsonOptions);
            }

            return JsonSerializer.Serialize(new
            {
                success = true,
                Id = entry.Id,
                TicketKey = ticketKey,
                message = "Progress logged"
            }, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool(Name = "complete_planning"), Description("Mark a planning session as complete")]
    public async Task<string> CompletePlanning(
        [Description("Planning session ID (GUID)")] string sessionId,
        [Description("The approach that was chosen")] string? chosenApproach = null,
        [Description("Rationale for the chosen approach")] string? rationale = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var id = Guid.Parse(sessionId);
            var request = new CompletePlanningSessionRequest
            {
                ChosenApproach = chosenApproach,
                Rationale = rationale
            };

            var session = await _planningService.CompleteSessionAsync(id, request, cancellationToken);
            if (session == null)
                return JsonSerializer.Serialize(new { error = $"Planning session '{sessionId}' not found" });

            return JsonSerializer.Serialize(new
            {
                success = true,
                session.Id,
                session.IsCompleted,
                session.CompletedAt,
                message = "Planning session completed"
            }, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool(Name = "get_planning_data"), Description("Get all planning data for a ticket (sessions, decisions, progress)")]
    public async Task<string> GetPlanningData(
        [Description("Ticket key (e.g., DEMO-1) or ID")] string ticketKeyOrId,
        [Description("Return compact response (counts and recent items only)")] bool compact = false,
        [Description("Return full history of all entries (default: false returns summary + recent items)")] bool fullHistory = false,
        [Description("Number of recent items to include in summary response (default: 5)")] int recentCount = 5,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var ticketId = await ResolveTicketIdAsync(ticketKeyOrId, cancellationToken);
            if (ticketId == null)
                return JsonSerializer.Serialize(new { error = $"Ticket '{ticketKeyOrId}' not found" });

            // Full history mode - return all entries (original behavior)
            if (fullHistory)
            {
                var data = await _planningService.GetPlanningDataByTicketIdAsync(ticketId.Value, cancellationToken);
                return JsonSerializer.Serialize(new
                {
                    sessions = data.Sessions.Select(s => new
                    {
                        s.Id,
                        s.InitialUnderstanding,
                        s.Assumptions,
                        s.ChosenApproach,
                        s.Rationale,
                        s.IsCompleted,
                        s.CreatedAt,
                        s.CompletedAt
                    }),
                    reasoningLogs = data.ReasoningLogs.Select(r => new
                    {
                        r.Id,
                        r.DecisionPoint,
                        r.OptionsConsidered,
                        r.ChosenOption,
                        r.Rationale,
                        r.ConfidencePercent,
                        r.CreatedAt
                    }),
                    progressEntries = data.ProgressEntries.Select(p => new
                    {
                        p.Id,
                        p.Content,
                        Outcome = p.Outcome.ToString(),
                        p.FilesAffected,
                        p.ErrorDetails,
                        p.CreatedAt
                    })
                }, JsonOptions);
            }

            // Compact mode - counts and minimal recent items (backwards compatibility)
            if (compact)
            {
                var data = await _planningService.GetPlanningDataByTicketIdAsync(ticketId.Value, cancellationToken);
                var latestSession = data.Sessions.OrderByDescending(s => s.CreatedAt).FirstOrDefault();
                var recentDecisions = data.ReasoningLogs.OrderByDescending(r => r.CreatedAt).Take(3);
                var recentProgress = data.ProgressEntries.OrderByDescending(p => p.CreatedAt).Take(3);

                return JsonSerializer.Serialize(new
                {
                    sessionCount = data.Sessions.Count(),
                    latestSession = latestSession == null ? null : new
                    {
                        latestSession.Id,
                        latestSession.IsCompleted,
                        latestSession.CreatedAt
                    },
                    reasoningLogCount = data.ReasoningLogs.Count(),
                    recentDecisions = recentDecisions.Select(r => new
                    {
                        r.DecisionPoint,
                        r.ChosenOption,
                        r.CreatedAt
                    }),
                    progressEntryCount = data.ProgressEntries.Count(),
                    recentProgress = recentProgress.Select(p => new
                    {
                        Content = string.IsNullOrEmpty(p.Content) ? p.Content
                            : (p.Content.Length > 100 ? p.Content.Substring(0, 100) + "..." : p.Content),
                        Outcome = p.Outcome.ToString(),
                        p.CreatedAt
                    })
                }, JsonOptions);
            }

            // Default: Summary mode - summaries + recent items (token-optimized)
            var summary = await _summaryService.GetPlanningDataSummaryAsync(ticketId.Value, recentCount, cancellationToken);
            return JsonSerializer.Serialize(new
            {
                summary = new
                {
                    summary.ProgressSummary,
                    summary.DecisionSummary,
                    summary.OutcomeStatistics,
                    summary.TotalProgressEntries,
                    summary.TotalReasoningLogs,
                    summary.TotalSessions,
                    summary.SummaryUpdatedAt
                },
                lastEntry = summary.LastProgressEntry == null ? null : new
                {
                    summary.LastProgressEntry.Id,
                    summary.LastProgressEntry.Content,
                    Outcome = summary.LastProgressEntry.Outcome.ToString(),
                    summary.LastProgressEntry.CreatedAt
                },
                recentProgress = summary.RecentProgress.Select(p => new
                {
                    p.Id,
                    p.Content,
                    Outcome = p.Outcome.ToString(),
                    p.FilesAffected,
                    p.CreatedAt
                }),
                recentDecisions = summary.RecentDecisions.Select(r => new
                {
                    r.Id,
                    r.DecisionPoint,
                    r.ChosenOption,
                    r.ConfidencePercent,
                    r.CreatedAt
                }),
                fullHistoryAvailable = summary.FullHistoryAvailable
            }, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    private async Task<Guid?> ResolveTicketIdAsync(string ticketKeyOrId, CancellationToken cancellationToken)
    {
        if (Guid.TryParse(ticketKeyOrId, out var id))
            return id;

        var ticket = await _ticketService.GetByKeyAsync(ticketKeyOrId.ToUpperInvariant(), cancellationToken);
        return ticket?.Id;
    }

    private async Task<(Guid? Id, string? Key)> ResolveTicketAsync(string ticketKeyOrId, CancellationToken cancellationToken)
    {
        if (Guid.TryParse(ticketKeyOrId, out var id))
        {
            var ticket = await _ticketService.GetByIdAsync(id, cancellationToken);
            return (ticket?.Id, ticket?.Key);
        }

        var ticketByKey = await _ticketService.GetByKeyAsync(ticketKeyOrId.ToUpperInvariant(), cancellationToken);
        return (ticketByKey?.Id, ticketByKey?.Key);
    }
}
