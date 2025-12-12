using AiForge.Domain.Entities;
using AiForge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AiForge.Infrastructure.Repositories;

public interface ITicketHistoryRepository
{
    Task<IEnumerable<TicketHistory>> GetByTicketIdAsync(Guid ticketId, CancellationToken cancellationToken = default);
    Task<TicketHistory> AddAsync(TicketHistory history, CancellationToken cancellationToken = default);
}

public class TicketHistoryRepository : Repository<TicketHistory>, ITicketHistoryRepository
{
    public TicketHistoryRepository(AiForgeDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<TicketHistory>> GetByTicketIdAsync(Guid ticketId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(h => h.TicketId == ticketId)
            .OrderByDescending(h => h.ChangedAt)
            .ToListAsync(cancellationToken);
    }
}
