using AiForge.Domain.Entities;
using AiForge.Domain.Enums;
using AiForge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AiForge.Infrastructure.Repositories;

public interface IImplementationPlanRepository
{
    Task<ImplementationPlan?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<ImplementationPlan>> GetByTicketIdAsync(Guid ticketId, CancellationToken cancellationToken = default);
    Task<ImplementationPlan?> GetCurrentByTicketIdAsync(Guid ticketId, CancellationToken cancellationToken = default);
    Task<ImplementationPlan?> GetApprovedByTicketIdAsync(Guid ticketId, CancellationToken cancellationToken = default);
    Task<int> GetNextVersionAsync(Guid ticketId, CancellationToken cancellationToken = default);
    Task<ImplementationPlan> AddAsync(ImplementationPlan plan, CancellationToken cancellationToken = default);
    Task UpdateAsync(ImplementationPlan plan, CancellationToken cancellationToken = default);
    Task DeleteAsync(ImplementationPlan plan, CancellationToken cancellationToken = default);
}

public class ImplementationPlanRepository : Repository<ImplementationPlan>, IImplementationPlanRepository
{
    public ImplementationPlanRepository(AiForgeDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ImplementationPlan>> GetByTicketIdAsync(Guid ticketId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.TicketId == ticketId)
            .OrderByDescending(p => p.Version)
            .ToListAsync(cancellationToken);
    }

    public async Task<ImplementationPlan?> GetCurrentByTicketIdAsync(Guid ticketId, CancellationToken cancellationToken = default)
    {
        // First try to get the latest approved plan
        var approved = await _dbSet
            .Where(p => p.TicketId == ticketId && p.Status == PlanStatus.Approved)
            .OrderByDescending(p => p.Version)
            .FirstOrDefaultAsync(cancellationToken);

        if (approved != null)
            return approved;

        // Otherwise get the latest draft
        return await _dbSet
            .Where(p => p.TicketId == ticketId && p.Status == PlanStatus.Draft)
            .OrderByDescending(p => p.Version)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ImplementationPlan?> GetApprovedByTicketIdAsync(Guid ticketId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.TicketId == ticketId && p.Status == PlanStatus.Approved)
            .OrderByDescending(p => p.Version)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<int> GetNextVersionAsync(Guid ticketId, CancellationToken cancellationToken = default)
    {
        var maxVersion = await _dbSet
            .Where(p => p.TicketId == ticketId)
            .MaxAsync(p => (int?)p.Version, cancellationToken);

        return (maxVersion ?? 0) + 1;
    }
}
