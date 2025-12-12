using AiForge.Domain.Entities;
using AiForge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AiForge.Infrastructure.Repositories;

public interface IReasoningLogRepository
{
    Task<ReasoningLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<ReasoningLog>> GetByTicketIdAsync(Guid ticketId, int? limit = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<ReasoningLog>> GetBySessionIdAsync(string sessionId, CancellationToken cancellationToken = default);
    Task<ReasoningLog> AddAsync(ReasoningLog log, CancellationToken cancellationToken = default);
}

public class ReasoningLogRepository : Repository<ReasoningLog>, IReasoningLogRepository
{
    public ReasoningLogRepository(AiForgeDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ReasoningLog>> GetByTicketIdAsync(Guid ticketId, int? limit = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Where(r => r.TicketId == ticketId)
            .OrderByDescending(r => r.CreatedAt);

        if (limit.HasValue)
            return await query.Take(limit.Value).ToListAsync(cancellationToken);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ReasoningLog>> GetBySessionIdAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(r => r.SessionId == sessionId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
