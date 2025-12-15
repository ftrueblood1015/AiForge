using AiForge.Domain.Enums;

namespace AiForge.Application.DTOs.Estimation;

public class EffortEstimationDto
{
    public Guid Id { get; set; }
    public Guid TicketId { get; set; }
    public string Complexity { get; set; } = string.Empty;
    public string EstimatedEffort { get; set; } = string.Empty;
    public int ConfidencePercent { get; set; }
    public string? EstimationReasoning { get; set; }
    public string? Assumptions { get; set; }
    public string? ActualEffort { get; set; }
    public string? VarianceNotes { get; set; }
    public int Version { get; set; }
    public string? RevisionReason { get; set; }
    public string? SessionId { get; set; }
    public bool IsLatest { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateEstimationRequest
{
    public string Complexity { get; set; } = string.Empty;
    public string EstimatedEffort { get; set; } = string.Empty;
    public int ConfidencePercent { get; set; }
    public string? EstimationReasoning { get; set; }
    public string? Assumptions { get; set; }
    public string? SessionId { get; set; }
}

public class ReviseEstimationRequest
{
    public string Complexity { get; set; } = string.Empty;
    public string EstimatedEffort { get; set; } = string.Empty;
    public int ConfidencePercent { get; set; }
    public string? EstimationReasoning { get; set; }
    public string? Assumptions { get; set; }
    public string RevisionReason { get; set; } = string.Empty;
    public string? SessionId { get; set; }
}

public class RecordActualEffortRequest
{
    public string ActualEffort { get; set; } = string.Empty;
    public string? VarianceNotes { get; set; }
}

public class EstimationHistoryResponse
{
    public Guid TicketId { get; set; }
    public List<EffortEstimationDto> Estimations { get; set; } = new();
    public int TotalVersions { get; set; }
}
