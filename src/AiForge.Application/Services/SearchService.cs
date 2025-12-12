using AiForge.Application.DTOs.Handoffs;
using AiForge.Application.DTOs.Tickets;
using AiForge.Domain.Enums;

namespace AiForge.Application.Services;

public interface ISearchService
{
    Task<SearchResultDto> SearchAsync(string query, SearchType? type, Guid? projectId, CancellationToken cancellationToken = default);
}

public enum SearchType
{
    All,
    Tickets,
    Handoffs
}

public class SearchResultDto
{
    public List<TicketDto> Tickets { get; set; } = new();
    public List<HandoffDto> Handoffs { get; set; } = new();
    public int TotalCount => Tickets.Count + Handoffs.Count;
}

public class SearchService : ISearchService
{
    private readonly ITicketService _ticketService;
    private readonly IHandoffService _handoffService;

    public SearchService(ITicketService ticketService, IHandoffService handoffService)
    {
        _ticketService = ticketService;
        _handoffService = handoffService;
    }

    public async Task<SearchResultDto> SearchAsync(string query, SearchType? type, Guid? projectId, CancellationToken cancellationToken = default)
    {
        var result = new SearchResultDto();
        var searchType = type ?? SearchType.All;

        if (searchType == SearchType.All || searchType == SearchType.Tickets)
        {
            var ticketRequest = new TicketSearchRequest
            {
                ProjectId = projectId,
                Search = query
            };
            var tickets = await _ticketService.SearchAsync(ticketRequest, cancellationToken);
            result.Tickets = tickets.ToList();
        }

        if (searchType == SearchType.All || searchType == SearchType.Handoffs)
        {
            // For handoffs, we search across all tickets if projectId is not specified
            // If projectId is specified, we'd need to filter handoffs by tickets in that project
            var handoffs = await _handoffService.SearchAsync(null, null, query, cancellationToken);

            // If projectId is specified, we need to filter by project
            // This would require joining with tickets - for now, return all matches
            result.Handoffs = handoffs.ToList();
        }

        return result;
    }
}
