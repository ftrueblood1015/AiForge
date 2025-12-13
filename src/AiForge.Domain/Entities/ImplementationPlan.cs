using AiForge.Domain.Enums;

namespace AiForge.Domain.Entities;

public class ImplementationPlan
{
    public Guid Id { get; set; }
    public Guid TicketId { get; set; }
    public string Content { get; set; } = string.Empty;             // Markdown content
    public PlanStatus Status { get; set; }
    public int Version { get; set; }                                // Starts at 1, increments per ticket
    public string? EstimatedEffort { get; set; }                    // e.g., "Small", "Medium", "Large"
    public string? AffectedFiles { get; set; }                      // JSON array of file paths
    public string? DependencyTicketIds { get; set; }                // JSON array of ticket IDs
    public string? CreatedBy { get; set; }                          // Session ID or user identifier
    public DateTime CreatedAt { get; set; }
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public Guid? SupersededById { get; set; }                       // FK to newer plan that replaced this one
    public DateTime? SupersededAt { get; set; }
    public string? RejectedBy { get; set; }
    public DateTime? RejectedAt { get; set; }
    public string? RejectionReason { get; set; }

    // Navigation
    public Ticket Ticket { get; set; } = null!;
    public ImplementationPlan? SupersededBy { get; set; }
}
