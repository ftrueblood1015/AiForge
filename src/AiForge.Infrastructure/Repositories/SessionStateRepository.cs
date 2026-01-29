using AiForge.Domain.Entities;
using AiForge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AiForge.Infrastructure.Repositories;

public interface ISessionStateRepository
{
    Task<SessionState?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<SessionState?> GetBySessionIdAsync(string sessionId, CancellationToken cancellationToken = default);
    Task<SessionState?> GetActiveByTicketIdAsync(Guid ticketId, CancellationToken cancellationToken = default);
    Task<IEnumerable<SessionState>> GetExpiredAsync(DateTime olderThan, CancellationToken cancellationToken = default);
    Task<SessionState> AddAsync(SessionState state, CancellationToken cancellationToken = default);
    Task UpdateAsync(SessionState state, CancellationToken cancellationToken = default);
    Task DeleteAsync(SessionState state, CancellationToken cancellationToken = default);
    Task DeleteRangeAsync(IEnumerable<SessionState> states, CancellationToken cancellationToken = default);
}

public class SessionStateRepository : Repository<SessionState>, ISessionStateRepository
{
    public SessionStateRepository(AiForgeDbContext context) : base(context)
    {
    }

    public async Task<SessionState?> GetBySessionIdAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(s => s.Ticket)
            .Include(s => s.Queue)
            .FirstOrDefaultAsync(s => s.SessionId == sessionId, cancellationToken);
    }

    public async Task<SessionState?> GetActiveByTicketIdAsync(Guid ticketId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(s => s.Ticket)
            .Where(s => s.TicketId == ticketId && s.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(s => s.UpdatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<SessionState>> GetExpiredAsync(DateTime olderThan, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(s => s.ExpiresAt < olderThan)
            .ToListAsync(cancellationToken);
    }

    public async Task DeleteRangeAsync(IEnumerable<SessionState> states, CancellationToken cancellationToken = default)
    {
        _dbSet.RemoveRange(states);
        await Task.CompletedTask;
    }
}
