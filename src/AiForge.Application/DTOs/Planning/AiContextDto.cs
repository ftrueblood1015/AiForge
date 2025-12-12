using AiForge.Application.DTOs.Handoffs;
using AiForge.Application.DTOs.Tickets;

namespace AiForge.Application.DTOs.Planning;

/// <summary>
/// Aggregated context for Claude to resume work on a ticket
/// </summary>
public class AiContextDto
{
    public TicketDetailDto Ticket { get; set; } = null!;
    public HandoffDetailDto? LatestHandoff { get; set; }
    public List<ReasoningLogDto> RecentReasoning { get; set; } = new();
    public List<ProgressEntryDto> RecentProgress { get; set; } = new();
    public PlanningSessionDto? ActivePlanningSession { get; set; }
}

public class StartSessionRequest
{
    public Guid TicketId { get; set; }
    public string SessionId { get; set; } = string.Empty;
}

public class EndSessionRequest
{
    public string SessionId { get; set; } = string.Empty;
    public string? Summary { get; set; }
}
