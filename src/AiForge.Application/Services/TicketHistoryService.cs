using AutoMapper;
using AiForge.Application.DTOs.Tickets;
using AiForge.Infrastructure.Repositories;

namespace AiForge.Application.Services;

public interface ITicketHistoryService
{
    Task<IEnumerable<TicketHistoryDto>> GetByTicketIdAsync(Guid ticketId, CancellationToken cancellationToken = default);
}

public class TicketHistoryService : ITicketHistoryService
{
    private readonly ITicketHistoryRepository _historyRepository;
    private readonly IMapper _mapper;

    public TicketHistoryService(ITicketHistoryRepository historyRepository, IMapper mapper)
    {
        _historyRepository = historyRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<TicketHistoryDto>> GetByTicketIdAsync(Guid ticketId, CancellationToken cancellationToken = default)
    {
        var history = await _historyRepository.GetByTicketIdAsync(ticketId, cancellationToken);
        return _mapper.Map<IEnumerable<TicketHistoryDto>>(history);
    }
}
