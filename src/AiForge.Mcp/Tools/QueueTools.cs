using System.ComponentModel;
using System.Text.Json;
using AiForge.Application.DTOs.WorkQueues;
using AiForge.Application.Services;
using AiForge.Domain.Enums;
using ModelContextProtocol.Server;

namespace AiForge.Mcp.Tools;

[McpServerToolType]
public class QueueTools
{
    private readonly IWorkQueueService _queueService;
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private readonly IProjectService _projectService;
    private readonly ITicketService _ticketService;

    public QueueTools(IWorkQueueService queueService, IProjectService projectService, ITicketService ticketService)
    {
        _queueService = queueService;
        _projectService = projectService;
        _ticketService = ticketService;
    }

    [McpServerTool(Name = "aiforge_create_queue"), Description("Create a new work queue for a project. Use this to set up focused work sessions with tickets and implementation plans.")]
    public async Task<string> CreateQueue(
        [Description("Project key (e.g., AIFORGE) or project ID")] string projectKeyOrId,
        [Description("Queue name (e.g., 'AIFORGE-19 Implementation')")] string name,
        [Description("Queue description (optional)")] string? description = null,
        [Description("Implementation plan ID to link (optional, GUID)")] string? implementationPlanId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Resolve project
            var project = await ResolveProjectAsync(projectKeyOrId, cancellationToken);
            if (project == null)
                return JsonSerializer.Serialize(new { error = $"Project '{projectKeyOrId}' not found" });

            Guid? planId = null;
            if (!string.IsNullOrEmpty(implementationPlanId) && Guid.TryParse(implementationPlanId, out var parsedPlanId))
                planId = parsedPlanId;

            var request = new CreateWorkQueueRequest
            {
                Name = name,
                Description = description,
                ImplementationPlanId = planId
            };

            var queue = await _queueService.CreateAsync(project.Id, request, "mcp-client", cancellationToken);

            return JsonSerializer.Serialize(new
            {
                success = true,
                queueId = queue.Id,
                queueName = queue.Name,
                projectId = queue.ProjectId,
                projectName = queue.ProjectName,
                implementationPlanId = queue.ImplementationPlanId,
                message = $"Queue '{queue.Name}' created successfully"
            }, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool(Name = "aiforge_add_queue_item"), Description("Add a ticket to a work queue. Items are processed in position order.")]
    public async Task<string> AddQueueItem(
        [Description("Queue ID (GUID)")] string queueId,
        [Description("Ticket key (e.g., DEMO-1) or ticket ID to add")] string ticketKeyOrId,
        [Description("Position in queue (optional, defaults to end)")] int? position = null,
        [Description("Notes about this item (optional)")] string? notes = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!Guid.TryParse(queueId, out var qId))
                return JsonSerializer.Serialize(new { error = "Invalid queue ID format" });

            // Resolve ticket
            var ticket = await ResolveTicketAsync(ticketKeyOrId, cancellationToken);
            if (ticket == null)
                return JsonSerializer.Serialize(new { error = $"Ticket '{ticketKeyOrId}' not found" });

            var request = new AddQueueItemRequest
            {
                WorkItemId = ticket.Id,
                WorkItemType = WorkItemType.Task,
                Position = position,
                Notes = notes ?? $"{ticket.Key}: {ticket.Title}"
            };

            var item = await _queueService.AddItemAsync(qId, request, "mcp-client", cancellationToken);
            if (item == null)
                return JsonSerializer.Serialize(new { error = "Queue not found" });

            return JsonSerializer.Serialize(new
            {
                success = true,
                itemId = item.Id,
                ticketKey = ticket.Key,
                ticketTitle = ticket.Title,
                position = item.Position,
                message = $"Ticket {ticket.Key} added to queue at position {item.Position}"
            }, JsonOptions);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
        {
            return JsonSerializer.Serialize(new { error = "Ticket already exists in queue" });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool(Name = "aiforge_list_queues"), Description("List work queues for a project.")]
    public async Task<string> ListQueues(
        [Description("Project key (e.g., AIFORGE) or project ID")] string projectKeyOrId,
        [Description("Filter by status: Active, Paused, Completed, Archived (optional)")] string? status = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var project = await ResolveProjectAsync(projectKeyOrId, cancellationToken);
            if (project == null)
                return JsonSerializer.Serialize(new { error = $"Project '{projectKeyOrId}' not found" });

            WorkQueueStatus? statusFilter = null;
            if (!string.IsNullOrEmpty(status) && Enum.TryParse<WorkQueueStatus>(status, true, out var parsed))
                statusFilter = parsed;

            var queues = await _queueService.GetByProjectAsync(project.Id, statusFilter, cancellationToken);

            return JsonSerializer.Serialize(new
            {
                projectKey = project.Key,
                projectName = project.Name,
                queueCount = queues.Count,
                queues = queues.Select(q => new
                {
                    q.Id,
                    q.Name,
                    q.Description,
                    status = q.Status.ToString(),
                    q.ItemCount,
                    q.CheckedOutBy,
                    q.ImplementationPlanId,
                    q.ImplementationPlanTitle,
                    q.CreatedAt
                })
            }, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool(Name = "aiforge_checkout_queue"), Description("Checkout a work queue for focused work. Returns Tier 1 context with current focus, decisions, and next steps. Use this at the start of a work session.")]
    public async Task<string> CheckoutQueue(
        [Description("Queue ID (GUID)")] string queueId,
        [Description("Checkout duration in minutes (optional, default: no expiry)")] int? durationMinutes = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!Guid.TryParse(queueId, out var id))
                return JsonSerializer.Serialize(new { error = "Invalid queue ID format" });

            var request = new CheckoutRequest { DurationMinutes = durationMinutes };
            var (queue, conflict) = await _queueService.CheckoutAsync(id, request, "mcp-client", cancellationToken);

            if (conflict != null)
            {
                return JsonSerializer.Serialize(new
                {
                    error = "Queue is already checked out",
                    checkedOutBy = conflict.CheckedOutBy,
                    expiresAt = conflict.ExpiresAt?.ToString("O")
                }, JsonOptions);
            }

            return JsonSerializer.Serialize(new
            {
                success = true,
                queueId = queue!.Id,
                queueName = queue.Name,
                checkedOutAt = queue.CheckedOutAt?.ToString("O"),
                context = new
                {
                    queue.Context.CurrentFocus,
                    queue.Context.KeyDecisions,
                    queue.Context.BlockersResolved,
                    queue.Context.NextSteps,
                    queue.Context.LastUpdated
                },
                itemCount = queue.ItemCount,
                message = "Queue checked out. Review CurrentFocus and NextSteps to resume work."
            }, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool(Name = "aiforge_release_queue"), Description("Release checkout on a work queue. Call this when done working or pausing.")]
    public async Task<string> ReleaseQueue(
        [Description("Queue ID (GUID)")] string queueId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!Guid.TryParse(queueId, out var id))
                return JsonSerializer.Serialize(new { error = "Invalid queue ID format" });

            var released = await _queueService.ReleaseAsync(id, "mcp-client", cancellationToken);
            if (!released)
                return JsonSerializer.Serialize(new { error = "Queue not found" });

            return JsonSerializer.Serialize(new
            {
                success = true,
                message = "Queue released successfully"
            }, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool(Name = "aiforge_get_context"), Description("Get tiered context for a work queue. Tier 1: Basic context (~500 tokens). Tier 2: +Implementation plan (~1500 tokens). Tier 3: +Item details (~3000 tokens). Tier 4: +File snapshots (~5000+ tokens).")]
    public async Task<string> GetContext(
        [Description("Queue ID (GUID)")] string queueId,
        [Description("Context tier (1-4, default: 1)")] int tier = 1,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!Guid.TryParse(queueId, out var id))
                return JsonSerializer.Serialize(new { error = "Invalid queue ID format" });

            if (tier < 1 || tier > 4)
                return JsonSerializer.Serialize(new { error = "Tier must be between 1 and 4" });

            var context = await _queueService.GetTieredContextAsync(id, tier, cancellationToken);
            if (context == null)
                return JsonSerializer.Serialize(new { error = "Queue not found" });

            return JsonSerializer.Serialize(context, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool(Name = "aiforge_update_context"), Description("Update the context helper for a queue. Use this to track progress, decisions, and next steps as you work.")]
    public async Task<string> UpdateContext(
        [Description("Queue ID (GUID)")] string queueId,
        [Description("Current focus/task being worked on")] string? currentFocus = null,
        [Description("New key decisions made (will be appended)")] string[]? keyDecisions = null,
        [Description("Blockers that were resolved (will be appended)")] string[]? blockersResolved = null,
        [Description("Updated next steps (will replace existing)")] string[]? nextSteps = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!Guid.TryParse(queueId, out var id))
                return JsonSerializer.Serialize(new { error = "Invalid queue ID format" });

            var request = new UpdateContextRequest
            {
                CurrentFocus = currentFocus,
                AppendKeyDecisions = keyDecisions?.ToList(),
                AppendBlockersResolved = blockersResolved?.ToList(),
                ReplaceNextSteps = nextSteps?.ToList()
            };

            var context = await _queueService.UpdateContextAsync(id, request, cancellationToken);
            if (context == null)
                return JsonSerializer.Serialize(new { error = "Queue not found" });

            return JsonSerializer.Serialize(new
            {
                success = true,
                context = new
                {
                    context.CurrentFocus,
                    context.KeyDecisions,
                    context.BlockersResolved,
                    context.NextSteps,
                    context.LastUpdated
                },
                message = "Context updated successfully"
            }, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool(Name = "aiforge_get_queue_items"), Description("List all items in a work queue, ordered by position.")]
    public async Task<string> GetQueueItems(
        [Description("Queue ID (GUID)")] string queueId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!Guid.TryParse(queueId, out var id))
                return JsonSerializer.Serialize(new { error = "Invalid queue ID format" });

            var items = await _queueService.GetItemsAsync(id, cancellationToken);

            return JsonSerializer.Serialize(new
            {
                queueId,
                itemCount = items.Count,
                items = items.Select(i => new
                {
                    i.Id,
                    i.WorkItemId,
                    workItemType = i.WorkItemType.ToString(),
                    i.Position,
                    status = i.Status.ToString(),
                    i.Notes,
                    i.AddedAt
                })
            }, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool(Name = "aiforge_advance_queue_item"), Description("Complete the current queue item and advance to the next. Updates item status and returns the next item to work on.")]
    public async Task<string> AdvanceQueueItem(
        [Description("Queue ID (GUID)")] string queueId,
        [Description("Item ID (GUID) to mark as completed")] string itemId,
        [Description("Optional notes about completion")] string? completionNotes = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!Guid.TryParse(queueId, out var qId))
                return JsonSerializer.Serialize(new { error = "Invalid queue ID format" });

            if (!Guid.TryParse(itemId, out var iId))
                return JsonSerializer.Serialize(new { error = "Invalid item ID format" });

            // Mark current item as completed
            var updateRequest = new UpdateQueueItemRequest
            {
                Status = WorkQueueItemStatus.Completed,
                Notes = completionNotes
            };

            var completedItem = await _queueService.UpdateItemAsync(qId, iId, updateRequest, cancellationToken);
            if (completedItem == null)
                return JsonSerializer.Serialize(new { error = "Item not found" });

            // Get all items to find the next one
            var items = await _queueService.GetItemsAsync(qId, cancellationToken);
            var nextItem = items
                .Where(i => i.Status == WorkQueueItemStatus.Pending)
                .OrderBy(i => i.Position)
                .FirstOrDefault();

            return JsonSerializer.Serialize(new
            {
                success = true,
                completedItem = new
                {
                    completedItem.Id,
                    completedItem.Position,
                    status = completedItem.Status.ToString()
                },
                nextItem = nextItem != null ? new
                {
                    nextItem.Id,
                    nextItem.WorkItemId,
                    workItemType = nextItem.WorkItemType.ToString(),
                    nextItem.Position,
                    nextItem.Notes
                } : null,
                remainingItems = items.Count(i => i.Status == WorkQueueItemStatus.Pending),
                message = nextItem != null
                    ? $"Advanced to item at position {nextItem.Position}"
                    : "All items completed!"
            }, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    // Helper methods
    private async Task<Application.DTOs.Projects.ProjectDto?> ResolveProjectAsync(string keyOrId, CancellationToken ct)
    {
        if (Guid.TryParse(keyOrId, out var id))
            return await _projectService.GetByIdAsync(id, ct);

        return await _projectService.GetByKeyAsync(keyOrId.ToUpperInvariant(), ct);
    }

    private async Task<Application.DTOs.Tickets.TicketDto?> ResolveTicketAsync(string keyOrId, CancellationToken ct)
    {
        if (Guid.TryParse(keyOrId, out var id))
            return await _ticketService.GetByIdAsync(id, ct);

        return await _ticketService.GetByKeyAsync(keyOrId.ToUpperInvariant(), ct);
    }
}
