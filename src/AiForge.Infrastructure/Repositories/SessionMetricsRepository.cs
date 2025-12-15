using AiForge.Domain.Entities;
using AiForge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AiForge.Infrastructure.Repositories;

public interface ISessionMetricsRepository
{
    Task<SessionMetrics?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<SessionMetrics?> GetBySessionIdAsync(string sessionId, CancellationToken cancellationToken = default);
    Task<IEnumerable<SessionMetrics>> GetByTicketIdAsync(Guid ticketId, CancellationToken cancellationToken = default);
    Task<SessionMetrics> AddAsync(SessionMetrics sessionMetrics, CancellationToken cancellationToken = default);
    Task UpdateAsync(SessionMetrics sessionMetrics, CancellationToken cancellationToken = default);
}

public class SessionMetricsRepository : Repository<SessionMetrics>, ISessionMetricsRepository
{
    public SessionMetricsRepository(AiForgeDbContext context) : base(context)
    {
    }

    public async Task<SessionMetrics?> GetBySessionIdAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(s => s.Ticket)
            .FirstOrDefaultAsync(s => s.SessionId == sessionId, cancellationToken);
    }

    public async Task<IEnumerable<SessionMetrics>> GetByTicketIdAsync(Guid ticketId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(s => s.TicketId == ticketId)
            .OrderByDescending(s => s.SessionStartedAt)
            .ToListAsync(cancellationToken);
    }
}
