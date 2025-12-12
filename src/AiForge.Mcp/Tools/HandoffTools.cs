using System.ComponentModel;
using System.Text.Json;
using AiForge.Application.DTOs.Handoffs;
using AiForge.Application.DTOs.Planning;
using AiForge.Application.Services;
using AiForge.Domain.Enums;
using ModelContextProtocol.Server;

namespace AiForge.Mcp.Tools;

[McpServerToolType]
public class HandoffTools
{
    private readonly IHandoffService _handoffService;
    private readonly IAiContextService _contextService;
    private readonly ITicketService _ticketService;
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public HandoffTools(
        IHandoffService handoffService,
        IAiContextService contextService,
        ITicketService ticketService)
    {
        _handoffService = handoffService;
        _contextService = contextService;
        _ticketService = ticketService;
    }

    [McpServerTool(Name = "get_context"), Description("Get full context for resuming work on a ticket (ticket details, latest handoff, recent planning data)")]
    public async Task<string> GetContext(
        [Description("Ticket key (e.g., DEMO-1) or ID")] string ticketKeyOrId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var ticketId = await ResolveTicketIdAsync(ticketKeyOrId, cancellationToken);
            if (ticketId == null)
                return JsonSerializer.Serialize(new { error = $"Ticket '{ticketKeyOrId}' not found" });

            var context = await _contextService.GetContextByTicketIdAsync(ticketId.Value, cancellationToken);
            if (context == null)
                return JsonSerializer.Serialize(new { error = $"Context not found for ticket '{ticketKeyOrId}'" });

            return JsonSerializer.Serialize(new
            {
                ticket = new
                {
                    context.Ticket.Id,
                    context.Ticket.Key,
                    context.Ticket.Title,
                    context.Ticket.Description,
                    Status = context.Ticket.Status.ToString(),
                    Type = context.Ticket.Type.ToString(),
                    Priority = context.Ticket.Priority.ToString(),
                    context.Ticket.CurrentHandoffSummary
                },
                latestHandoff = context.LatestHandoff == null ? null : new
                {
                    context.LatestHandoff.Id,
                    context.LatestHandoff.Title,
                    Type = context.LatestHandoff.Type.ToString(),
                    context.LatestHandoff.Summary,
                    context.LatestHandoff.Content,
                    context.LatestHandoff.StructuredContext,
                    context.LatestHandoff.CreatedAt
                },
                recentReasoning = context.RecentReasoning.Select(r => new
                {
                    r.DecisionPoint,
                    r.ChosenOption,
                    r.Rationale,
                    r.ConfidencePercent,
                    r.CreatedAt
                }),
                recentProgress = context.RecentProgress.Select(p => new
                {
                    p.Content,
                    Outcome = p.Outcome.ToString(),
                    p.FilesAffected,
                    p.ErrorDetails,
                    p.CreatedAt
                }),
                activePlanningSession = context.ActivePlanningSession == null ? null : new
                {
                    context.ActivePlanningSession.Id,
                    context.ActivePlanningSession.InitialUnderstanding,
                    context.ActivePlanningSession.Assumptions,
                    context.ActivePlanningSession.AlternativesConsidered,
                    context.ActivePlanningSession.ChosenApproach
                }
            }, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool(Name = "create_handoff"), Description("Create a handoff document summarizing work done (for session end, blockers, or milestones)")]
    public async Task<string> CreateHandoff(
        [Description("Ticket key (e.g., DEMO-1) or ID")] string ticketKeyOrId,
        [Description("Handoff title")] string title,
        [Description("Brief summary for lists")] string summary,
        [Description("Full content (markdown supported)")] string content,
        [Description("Type: SessionEnd, Blocker, Milestone, ContextDump")] string type = "SessionEnd",
        [Description("Comma-separated list of assumptions made")] string? assumptions = null,
        [Description("Comma-separated list of open questions")] string? openQuestions = null,
        [Description("Comma-separated list of blockers")] string? blockers = null,
        [Description("Comma-separated list of files modified")] string? filesModified = null,
        [Description("Comma-separated list of next steps")] string? nextSteps = null,
        [Description("Comma-separated list of warnings")] string? warnings = null,
        [Description("Claude session identifier")] string? sessionId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var ticketId = await ResolveTicketIdAsync(ticketKeyOrId, cancellationToken);
            if (ticketId == null)
                return JsonSerializer.Serialize(new { error = $"Ticket '{ticketKeyOrId}' not found" });

            var structuredContext = new StructuredContextDto
            {
                Assumptions = ParseList(assumptions),
                OpenQuestions = ParseList(openQuestions),
                Blockers = ParseList(blockers),
                FilesModified = ParseList(filesModified),
                NextSteps = ParseList(nextSteps),
                Warnings = ParseList(warnings)
            };

            var request = new CreateHandoffRequest
            {
                TicketId = ticketId.Value,
                SessionId = sessionId,
                Title = title,
                Type = Enum.Parse<HandoffType>(type, true),
                Summary = summary,
                Content = content,
                Context = structuredContext
            };

            var handoff = await _handoffService.CreateAsync(request, cancellationToken);

            return JsonSerializer.Serialize(new
            {
                success = true,
                handoff.Id,
                handoff.Title,
                Type = handoff.Type.ToString(),
                handoff.Summary,
                message = "Handoff document created"
            }, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool(Name = "add_file_snapshot"), Description("Add a file diff/snapshot to a handoff document")]
    public async Task<string> AddFileSnapshot(
        [Description("Handoff ID (GUID)")] string handoffId,
        [Description("File path")] string filePath,
        [Description("File content before changes (optional for new files)")] string? contentBefore = null,
        [Description("File content after changes (optional for deleted files)")] string? contentAfter = null,
        [Description("Programming language for syntax highlighting")] string language = "text",
        CancellationToken cancellationToken = default)
    {
        try
        {
            var id = Guid.Parse(handoffId);
            var request = new CreateFileSnapshotRequest
            {
                FilePath = filePath,
                ContentBefore = contentBefore,
                ContentAfter = contentAfter,
                Language = language
            };

            var snapshot = await _handoffService.AddFileSnapshotAsync(id, request, cancellationToken);

            return JsonSerializer.Serialize(new
            {
                success = true,
                snapshot.Id,
                snapshot.FilePath,
                message = "File snapshot added to handoff"
            }, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool(Name = "get_handoff"), Description("Get a specific handoff document by ID")]
    public async Task<string> GetHandoff(
        [Description("Handoff ID (GUID)")] string handoffId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var id = Guid.Parse(handoffId);
            var handoff = await _handoffService.GetByIdAsync(id, cancellationToken);

            if (handoff == null)
                return JsonSerializer.Serialize(new { error = $"Handoff '{handoffId}' not found" });

            return JsonSerializer.Serialize(new
            {
                handoff.Id,
                handoff.TicketId,
                handoff.Title,
                Type = handoff.Type.ToString(),
                handoff.Summary,
                handoff.Content,
                handoff.StructuredContext,
                handoff.IsActive,
                handoff.CreatedAt,
                handoff.FileSnapshots,
                handoff.VersionCount
            }, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool(Name = "list_handoffs"), Description("List handoffs for a ticket")]
    public async Task<string> ListHandoffs(
        [Description("Ticket key (e.g., DEMO-1) or ID")] string ticketKeyOrId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var ticketId = await ResolveTicketIdAsync(ticketKeyOrId, cancellationToken);
            if (ticketId == null)
                return JsonSerializer.Serialize(new { error = $"Ticket '{ticketKeyOrId}' not found" });

            var handoffs = await _handoffService.GetByTicketIdAsync(ticketId.Value, cancellationToken);

            return JsonSerializer.Serialize(handoffs.Select(h => new
            {
                h.Id,
                h.Title,
                Type = h.Type.ToString(),
                h.Summary,
                h.IsActive,
                h.CreatedAt
            }), JsonOptions);
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

    private static List<string> ParseList(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return new List<string>();

        return input.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
    }
}
