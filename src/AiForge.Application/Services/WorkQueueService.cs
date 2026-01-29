using AiForge.Application.DTOs.WorkQueues;
using AiForge.Domain.Entities;
using AiForge.Domain.Enums;
using AiForge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AiForge.Application.Services;

public interface IWorkQueueService
{
    Task<List<WorkQueueDto>> GetByProjectAsync(Guid projectId, WorkQueueStatus? status = null, CancellationToken ct = default);
    Task<WorkQueueDetailDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<WorkQueueDto> CreateAsync(Guid projectId, CreateWorkQueueRequest request, string createdBy, CancellationToken ct = default);
    Task<WorkQueueDto?> UpdateAsync(Guid id, UpdateWorkQueueRequest request, string updatedBy, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);

    // Checkout
    Task<(WorkQueueDetailDto? Queue, CheckoutConflict? Conflict)> CheckoutAsync(Guid id, CheckoutRequest request, string checkedOutBy, CancellationToken ct = default);
    Task<bool> ReleaseAsync(Guid id, string releasedBy, CancellationToken ct = default);

    // Items
    Task<List<WorkQueueItemDto>> GetItemsAsync(Guid queueId, CancellationToken ct = default);
    Task<WorkQueueItemDto?> AddItemAsync(Guid queueId, AddQueueItemRequest request, string addedBy, CancellationToken ct = default);
    Task<WorkQueueItemDto?> UpdateItemAsync(Guid queueId, Guid itemId, UpdateQueueItemRequest request, CancellationToken ct = default);
    Task<bool> RemoveItemAsync(Guid queueId, Guid itemId, CancellationToken ct = default);
    Task<bool> ReorderItemsAsync(Guid queueId, ReorderItemsRequest request, CancellationToken ct = default);

    // Context
    Task<ContextHelperDto?> GetContextAsync(Guid queueId, CancellationToken ct = default);
    Task<ContextHelperDto?> UpdateContextAsync(Guid queueId, UpdateContextRequest request, CancellationToken ct = default);
    Task<TieredContextResponse?> GetTieredContextAsync(Guid queueId, int tier, CancellationToken ct = default);
}

public record CheckoutConflict(string CheckedOutBy, DateTime? ExpiresAt);

public class WorkQueueService(AiForgeDbContext db, ILogger<WorkQueueService> logger) : IWorkQueueService
{
    public async Task<List<WorkQueueDto>> GetByProjectAsync(Guid projectId, WorkQueueStatus? status = null, CancellationToken ct = default)
    {
        var query = db.WorkQueues
            .Include(q => q.Project)
            .Include(q => q.ImplementationPlan)
            .Where(q => q.ProjectId == projectId);

        if (status.HasValue)
            query = query.Where(q => q.Status == status.Value);

        var queues = await query
            .OrderByDescending(q => q.CreatedAt)
            .AsNoTracking()
            .ToListAsync(ct);

        return queues.Select(MapToDto).ToList();
    }

    public async Task<WorkQueueDetailDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var queue = await db.WorkQueues
            .Include(q => q.Project)
            .Include(q => q.ImplementationPlan)
            .Include(q => q.Items.OrderBy(i => i.Position))
            .AsNoTracking()
            .FirstOrDefaultAsync(q => q.Id == id, ct);

