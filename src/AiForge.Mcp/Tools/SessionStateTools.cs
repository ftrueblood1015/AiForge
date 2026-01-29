using System.ComponentModel;
using System.Text.Json;
using AiForge.Application.DTOs.SessionState;
using AiForge.Application.Services;
using ModelContextProtocol.Server;

namespace AiForge.Mcp.Tools;

[McpServerToolType]
public class SessionStateTools
{
    private readonly ISessionStateService _sessionStateService;
    private readonly ITicketService _ticketService;
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public SessionStateTools(ISessionStateService sessionStateService, ITicketService ticketService)
    {
        _sessionStateService = sessionStateService;
        _ticketService = ticketService;
    }

    [McpServerTool(Name = "save_session_state"), Description("Save Claude's current working state for later resumption")]
    public async Task<string> SaveSessionState(
        [Description("Claude session identifier")] string sessionId,
        [Description("Current phase: Researching, Planning, Implementing, Reviewing, Testing, Finalizing")] string currentPhase,
        [Description("Condensed summary of current understanding (max 4000 chars)")] string? workingSummary = null,
        [Description("Ticket key or ID for context (optional)")] string? ticketKeyOrId = null,
        [Description("Queue ID for context (optional)")] string? queueId = null,
        [Description("JSON checkpoint data (optional)")] string? checkpoint = null,
        [Description("Hours until expiry (default 24)")] int expiresInHours = 24,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Guid? ticketId = null;
            if (!string.IsNullOrEmpty(ticketKeyOrId))
            {
                ticketId = await ResolveTicketIdAsync(ticketKeyOrId, cancellationToken);
            }

            Guid? parsedQueueId = null;
            if (!string.IsNullOrEmpty(queueId) && Guid.TryParse(queueId, out var qid))
            {
                parsedQueueId = qid;
            }

            Dictionary<string, object>? checkpointData = null;
            if (!string.IsNullOrEmpty(checkpoint))
            {
                checkpointData = JsonSerializer.Deserialize<Dictionary<string, object>>(checkpoint);
            }

            var request = new SaveSessionStateRequest
            {
                SessionId = sessionId,
                TicketId = ticketId,
                QueueId = parsedQueueId,
                CurrentPhase = currentPhase,
                WorkingSummary = workingSummary,
                Checkpoint = checkpointData,
                ExpiresInHours = expiresInHours
            };

            var result = await _sessionStateService.SaveAsync(request, cancellationToken);

            return JsonSerializer.Serialize(new
            {
                success = true,
                result.Id,
                result.SessionId,
                result.TicketId,
                result.TicketKey,
                result.CurrentPhase,
                result.ExpiresAt,
                message = "Session state saved"
            }, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool(Name = "load_session_state"), Description("Load previously saved session state to resume work")]
    public async Task<string> LoadSessionState(
        [Description("Claude session identifier")] string sessionId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _sessionStateService.LoadAsync(sessionId, cancellationToken);

            if (result == null)
            {
                return JsonSerializer.Serialize(new
                {
                    found = false,
                    message = $"No session state found for sessionId '{sessionId}'"
                }, JsonOptions);
            }

            return JsonSerializer.Serialize(new
            {
                found = true,
                result.Id,
                result.SessionId,
                result.TicketId,
                result.TicketKey,
                result.QueueId,
                result.QueueName,
                result.CurrentPhase,
                result.WorkingSummary,
                result.Checkpoint,
                result.CreatedAt,
                result.UpdatedAt,
                result.ExpiresAt,
                result.IsExpired
            }, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool(Name = "clear_session_state"), Description("Clear session state when work is complete")]
    public async Task<string> ClearSessionState(
        [Description("Claude session identifier")] string sessionId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var deleted = await _sessionStateService.ClearAsync(sessionId, cancellationToken);

            return JsonSerializer.Serialize(new
            {
                success = deleted,
                message = deleted
                    ? $"Session state for '{sessionId}' cleared"
                    : $"No session state found for '{sessionId}'"
            }, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool(Name = "cleanup_expired_sessions"), Description("Remove all expired session states (admin tool)")]
    public async Task<string> CleanupExpiredSessions(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var count = await _sessionStateService.CleanupExpiredAsync(cancellationToken);

            return JsonSerializer.Serialize(new
            {
                success = true,
                deletedCount = count,
                message = count > 0
                    ? $"Cleaned up {count} expired session(s)"
                    : "No expired sessions to clean up"
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
}
