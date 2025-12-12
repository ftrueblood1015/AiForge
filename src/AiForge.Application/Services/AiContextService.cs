using AiForge.Application.DTOs.Planning;
using AiForge.Application.DTOs.Tickets;
using AiForge.Infrastructure.Repositories;

namespace AiForge.Application.Services;

public interface IAiContextService
{
    Task<AiContextDto?> GetContextByTicketIdAsync(Guid ticketId, CancellationToken cancellationToken = default);
    Task StartSessionAsync(StartSessionRequest request, CancellationToken cancellationToken = default);
    Task EndSessionAsync(EndSessionRequest request, CancellationToken cancellationToken = default);
}

public class AiContextService : IAiContextService
{
    private readonly ITicketService _ticketService;
    private readonly IHandoffService _handoffService;
    private readonly IPlanningService _planningService;
    private readonly IPlanningSessionRepository _sessionRepository;

    public AiContextService(
        ITicketService ticketService,
        IHandoffService handoffService,
        IPlanningService planningService,
        IPlanningSessionRepository sessionRepository)
    {
        _ticketService = ticketService;
        _handoffService = handoffService;
        _planningService = planningService;
        _sessionRepository = sessionRepository;
    }

    public async Task<AiContextDto?> GetContextByTicketIdAsync(Guid ticketId, CancellationToken cancellationToken = default)
    {
        var ticket = await _ticketService.GetByIdAsync(ticketId, cancellationToken);
        if (ticket == null)
            return null;

        var latestHandoff = await _handoffService.GetLatestActiveByTicketIdAsync(ticketId, cancellationToken);
        var recentReasoning = await _planningService.GetReasoningLogsByTicketIdAsync(ticketId, 10, cancellationToken);
        var recentProgress = await _planningService.GetProgressEntriesByTicketIdAsync(ticketId, 10, cancellationToken);

        // Get active planning session (not completed)
        var activeSession = await _sessionRepository.GetActiveByTicketIdAsync(ticketId, cancellationToken);
        PlanningSessionDto? activeSessionDto = null;
        if (activeSession != null)
        {
            activeSessionDto = await _planningService.GetSessionByIdAsync(activeSession.Id, cancellationToken);
        }

        return new AiContextDto
        {
            Ticket = ticket,
            LatestHandoff = latestHandoff,
            RecentReasoning = recentReasoning.ToList(),
            RecentProgress = recentProgress.ToList(),
            ActivePlanningSession = activeSessionDto
        };
    }

    public async Task StartSessionAsync(StartSessionRequest request, CancellationToken cancellationToken = default)
    {
        // This could be extended to track session starts, create planning sessions, etc.
        // For now, it's a no-op placeholder that can be expanded
        await Task.CompletedTask;
    }

    public async Task EndSessionAsync(EndSessionRequest request, CancellationToken cancellationToken = default)
    {
        // This could be extended to finalize sessions, create handoffs, etc.
        // For now, it's a no-op placeholder that can be expanded
        await Task.CompletedTask;
    }
}
