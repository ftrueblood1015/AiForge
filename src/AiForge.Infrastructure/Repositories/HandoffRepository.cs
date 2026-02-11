using AiForge.Domain.Entities;
using AiForge.Domain.Enums;
using AiForge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AiForge.Infrastructure.Repositories;

public interface IHandoffRepository
{
    Task<HandoffDocument?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<HandoffDocument?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<HandoffDocument?> GetLatestActiveByTicketIdAsync(Guid ticketId, CancellationToken cancellationToken = default);
    Task<IEnumerable<HandoffDocument>> GetByTicketIdAsync(Guid ticketId, CancellationToken cancellationToken = default);
    Task<IEnumerable<HandoffDocument>> SearchAsync(
        Guid? ticketId = null,
        HandoffType? type = null,
        string? searchText = null,
        CancellationToken cancellationToken = default);
    Task<HandoffDocument> AddAsync(HandoffDocument handoff, CancellationToken cancellationToken = default);
    Task UpdateAsync(HandoffDocument handoff, CancellationToken cancellationToken = default);
    Task<FileSnapshot> AddFileSnapshotAsync(FileSnapshot snapshot, CancellationToken cancellationToken = default);
    Task<IEnumerable<FileSnapshot>> GetFileSnapshotsByHandoffIdAsync(Guid handoffId, CancellationToken cancellationToken = default);
    Task<HandoffVersion> AddVersionAsync(HandoffVersion version, CancellationToken cancellationToken = default);
    Task<int> GetNextVersionNumberAsync(Guid handoffId, CancellationToken cancellationToken = default);
}

public class HandoffRepository : Repository<HandoffDocument>, IHandoffRepository
{
    public HandoffRepository(AiForgeDbContext context) : base(context)
    {
    }

    public async Task<HandoffDocument?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(h => h.Ticket)
            .Include(h => h.FileSnapshots)
            .Include(h => h.Versions)
            .FirstOrDefaultAsync(h => h.Id == id, cancellationToken);
    }

    public async Task<HandoffDocument?> GetLatestActiveByTicketIdAsync(Guid ticketId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(h => h.FileSnapshots)
            .Where(h => h.TicketId == ticketId && h.IsActive)
            .OrderByDescending(h => h.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<HandoffDocument>> GetByTicketIdAsync(Guid ticketId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(h => h.TicketId == ticketId)
            .OrderByDescending(h => h.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<HandoffDocument>> SearchAsync(
        Guid? ticketId = null,
        HandoffType? type = null,
        string? searchText = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Include(h => h.Ticket).AsQueryable();

        if (ticketId.HasValue)
            query = query.Where(h => h.TicketId == ticketId.Value);

        if (type.HasValue)
            query = query.Where(h => h.Type == type.Value);

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            var search = searchText.ToLower();
            query = query.Where(h =>
                h.Title.ToLower().Contains(search) ||
                h.Summary.ToLower().Contains(search) ||
                h.Content.ToLower().Contains(search));
        }

        return await query
            .OrderByDescending(h => h.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<FileSnapshot> AddFileSnapshotAsync(FileSnapshot snapshot, CancellationToken cancellationToken = default)
    {
        await _context.FileSnapshots.AddAsync(snapshot, cancellationToken);
        return snapshot;
    }

    public async Task<IEnumerable<FileSnapshot>> GetFileSnapshotsByHandoffIdAsync(Guid handoffId, CancellationToken cancellationToken = default)
    {
        return await _context.FileSnapshots
            .Where(f => f.HandoffId == handoffId)
            .OrderBy(f => f.FilePath)
            .ToListAsync(cancellationToken);
    }

    public async Task<HandoffVersion> AddVersionAsync(HandoffVersion version, CancellationToken cancellationToken = default)
    {
        await _context.HandoffVersions.AddAsync(version, cancellationToken);
        return version;
    }

    public async Task<int> GetNextVersionNumberAsync(Guid handoffId, CancellationToken cancellationToken = default)
    {
        var maxVersion = await _context.HandoffVersions
            .Where(v => v.HandoffId == handoffId)
            .MaxAsync(v => (int?)v.Version, cancellationToken);

        return (maxVersion ?? 0) + 1;
    }
}
