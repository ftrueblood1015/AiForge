using AiForge.Domain.Enums;

namespace AiForge.Domain.Entities;

public class EffortEstimation
{
    public Guid Id { get; set; }
    public Guid TicketId { get; set; }

    // Estimation fields
    public ComplexityLevel Complexity { get; set; }
    public EffortSize EstimatedEffort { get; set; }
    public int ConfidencePercent { get; set; }  // 0-100
    public string? EstimationReasoning { get; set; }
    public string? Assumptions { get; set; }

    // Actual effort (filled when ticket completed)
    public EffortSize? ActualEffort { get; set; }
    public string? VarianceNotes { get; set; }

    // Metadata
    public int Version { get; set; } = 1;
    public string? RevisionReason { get; set; }
    public string? SessionId { get; set; }
    public bool IsLatest { get; set; } = true;
    public DateTime CreatedAt { get; set; }

    // Navigation
    public Ticket Ticket { get; set; } = null!;
}
