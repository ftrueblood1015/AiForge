using System.ComponentModel;
using System.Text.Json;
using AiForge.Application.DTOs.Plans;
using AiForge.Application.Services;
using ModelContextProtocol.Server;

namespace AiForge.Mcp.Tools;

[McpServerToolType]
public class ImplementationPlanTools
{
    private readonly IImplementationPlanService _planService;
    private readonly ITicketService _ticketService;
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public ImplementationPlanTools(IImplementationPlanService planService, ITicketService ticketService)
    {
        _planService = planService;
        _ticketService = ticketService;
    }

    [McpServerTool(Name = "create_implementation_plan"), Description("Create a new implementation plan for a ticket. The plan starts in Draft status and must be approved before implementation.")]
    public async Task<string> CreateImplementationPlan(
        [Description("Ticket key (e.g., DEMO-1) or ID")] string ticketKeyOrId,
        [Description("Implementation plan content (markdown supported)")] string content,
        [Description("Estimated effort: Small, Medium, Large, XLarge")] string? estimatedEffort = null,
        [Description("Comma-separated list of files that will be affected")] string? affectedFiles = null,
        [Description("Comma-separated list of ticket IDs this plan depends on")] string? dependencyTicketIds = null,
        [Description("Claude session identifier")] string? createdBy = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var ticketId = await ResolveTicketIdAsync(ticketKeyOrId, cancellationToken);
            if (ticketId == null)
                return JsonSerializer.Serialize(new { error = $"Ticket '{ticketKeyOrId}' not found" });

            var request = new CreateImplementationPlanRequest
            {
                Content = content,
                EstimatedEffort = estimatedEffort,
                AffectedFiles = ParseCommaSeparatedList(affectedFiles),
                DependencyTicketIds = ParseCommaSeparatedList(dependencyTicketIds),
                CreatedBy = createdBy
            };

            var plan = await _planService.CreateAsync(ticketId.Value, request, cancellationToken);

            return JsonSerializer.Serialize(new
            {
                success = true,
                plan.Id,
                plan.TicketId,
                plan.Version,
                plan.Status,
                message = $"Implementation plan v{plan.Version} created as Draft"
            }, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool(Name = "update_implementation_plan"), Description("Update a draft implementation plan. Only plans with Draft status can be updated.")]
    public async Task<string> UpdateImplementationPlan(
        [Description("Implementation plan ID (GUID)")] string planId,
        [Description("Updated plan content (markdown supported)")] string? content = null,
        [Description("Updated estimated effort: Small, Medium, Large, XLarge")] string? estimatedEffort = null,
        [Description("Comma-separated list of files that will be affected")] string? affectedFiles = null,
        [Description("Comma-separated list of ticket IDs this plan depends on")] string? dependencyTicketIds = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var id = Guid.Parse(planId);

            var request = new UpdateImplementationPlanRequest
            {
                Content = content,
                EstimatedEffort = estimatedEffort,
                AffectedFiles = affectedFiles != null ? ParseCommaSeparatedList(affectedFiles) : null,
                DependencyTicketIds = dependencyTicketIds != null ? ParseCommaSeparatedList(dependencyTicketIds) : null
            };

            var plan = await _planService.UpdateAsync(id, request, cancellationToken);
            if (plan == null)
                return JsonSerializer.Serialize(new { error = $"Implementation plan '{planId}' not found" });

            return JsonSerializer.Serialize(new
            {
                success = true,
                plan.Id,
                plan.Version,
                plan.Status,
                message = "Implementation plan updated"
            }, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool(Name = "get_implementation_plan"), Description("Get the current implementation plan for a ticket (latest approved or draft)")]
    public async Task<string> GetImplementationPlan(
        [Description("Ticket key (e.g., DEMO-1) or ID")] string ticketKeyOrId,
        [Description("Return compact response (metadata only, no content)")] bool compact = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var ticketId = await ResolveTicketIdAsync(ticketKeyOrId, cancellationToken);
            if (ticketId == null)
                return JsonSerializer.Serialize(new { error = $"Ticket '{ticketKeyOrId}' not found" });

            var plan = await _planService.GetCurrentByTicketIdAsync(ticketId.Value, cancellationToken);
            if (plan == null)
                return JsonSerializer.Serialize(new {
                    found = false,
                    message = $"No implementation plan found for ticket '{ticketKeyOrId}'"
                });

            if (compact)
            {
                return JsonSerializer.Serialize(new
                {
                    found = true,
                    plan.Id,
                    plan.Status,
                    plan.Version,
                    plan.EstimatedEffort,
                    AffectedFileCount = plan.AffectedFiles?.Count ?? 0,
                    plan.CreatedAt,
                    plan.ApprovedAt
                }, JsonOptions);
            }

            return JsonSerializer.Serialize(new
            {
                found = true,
                plan.Id,
                plan.TicketId,
                plan.Content,
                plan.Status,
                plan.Version,
                plan.EstimatedEffort,
                plan.AffectedFiles,
                plan.DependencyTicketIds,
                plan.CreatedBy,
                plan.CreatedAt,
                plan.ApprovedBy,
                plan.ApprovedAt
            }, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool(Name = "list_implementation_plans"), Description("List all implementation plans for a ticket (all versions)")]
    public async Task<string> ListImplementationPlans(
        [Description("Ticket key (e.g., DEMO-1) or ID")] string ticketKeyOrId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var ticketId = await ResolveTicketIdAsync(ticketKeyOrId, cancellationToken);
            if (ticketId == null)
                return JsonSerializer.Serialize(new { error = $"Ticket '{ticketKeyOrId}' not found" });

            var plans = await _planService.GetByTicketIdAsync(ticketId.Value, cancellationToken);

            return JsonSerializer.Serialize(new
            {
                count = plans.Count(),
                plans = plans.Select(p => new
                {
                    p.Id,
                    p.Version,
                    p.Status,
                    p.EstimatedEffort,
                    p.CreatedAt,
                    p.ApprovedAt,
                    p.SupersededAt,
                    p.RejectedAt
                })
            }, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool(Name = "approve_implementation_plan"), Description("Approve a draft implementation plan, marking it ready for implementation")]
    public async Task<string> ApproveImplementationPlan(
        [Description("Implementation plan ID (GUID)")] string planId,
        [Description("Who is approving the plan")] string? approvedBy = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var id = Guid.Parse(planId);

            var request = new ApproveImplementationPlanRequest
            {
                ApprovedBy = approvedBy
            };

            var plan = await _planService.ApproveAsync(id, request, cancellationToken);
            if (plan == null)
                return JsonSerializer.Serialize(new { error = $"Implementation plan '{planId}' not found" });

            return JsonSerializer.Serialize(new
            {
                success = true,
                plan.Id,
                plan.Version,
                plan.Status,
                plan.ApprovedAt,
                message = $"Implementation plan v{plan.Version} approved"
            }, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool(Name = "reject_implementation_plan"), Description("Reject a draft implementation plan")]
    public async Task<string> RejectImplementationPlan(
        [Description("Implementation plan ID (GUID)")] string planId,
        [Description("Who is rejecting the plan")] string? rejectedBy = null,
        [Description("Reason for rejection")] string? rejectionReason = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var id = Guid.Parse(planId);

            var request = new RejectImplementationPlanRequest
            {
                RejectedBy = rejectedBy,
                RejectionReason = rejectionReason
            };

            var plan = await _planService.RejectAsync(id, request, cancellationToken);
            if (plan == null)
                return JsonSerializer.Serialize(new { error = $"Implementation plan '{planId}' not found" });

            return JsonSerializer.Serialize(new
            {
                success = true,
                plan.Id,
                plan.Version,
                plan.Status,
                plan.RejectedAt,
                plan.RejectionReason,
                message = $"Implementation plan v{plan.Version} rejected"
            }, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool(Name = "supersede_implementation_plan"), Description("Create a new version of an approved plan, marking the old one as superseded")]
    public async Task<string> SupersedeImplementationPlan(
        [Description("Implementation plan ID to supersede (GUID)")] string planId,
        [Description("New plan content (markdown supported)")] string content,
        [Description("Estimated effort: Small, Medium, Large, XLarge")] string? estimatedEffort = null,
        [Description("Comma-separated list of files that will be affected")] string? affectedFiles = null,
        [Description("Comma-separated list of ticket IDs this plan depends on")] string? dependencyTicketIds = null,
        [Description("Claude session identifier")] string? createdBy = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var id = Guid.Parse(planId);

            var request = new SupersedeImplementationPlanRequest
            {
                Content = content,
                EstimatedEffort = estimatedEffort,
                AffectedFiles = ParseCommaSeparatedList(affectedFiles),
                DependencyTicketIds = ParseCommaSeparatedList(dependencyTicketIds),
                CreatedBy = createdBy
            };

            var newPlan = await _planService.SupersedeAsync(id, request, cancellationToken);
            if (newPlan == null)
                return JsonSerializer.Serialize(new { error = $"Implementation plan '{planId}' not found" });

            return JsonSerializer.Serialize(new
            {
                success = true,
                newPlan.Id,
                newPlan.Version,
                newPlan.Status,
                supersededPlanId = id,
                message = $"Created new implementation plan v{newPlan.Version}, previous plan superseded"
            }, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    #region Helpers

    private async Task<Guid?> ResolveTicketIdAsync(string ticketKeyOrId, CancellationToken cancellationToken)
    {
        if (Guid.TryParse(ticketKeyOrId, out var id))
            return id;

        var ticket = await _ticketService.GetByKeyAsync(ticketKeyOrId.ToUpperInvariant(), cancellationToken);
        return ticket?.Id;
    }

    private static List<string>? ParseCommaSeparatedList(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
    }

    #endregion
}