        return queue == null ? null : MapToDetailDto(queue);
    }

    public async Task<WorkQueueDto> CreateAsync(Guid projectId, CreateWorkQueueRequest request, string createdBy, CancellationToken ct = default)
    {
        var project = await db.Projects.FindAsync([projectId], ct)
            ?? throw new InvalidOperationException($"Project {projectId} not found");

        var queue = new WorkQueue
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            ProjectId = projectId,
            ImplementationPlanId = request.ImplementationPlanId,
            Status = WorkQueueStatus.Active,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy,
            Context = new Domain.ValueObjects.ContextHelper
            {
                CurrentFocus = string.Empty,
                KeyDecisions = [],
                BlockersResolved = [],
                NextSteps = [],
                LastUpdated = DateTime.UtcNow
            }
        };

        db.WorkQueues.Add(queue);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("WorkQueue {QueueId} created for project {ProjectId} by {User}", queue.Id, projectId, createdBy);

        // Reload with includes
        return MapToDto((await db.WorkQueues
            .Include(q => q.Project)
            .Include(q => q.ImplementationPlan)
            .FirstAsync(q => q.Id == queue.Id, ct)));
    }

    public async Task<WorkQueueDto?> UpdateAsync(Guid id, UpdateWorkQueueRequest request, string updatedBy, CancellationToken ct = default)
    {
        var queue = await db.WorkQueues
            .Include(q => q.Project)
            .Include(q => q.ImplementationPlan)
            .FirstOrDefaultAsync(q => q.Id == id, ct);

        if (queue == null) return null;

        if (request.Name != null) queue.Name = request.Name;
        if (request.Description != null) queue.Description = request.Description;
        if (request.Status.HasValue) queue.Status = request.Status.Value;
        if (request.ImplementationPlanId.HasValue) queue.ImplementationPlanId = request.ImplementationPlanId;

        queue.UpdatedAt = DateTime.UtcNow;
        queue.UpdatedBy = updatedBy;

        await db.SaveChangesAsync(ct);

        logger.LogInformation("WorkQueue {QueueId} updated by {User}", id, updatedBy);
        return MapToDto(queue);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var queue = await db.WorkQueues.FindAsync([id], ct);
        if (queue == null) return false;

        db.WorkQueues.Remove(queue);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("WorkQueue {QueueId} deleted", id);
        return true;
    }

    public async Task<(WorkQueueDetailDto? Queue, CheckoutConflict? Conflict)> CheckoutAsync(
        Guid id, CheckoutRequest request, string checkedOutBy, CancellationToken ct = default)
    {
        var queue = await db.WorkQueues
            .Include(q => q.Project)
            .Include(q => q.ImplementationPlan)
            .Include(q => q.Items.OrderBy(i => i.Position))
            .FirstOrDefaultAsync(q => q.Id == id, ct);

        if (queue == null)
            throw new InvalidOperationException($"Queue {id} not found");

        // Check if already checked out (and not expired)
        if (queue.CheckedOutBy is not null &&
            queue.CheckedOutBy != checkedOutBy &&
            (queue.CheckoutExpiresAt is null || queue.CheckoutExpiresAt > DateTime.UtcNow))
        {
            return (null, new CheckoutConflict(queue.CheckedOutBy, queue.CheckoutExpiresAt));
        }

        // Perform checkout
        queue.CheckedOutBy = checkedOutBy;
        queue.CheckedOutAt = DateTime.UtcNow;
        queue.CheckoutExpiresAt = request.DurationMinutes.HasValue
            ? DateTime.UtcNow.AddMinutes(request.DurationMinutes.Value)
            : null;

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Queue {QueueId} checked out by {User}", id, checkedOutBy);
        return (MapToDetailDto(queue), null);
    }

    public async Task<bool> ReleaseAsync(Guid id, string releasedBy, CancellationToken ct = default)
    {
        var queue = await db.WorkQueues.FindAsync([id], ct);
        if (queue == null) return false;

        // Only allow release by the checkout holder (or if expired)
        if (queue.CheckedOutBy != releasedBy &&
            (queue.CheckoutExpiresAt is null || queue.CheckoutExpiresAt > DateTime.UtcNow))
        {
            throw new UnauthorizedAccessException("Cannot release queue checked out by another user");
        }

        queue.CheckedOutBy = null;
        queue.CheckedOutAt = null;
        queue.CheckoutExpiresAt = null;

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Queue {QueueId} released by {User}", id, releasedBy);
        return true;
    }

    public async Task<List<WorkQueueItemDto>> GetItemsAsync(Guid queueId, CancellationToken ct = default)
    {
        var items = await db.WorkQueueItems
            .Where(i => i.WorkQueueId == queueId)
            .OrderBy(i => i.Position)
            .AsNoTracking()
            .ToListAsync(ct);

        return items.Select(MapItemToDto).ToList();
    }

    public async Task<WorkQueueItemDto?> AddItemAsync(Guid queueId, AddQueueItemRequest request, string addedBy, CancellationToken ct = default)
    {
        var queue = await db.WorkQueues
            .Include(q => q.Items)
            .FirstOrDefaultAsync(q => q.Id == queueId, ct);

        if (queue == null) return null;

        // Check for duplicate
        if (queue.Items.Any(i => i.WorkItemId == request.WorkItemId && i.WorkItemType == request.WorkItemType))
        {
            throw new InvalidOperationException("Item already exists in queue");
        }

        // Calculate position
        var position = request.Position ?? (queue.Items.Count > 0 ? queue.Items.Max(i => i.Position) + 1 : 1);

        var item = new WorkQueueItem
        {
            Id = Guid.NewGuid(),
            WorkQueueId = queueId,
            WorkItemId = request.WorkItemId,
            WorkItemType = request.WorkItemType,
            Position = position,
            Status = WorkQueueItemStatus.Pending,
            Notes = request.Notes,
            AddedAt = DateTime.UtcNow,
            AddedBy = addedBy
        };

        db.WorkQueueItems.Add(item);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Item {ItemId} added to queue {QueueId} by {User}", item.Id, queueId, addedBy);
        return MapItemToDto(item);
    }

    public async Task<WorkQueueItemDto?> UpdateItemAsync(Guid queueId, Guid itemId, UpdateQueueItemRequest request, CancellationToken ct = default)
    {
        var item = await db.WorkQueueItems
            .FirstOrDefaultAsync(i => i.WorkQueueId == queueId && i.Id == itemId, ct);

        if (item == null) return null;

        if (request.Position.HasValue) item.Position = request.Position.Value;
        if (request.Status.HasValue)
        {
            item.Status = request.Status.Value;
            if (request.Status == WorkQueueItemStatus.Completed)
                item.CompletedAt = DateTime.UtcNow;
        }
        if (request.Notes != null) item.Notes = request.Notes;

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Item {ItemId} in queue {QueueId} updated", itemId, queueId);
        return MapItemToDto(item);
    }

    public async Task<bool> RemoveItemAsync(Guid queueId, Guid itemId, CancellationToken ct = default)
    {
        var item = await db.WorkQueueItems
            .FirstOrDefaultAsync(i => i.WorkQueueId == queueId && i.Id == itemId, ct);

        if (item == null) return false;

        db.WorkQueueItems.Remove(item);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Item {ItemId} removed from queue {QueueId}", itemId, queueId);
        return true;
    }

    public async Task<bool> ReorderItemsAsync(Guid queueId, ReorderItemsRequest request, CancellationToken ct = default)
    {
        var items = await db.WorkQueueItems
            .Where(i => i.WorkQueueId == queueId)
            .ToListAsync(ct);

        if (items.Count == 0) return false;

        var position = 1;
        foreach (var itemId in request.ItemIds)
        {
            var item = items.FirstOrDefault(i => i.Id == itemId);
            if (item != null)
            {
                item.Position = position++;
            }
        }

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Items reordered in queue {QueueId}", queueId);
        return true;
    }

    public async Task<ContextHelperDto?> GetContextAsync(Guid queueId, CancellationToken ct = default)
    {
        var queue = await db.WorkQueues
            .AsNoTracking()
            .FirstOrDefaultAsync(q => q.Id == queueId, ct);

        return queue == null ? null : MapContextToDto(queue.Context);
    }

    public async Task<ContextHelperDto?> UpdateContextAsync(Guid queueId, UpdateContextRequest request, CancellationToken ct = default)
    {
        var queue = await db.WorkQueues.FindAsync([queueId], ct);
        if (queue == null) return null;

        if (request.CurrentFocus != null)
            queue.Context.CurrentFocus = request.CurrentFocus;

        if (request.AppendKeyDecisions?.Count > 0)
            queue.Context.KeyDecisions.AddRange(request.AppendKeyDecisions);

        if (request.AppendBlockersResolved?.Count > 0)
            queue.Context.BlockersResolved.AddRange(request.AppendBlockersResolved);

        if (request.ReplaceNextSteps != null)
            queue.Context.NextSteps = request.ReplaceNextSteps;

        queue.Context.LastUpdated = DateTime.UtcNow;

        // Validate size
        if (!queue.Context.IsValidSize())
            throw new InvalidOperationException("ContextHelper exceeds 2KB limit. Summarize older entries.");

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Context updated for queue {QueueId}", queueId);
        return MapContextToDto(queue.Context);
    }

    public async Task<TieredContextResponse?> GetTieredContextAsync(Guid queueId, int tier, CancellationToken ct = default)
    {
        if (tier < 1 || tier > 4)
            throw new ArgumentOutOfRangeException(nameof(tier), "Tier must be between 1 and 4");

        var queue = await db.WorkQueues
            .Include(q => q.Items.OrderBy(i => i.Position))
            .Include(q => q.ImplementationPlan)
            .AsNoTracking()
            .FirstOrDefaultAsync(q => q.Id == queueId, ct);

        if (queue == null) return null;

        var response = new TieredContextResponse { Tier = tier };

        // Tier 1: Always included - basic queue info and context helper
        var currentItem = queue.Items.FirstOrDefault(i => i.Status == WorkQueueItemStatus.InProgress)
                       ?? queue.Items.FirstOrDefault(i => i.Status == WorkQueueItemStatus.Pending);

        // Check for stale context (>24 hours since last update)
        var hoursSinceUpdate = (DateTime.UtcNow - queue.Context.LastUpdated).TotalHours;
        var isStale = hoursSinceUpdate > 24;
        string? staleWarning = null;
        if (isStale)
        {
            staleWarning = $"Context last updated {hoursSinceUpdate:F0} hours ago. Review CurrentFocus and NextSteps before proceeding.";
        }

        response.Tier1 = new QueueContextTier1
        {
            QueueName = queue.Name,
            CurrentItemTitle = currentItem != null ? $"Item at position {currentItem.Position}" : null,
            TotalItems = queue.Items.Count,
            CompletedItems = queue.Items.Count(i => i.Status == WorkQueueItemStatus.Completed),
            Context = MapContextToDto(queue.Context),
            IsStale = isStale,
            StaleWarning = staleWarning
        };

        // Tier 2: Implementation plan outline
        if (tier >= 2 && queue.ImplementationPlan != null)
        {
            response.Tier2 = new QueueContextTier2
            {
                ImplementationPlanTitle = ExtractPlanTitle(queue.ImplementationPlan.Content),
                ImplementationPlanSummary = ExtractPlanSummary(queue.ImplementationPlan.Content),
                PlanOutline = ExtractPlanOutline(queue.ImplementationPlan.Content)
            };
        }

        // Tier 3: Full item details
        if (tier >= 3)
        {
            var nextItems = queue.Items
                .Where(i => i.Status == WorkQueueItemStatus.Pending && i.Id != currentItem?.Id)
                .Take(3)
                .Select(MapItemToDto)
                .ToList();

            response.Tier3 = new QueueContextTier3
            {
                CurrentItem = currentItem != null ? MapItemToDto(currentItem) : null,
                ItemDescription = currentItem?.Notes,
                AcceptanceCriteria = null, // Would need to fetch from actual work item
                NextItems = nextItems
            };
        }

        // Tier 4: File snapshots and related files from queue items
        if (tier >= 4)
        {
            // Get ticket IDs from non-completed queue items
            var ticketIds = queue.Items
                .Where(i => i.Status != WorkQueueItemStatus.Completed)
                .Select(i => i.WorkItemId)
                .ToList();

            var recentSnapshots = new List<FileSnapshotSummary>();
            var relatedFiles = new List<string>();

            if (ticketIds.Count > 0)
            {
                // Get recent file snapshots from handoffs for these tickets
                recentSnapshots = await db.FileSnapshots
                    .Where(fs => ticketIds.Contains(fs.Handoff.TicketId))
                    .OrderByDescending(fs => fs.CreatedAt)
                    .Take(20)
                    .Select(fs => new FileSnapshotSummary
                    {
                        FilePath = fs.FilePath,
                        ChangeType = fs.ContentBefore == null ? "Created"
                                   : fs.ContentAfter == null ? "Deleted"
                                   : "Modified",
                        CapturedAt = fs.CreatedAt
                    })
                    .ToListAsync(ct);

                // Get related files from file change audit log
                relatedFiles = await db.FileChanges
                    .Where(fc => ticketIds.Contains(fc.TicketId))
                    .Select(fc => fc.FilePath)
                    .Distinct()
                    .Take(50)
                    .ToListAsync(ct);
            }

            response.Tier4 = new QueueContextTier4
            {
                RecentFileSnapshots = recentSnapshots,
                RelatedFiles = relatedFiles
            };
        }

        // Estimate tokens
        response.EstimatedTokens = EstimateTokens(response);

        logger.LogInformation("Tiered context loaded for queue {QueueId} at tier {Tier}, ~{Tokens} tokens",
            queueId, tier, response.EstimatedTokens);

        return response;
    }

    private static string? ExtractPlanTitle(string? content)
    {
        if (string.IsNullOrEmpty(content)) return null;

        // Find first H1 header (# Title)
        var lines = content.Split('\n');
        var h1 = lines.FirstOrDefault(l => l.TrimStart().StartsWith("# "));
        return h1?.TrimStart('#', ' ').Trim();
    }

    private static string? ExtractPlanSummary(string? content)
    {
        if (string.IsNullOrEmpty(content)) return null;

        // Extract first non-empty, non-header paragraph
        var lines = content.Split('\n');
        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (!string.IsNullOrEmpty(trimmed) && !trimmed.StartsWith('#'))
            {
                // Return first 200 chars of first paragraph
                return trimmed.Length > 200 ? trimmed[..200] + "..." : trimmed;
            }
        }
        return null;
    }

    private static List<string> ExtractPlanOutline(string? content)
    {
        if (string.IsNullOrEmpty(content)) return [];

        // Extract headers from markdown content
        var lines = content.Split('\n');
        return lines
            .Where(l => l.TrimStart().StartsWith('#'))
            .Select(l => l.TrimStart('#', ' '))
            .Take(10)
            .ToList();
    }

    private static int EstimateTokens(TieredContextResponse response)
    {
        // Rough estimate: 1 token ≈ 4 characters
        var json = System.Text.Json.JsonSerializer.Serialize(response);
        return json.Length / 4;
    }

    // Mapping helpers
    private static WorkQueueDto MapToDto(WorkQueue queue) => new()
    {
        Id = queue.Id,
        Name = queue.Name,
        Description = queue.Description,
        ProjectId = queue.ProjectId,
        ProjectName = queue.Project?.Name ?? string.Empty,
        ImplementationPlanId = queue.ImplementationPlanId,
        ImplementationPlanTitle = ExtractPlanTitle(queue.ImplementationPlan?.Content),
        Status = queue.Status,
        CheckedOutBy = queue.CheckedOutBy,
        CheckedOutAt = queue.CheckedOutAt,
        CheckoutExpiresAt = queue.CheckoutExpiresAt,
        ItemCount = queue.Items?.Count ?? 0,
        CreatedAt = queue.CreatedAt,
        CreatedBy = queue.CreatedBy
    };

    private static WorkQueueDetailDto MapToDetailDto(WorkQueue queue) => new()
    {
        Id = queue.Id,
        Name = queue.Name,
        Description = queue.Description,
        ProjectId = queue.ProjectId,
        ProjectName = queue.Project?.Name ?? string.Empty,
        ImplementationPlanId = queue.ImplementationPlanId,
        ImplementationPlanTitle = ExtractPlanTitle(queue.ImplementationPlan?.Content),
        Status = queue.Status,
        CheckedOutBy = queue.CheckedOutBy,
        CheckedOutAt = queue.CheckedOutAt,
        CheckoutExpiresAt = queue.CheckoutExpiresAt,
        ItemCount = queue.Items?.Count ?? 0,
        CreatedAt = queue.CreatedAt,
        CreatedBy = queue.CreatedBy,
        Context = MapContextToDto(queue.Context),
        Items = queue.Items?.Select(MapItemToDto).ToList() ?? []
    };

    private static ContextHelperDto MapContextToDto(Domain.ValueObjects.ContextHelper context) => new()
    {
        CurrentFocus = context.CurrentFocus,
        KeyDecisions = context.KeyDecisions,
        BlockersResolved = context.BlockersResolved,
        NextSteps = context.NextSteps,
        LastUpdated = context.LastUpdated
    };

    private static WorkQueueItemDto MapItemToDto(WorkQueueItem item) => new()
    {
        Id = item.Id,
        WorkItemId = item.WorkItemId,
        WorkItemType = item.WorkItemType,
        WorkItemTitle = string.Empty, // Would need join to get title
        Position = item.Position,
        Status = item.Status,
        Notes = item.Notes,
        AddedAt = item.AddedAt,
        AddedBy = item.AddedBy,
        CompletedAt = item.CompletedAt
    };
}
