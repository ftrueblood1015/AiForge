using AiForge.Domain.Enums;

namespace AiForge.Application.DTOs.WorkQueues;

public class WorkQueueDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public Guid? ImplementationPlanId { get; set; }
    public string? ImplementationPlanTitle { get; set; }
    public WorkQueueStatus Status { get; set; }
    public string? CheckedOutBy { get; set; }
    public DateTime? CheckedOutAt { get; set; }
    public DateTime? CheckoutExpiresAt { get; set; }
    public int ItemCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
}

public class WorkQueueDetailDto : WorkQueueDto
{
    public ContextHelperDto Context { get; set; } = new();
    public List<WorkQueueItemDto> Items { get; set; } = [];
}

public class ContextHelperDto
{
    public string CurrentFocus { get; set; } = string.Empty;
    public List<string> KeyDecisions { get; set; } = [];
    public List<string> BlockersResolved { get; set; } = [];
    public List<string> NextSteps { get; set; } = [];
    public DateTime LastUpdated { get; set; }
}

public class WorkQueueItemDto
{
    public Guid Id { get; set; }
    public Guid WorkItemId { get; set; }
    public WorkItemType WorkItemType { get; set; }
    public string WorkItemTitle { get; set; } = string.Empty;
    public int Position { get; set; }
    public WorkQueueItemStatus Status { get; set; }
    public string? Notes { get; set; }
    public DateTime AddedAt { get; set; }
    public string? AddedBy { get; set; }
    public DateTime? CompletedAt { get; set; }
}

// Request DTOs
public class CreateWorkQueueRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ImplementationPlanId { get; set; }
}

public class UpdateWorkQueueRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public WorkQueueStatus? Status { get; set; }
    public Guid? ImplementationPlanId { get; set; }
}

public class AddQueueItemRequest
{
    public Guid WorkItemId { get; set; }
    public WorkItemType WorkItemType { get; set; }
    public int? Position { get; set; }
    public string? Notes { get; set; }
}

public class UpdateQueueItemRequest
{
    public int? Position { get; set; }
    public WorkQueueItemStatus? Status { get; set; }
    public string? Notes { get; set; }
}

public class ReorderItemsRequest
{
    public List<Guid> ItemIds { get; set; } = [];
}

public class CheckoutRequest
{
    public int? DurationMinutes { get; set; }
}

public class UpdateContextRequest
{
    public string? CurrentFocus { get; set; }
    public List<string>? AppendKeyDecisions { get; set; }
    public List<string>? AppendBlockersResolved { get; set; }
    public List<string>? ReplaceNextSteps { get; set; }
}

// Tiered Context Response DTOs
public class TieredContextResponse
{
    public int Tier { get; set; }
    public int EstimatedTokens { get; set; }
    public QueueContextTier1 Tier1 { get; set; } = new();
    public QueueContextTier2? Tier2 { get; set; }
    public QueueContextTier3? Tier3 { get; set; }
    public QueueContextTier4? Tier4 { get; set; }
}

public class QueueContextTier1
{
    public string QueueName { get; set; } = string.Empty;
    public string? CurrentItemTitle { get; set; }
    public int TotalItems { get; set; }
    public int CompletedItems { get; set; }
    public ContextHelperDto Context { get; set; } = new();
    public bool IsStale { get; set; }
    public string? StaleWarning { get; set; }
}

public class QueueContextTier2
{
    public string? ImplementationPlanTitle { get; set; }
    public string? ImplementationPlanSummary { get; set; }
    public List<string> PlanOutline { get; set; } = [];
}

public class QueueContextTier3
{
    public WorkQueueItemDto? CurrentItem { get; set; }
    public string? ItemDescription { get; set; }
    public List<string>? AcceptanceCriteria { get; set; }
    public List<WorkQueueItemDto> NextItems { get; set; } = [];
}

public class QueueContextTier4
{
    public List<FileSnapshotSummary> RecentFileSnapshots { get; set; } = [];
    public List<string> RelatedFiles { get; set; } = [];
}

public class FileSnapshotSummary
{
    public string FilePath { get; set; } = string.Empty;
    public string? ChangeType { get; set; }
    public DateTime CapturedAt { get; set; }
}
