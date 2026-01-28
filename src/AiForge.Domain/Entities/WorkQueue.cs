using AiForge.Domain.Enums;
using AiForge.Domain.ValueObjects;

namespace AiForge.Domain.Entities;

public class WorkQueue
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Relationships
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public Guid? ImplementationPlanId { get; set; }
    public ImplementationPlan? ImplementationPlan { get; set; }

    // Status
    public WorkQueueStatus Status { get; set; } = WorkQueueStatus.Active;

    // Context (JSON column)
    public ContextHelper Context { get; set; } = new();

    // Checkout tracking
    public string? CheckedOutBy { get; set; }
    public DateTime? CheckedOutAt { get; set; }
    public DateTime? CheckoutExpiresAt { get; set; }

    // Audit
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // Navigation
    public ICollection<WorkQueueItem> Items { get; set; } = [];
}
