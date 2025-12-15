using System.ComponentModel;
using System.Text.Json;
using AiForge.Application.DTOs.Estimation;
using AiForge.Application.Services;
using ModelContextProtocol.Server;

namespace AiForge.Mcp.Tools;

[McpServerToolType]
public class EffortEstimationTools
{
    private readonly IEffortEstimationService _estimationService;
    private readonly ITicketService _ticketService;
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public EffortEstimationTools(IEffortEstimationService estimationService, ITicketService ticketService)
    {
        _estimationService = estimationService;
        _ticketService = ticketService;
    }

    [McpServerTool(Name = "estimate_ticket"), Description("Create or update effort estimation for a ticket")]
    public async Task<string> EstimateTicket(
        [Description("Ticket key (e.g., DEMO-1) or ID")] string ticketKeyOrId,
        [Description("Complexity level: Low, Medium, High, VeryHigh")] string complexity,
        [Description("Estimated effort: XSmall, Small, Medium, Large, XLarge")] string estimatedEffort,
        [Description("Confidence level 0-100")] int confidence,
        [Description("Reasoning behind the estimate")] string reasoning,
        [Description("Comma-separated list of assumptions")] string? assumptions = null,
        [Description("Reason for revising (required if estimation already exists)")] string? revisionReason = null,
        [Description("Claude session identifier")] string? sessionId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var ticketId = await ResolveTicketIdAsync(ticketKeyOrId, cancellationToken);
            if (ticketId == null)
                return JsonSerializer.Serialize(new { error = $"Ticket '{ticketKeyOrId}' not found" });

            // Check if estimation exists
            var existing = await _estimationService.GetLatestEstimationAsync(ticketId.Value, cancellationToken);

            EffortEstimationDto estimation;

            if (existing == null)
            {
                // Create new estimation
                var createRequest = new CreateEstimationRequest
                {
                    Complexity = complexity,
                    EstimatedEffort = estimatedEffort,
                    ConfidencePercent = confidence,
                    EstimationReasoning = reasoning,
                    Assumptions = assumptions,
                    SessionId = sessionId
                };

                estimation = await _estimationService.CreateEstimationAsync(ticketId.Value, createRequest, cancellationToken);

                return JsonSerializer.Serialize(new
                {
                    success = true,
                    estimation.Id,
                    estimation.TicketId,
                    estimation.Complexity,
                    estimation.EstimatedEffort,
                    estimation.ConfidencePercent,
                    estimation.Version,
                    message = "Initial estimation created"
                }, JsonOptions);
            }
            else
            {
                // Revise existing estimation
                if (string.IsNullOrWhiteSpace(revisionReason))
                    return JsonSerializer.Serialize(new { error = "revisionReason is required when revising an existing estimation" });

                var reviseRequest = new ReviseEstimationRequest
                {
                    Complexity = complexity,
                    EstimatedEffort = estimatedEffort,
                    ConfidencePercent = confidence,
                    EstimationReasoning = reasoning,
                    Assumptions = assumptions,
                    RevisionReason = revisionReason,
                    SessionId = sessionId
                };

                estimation = await _estimationService.ReviseEstimationAsync(ticketId.Value, reviseRequest, cancellationToken);

                return JsonSerializer.Serialize(new
                {
                    success = true,
                    estimation.Id,
                    estimation.TicketId,
                    estimation.Complexity,
                    estimation.EstimatedEffort,
                    estimation.ConfidencePercent,
                    estimation.Version,
                    estimation.RevisionReason,
                    message = $"Estimation revised (v{estimation.Version})"
                }, JsonOptions);
            }
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool(Name = "record_actual_effort"), Description("Record actual effort when ticket is completed")]
    public async Task<string> RecordActualEffort(
        [Description("Ticket key (e.g., DEMO-1) or ID")] string ticketKeyOrId,
        [Description("Actual effort: XSmall, Small, Medium, Large, XLarge")] string actualEffort,
        [Description("Notes explaining any variance from estimate")] string? varianceNotes = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var ticketId = await ResolveTicketIdAsync(ticketKeyOrId, cancellationToken);
            if (ticketId == null)
                return JsonSerializer.Serialize(new { error = $"Ticket '{ticketKeyOrId}' not found" });

            var request = new RecordActualEffortRequest
            {
                ActualEffort = actualEffort,
                VarianceNotes = varianceNotes
            };

            var estimation = await _estimationService.RecordActualEffortAsync(ticketId.Value, request, cancellationToken);
            if (estimation == null)
                return JsonSerializer.Serialize(new { error = $"No estimation found for ticket '{ticketKeyOrId}'. Create an estimation first." });

            // Calculate variance
            var estimatedIndex = GetEffortIndex(estimation.EstimatedEffort);
            var actualIndex = GetEffortIndex(estimation.ActualEffort!);
            var variance = actualIndex - estimatedIndex;
            var varianceText = variance switch
            {
                0 => "On target",
                > 0 => $"Underestimated by {variance} size(s)",
                < 0 => $"Overestimated by {Math.Abs(variance)} size(s)"
            };

            return JsonSerializer.Serialize(new
            {
                success = true,
                estimation.Id,
                estimation.TicketId,
                estimation.EstimatedEffort,
                estimation.ActualEffort,
                variance = varianceText,
                estimation.VarianceNotes,
                message = "Actual effort recorded"
            }, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool(Name = "get_estimation_data"), Description("Get estimation data and history for a ticket")]
    public async Task<string> GetEstimationData(
        [Description("Ticket key (e.g., DEMO-1) or ID")] string ticketKeyOrId,
        [Description("Include full estimation history")] bool includeHistory = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var ticketId = await ResolveTicketIdAsync(ticketKeyOrId, cancellationToken);
            if (ticketId == null)
                return JsonSerializer.Serialize(new { error = $"Ticket '{ticketKeyOrId}' not found" });

            if (includeHistory)
            {
                var history = await _estimationService.GetEstimationHistoryAsync(ticketId.Value, cancellationToken);

                return JsonSerializer.Serialize(new
                {
                    found = history.TotalVersions > 0,
                    history.TicketId,
                    history.TotalVersions,
                    estimations = history.Estimations.Select(e => new
                    {
                        e.Id,
                        e.Complexity,
                        e.EstimatedEffort,
                        e.ConfidencePercent,
                        e.EstimationReasoning,
                        e.Assumptions,
                        e.ActualEffort,
                        e.VarianceNotes,
                        e.Version,
                        e.RevisionReason,
                        e.IsLatest,
                        e.CreatedAt
                    })
                }, JsonOptions);
            }
            else
            {
                var estimation = await _estimationService.GetLatestEstimationAsync(ticketId.Value, cancellationToken);

                if (estimation == null)
                    return JsonSerializer.Serialize(new { found = false, message = $"No estimation found for ticket '{ticketKeyOrId}'" });

                // Calculate variance if actual is recorded
                string? varianceText = null;
                if (estimation.ActualEffort != null)
                {
                    var estimatedIndex = GetEffortIndex(estimation.EstimatedEffort);
                    var actualIndex = GetEffortIndex(estimation.ActualEffort);
                    var variance = actualIndex - estimatedIndex;
                    varianceText = variance switch
                    {
                        0 => "On target",
                        > 0 => $"Underestimated by {variance} size(s)",
                        < 0 => $"Overestimated by {Math.Abs(variance)} size(s)"
                    };
                }

                return JsonSerializer.Serialize(new
                {
                    found = true,
                    estimation.Id,
                    estimation.TicketId,
                    estimation.Complexity,
                    estimation.EstimatedEffort,
                    estimation.ConfidencePercent,
                    estimation.EstimationReasoning,
                    estimation.Assumptions,
                    estimation.ActualEffort,
                    estimation.VarianceNotes,
                    variance = varianceText,
                    estimation.Version,
                    estimation.RevisionReason,
                    estimation.CreatedAt
                }, JsonOptions);
            }
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

    private static int GetEffortIndex(string effort)
    {
        return effort.ToLower() switch
        {
            "xsmall" => 0,
            "small" => 1,
            "medium" => 2,
            "large" => 3,
            "xlarge" => 4,
            _ => -1
        };
    }
}
