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

    public QueueTools(IWorkQueueService queueService)
    {
        _queueService = queueService;
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
}
