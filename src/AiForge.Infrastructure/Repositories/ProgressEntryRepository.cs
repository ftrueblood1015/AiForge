using AiForge.Domain.Entities;
using AiForge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AiForge.Infrastructure.Repositories;

public interface IProgressEntryRepository
{
    Task<ProgressEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProgressEntry>> GetByTicketIdAsync(Guid ticketId, int? limit = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProgressEntry>> GetBySessionIdAsync(string sessionId, CancellationToken cancellationToken = default);
    Task<ProgressEntry> AddAsync(ProgressEntry entry, CancellationToken cancellationToken = default);
}

public class ProgressEntryRepository : Repository<ProgressEntry>, IProgressEntryRepository
{
    public ProgressEntryRepository(AiForgeDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ProgressEntry>> GetByTicketIdAsync(Guid ticketId, int? limit = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Where(p => p.TicketId == ticketId)
            .OrderByDescending(p => p.CreatedAt);

        if (limit.HasValue)
            return await query.Take(limit.Value).ToListAsync(cancellationToken);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ProgressEntry>> GetBySessionIdAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.SessionId == sessionId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
