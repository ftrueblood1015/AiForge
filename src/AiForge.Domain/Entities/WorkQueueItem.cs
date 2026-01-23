using AiForge.Domain.Enums;

namespace AiForge.Domain.Entities;

public class WorkQueueItem
{
    public Guid Id { get; set; }

    // Parent queue
    public Guid WorkQueueId { get; set; }
    public WorkQueue WorkQueue { get; set; } = null!;

    // Polymorphic link to work item
    public Guid WorkItemId { get; set; }
    public WorkItemType WorkItemType { get; set; }

    // Ordering and status
    public int Position { get; set; }
    public WorkQueueItemStatus Status { get; set; } = WorkQueueItemStatus.Pending;

    // Item-specific context
    public string? Notes { get; set; }

    // Audit
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    public string? AddedBy { get; set; }
    public DateTime? CompletedAt { get; set; }
}
