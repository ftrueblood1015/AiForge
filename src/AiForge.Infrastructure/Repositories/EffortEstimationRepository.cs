using AiForge.Domain.Entities;
using AiForge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AiForge.Infrastructure.Repositories;

public interface IEffortEstimationRepository
{
    Task<EffortEstimation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<EffortEstimation?> GetLatestByTicketIdAsync(Guid ticketId, CancellationToken cancellationToken = default);
    Task<IEnumerable<EffortEstimation>> GetAllByTicketIdAsync(Guid ticketId, CancellationToken cancellationToken = default);
    Task<int> GetNextVersionAsync(Guid ticketId, CancellationToken cancellationToken = default);
    Task<EffortEstimation> AddAsync(EffortEstimation estimation, CancellationToken cancellationToken = default);
    Task UpdateAsync(EffortEstimation estimation, CancellationToken cancellationToken = default);
}

public class EffortEstimationRepository : Repository<EffortEstimation>, IEffortEstimationRepository
{
    public EffortEstimationRepository(AiForgeDbContext context) : base(context)
    {
    }

    public async Task<EffortEstimation?> GetLatestByTicketIdAsync(Guid ticketId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(e => e.TicketId == ticketId && e.IsLatest)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<EffortEstimation>> GetAllByTicketIdAsync(Guid ticketId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(e => e.TicketId == ticketId)
            .OrderByDescending(e => e.Version)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetNextVersionAsync(Guid ticketId, CancellationToken cancellationToken = default)
    {
        var maxVersion = await _dbSet
            .Where(e => e.TicketId == ticketId)
            .MaxAsync(e => (int?)e.Version, cancellationToken);

        return (maxVersion ?? 0) + 1;
    }
}
