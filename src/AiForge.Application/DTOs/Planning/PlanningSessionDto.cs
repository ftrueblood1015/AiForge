namespace AiForge.Application.DTOs.Planning;

public class PlanningSessionDto
{
    public Guid Id { get; set; }
    public Guid TicketId { get; set; }
    public string? SessionId { get; set; }
    public string InitialUnderstanding { get; set; } = string.Empty;
    public List<string> Assumptions { get; set; } = new();
    public List<string> AlternativesConsidered { get; set; } = new();
    public string? ChosenApproach { get; set; }
    public string? Rationale { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public bool IsCompleted => CompletedAt.HasValue;
}

public class CreatePlanningSessionRequest
{
    public Guid TicketId { get; set; }
    public string? SessionId { get; set; }
    public string InitialUnderstanding { get; set; } = string.Empty;
    public List<string>? Assumptions { get; set; }
}

public class UpdatePlanningSessionRequest
{
    public List<string>? Assumptions { get; set; }
    public List<string>? AlternativesConsidered { get; set; }
    public string? ChosenApproach { get; set; }
    public string? Rationale { get; set; }
}

public class CompletePlanningSessionRequest
{
    public string? ChosenApproach { get; set; }
    public string? Rationale { get; set; }
}
