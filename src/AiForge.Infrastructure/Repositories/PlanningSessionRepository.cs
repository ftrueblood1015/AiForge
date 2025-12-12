using AiForge.Domain.Entities;
using AiForge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AiForge.Infrastructure.Repositories;

public interface IPlanningSessionRepository
{
    Task<PlanningSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<PlanningSession>> GetByTicketIdAsync(Guid ticketId, CancellationToken cancellationToken = default);
    Task<PlanningSession?> GetActiveByTicketIdAsync(Guid ticketId, CancellationToken cancellationToken = default);
    Task<PlanningSession> AddAsync(PlanningSession session, CancellationToken cancellationToken = default);
    Task UpdateAsync(PlanningSession session, CancellationToken cancellationToken = default);
}

public class PlanningSessionRepository : Repository<PlanningSession>, IPlanningSessionRepository
{
    public PlanningSessionRepository(AiForgeDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<PlanningSession>> GetByTicketIdAsync(Guid ticketId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.TicketId == ticketId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<PlanningSession?> GetActiveByTicketIdAsync(Guid ticketId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.TicketId == ticketId && p.CompletedAt == null)
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
