using AiForge.Domain.Entities;
using AiForge.Domain.Enums;
using AiForge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AiForge.Infrastructure.Repositories;

public interface ITechnicalDebtRepository
{
    Task<TechnicalDebt?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<TechnicalDebt>> GetByTicketIdAsync(Guid ticketId, CancellationToken cancellationToken = default);
    Task<IEnumerable<TechnicalDebt>> GetBacklogAsync(DebtStatus? status, DebtCategory? category, DebtSeverity? severity, CancellationToken cancellationToken = default);
    Task<TechnicalDebtSummary> GetSummaryAsync(CancellationToken cancellationToken = default);
    Task<TechnicalDebt> AddAsync(TechnicalDebt debt, CancellationToken cancellationToken = default);
    Task UpdateAsync(TechnicalDebt debt, CancellationToken cancellationToken = default);
}

public class TechnicalDebtSummary
{
    public int TotalOpen { get; set; }
    public int TotalResolved { get; set; }
    public Dictionary<string, int> ByCategory { get; set; } = new();
    public Dictionary<string, int> BySeverity { get; set; } = new();
}

public class TechnicalDebtRepository : Repository<TechnicalDebt>, ITechnicalDebtRepository
{
    public TechnicalDebtRepository(AiForgeDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<TechnicalDebt>> GetByTicketIdAsync(Guid ticketId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(d => d.OriginatingTicket)
            .Include(d => d.ResolutionTicket)
            .Where(d => d.OriginatingTicketId == ticketId)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<TechnicalDebt>> GetBacklogAsync(DebtStatus? status, DebtCategory? category, DebtSeverity? severity, CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Include(d => d.OriginatingTicket)
            .Include(d => d.ResolutionTicket)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(d => d.Status == status.Value);

        if (category.HasValue)
            query = query.Where(d => d.Category == category.Value);

        if (severity.HasValue)
            query = query.Where(d => d.Severity == severity.Value);

        return await query
            .OrderByDescending(d => d.Severity)
            .ThenByDescending(d => d.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<TechnicalDebtSummary> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var allDebts = await _dbSet.ToListAsync(cancellationToken);

        return new TechnicalDebtSummary
        {
            TotalOpen = allDebts.Count(d => d.Status != DebtStatus.Resolved),
            TotalResolved = allDebts.Count(d => d.Status == DebtStatus.Resolved),
            ByCategory = allDebts
                .Where(d => d.Status != DebtStatus.Resolved)
                .GroupBy(d => d.Category.ToString())
                .ToDictionary(g => g.Key, g => g.Count()),
            BySeverity = allDebts
                .Where(d => d.Status != DebtStatus.Resolved)
                .GroupBy(d => d.Severity.ToString())
                .ToDictionary(g => g.Key, g => g.Count())
        };
    }
}
