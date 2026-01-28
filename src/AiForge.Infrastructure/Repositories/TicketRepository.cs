using AiForge.Domain.Entities;
using AiForge.Domain.Enums;
using AiForge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AiForge.Infrastructure.Repositories;

public interface ITicketRepository
{
    Task<Ticket?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Ticket?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Ticket?> GetByKeyAsync(string key, CancellationToken cancellationToken = default);
    Task<IEnumerable<Ticket>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Ticket>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Ticket>> SearchAsync(
        Guid? projectId = null,
        TicketStatus? status = null,
        TicketType? type = null,
        Priority? priority = null,
        string? searchText = null,
        CancellationToken cancellationToken = default);
    Task<Ticket> AddAsync(Ticket ticket, CancellationToken cancellationToken = default);
    Task UpdateAsync(Ticket ticket, CancellationToken cancellationToken = default);
    Task DeleteAsync(Ticket ticket, CancellationToken cancellationToken = default);

    // Sub-ticket operations
    Task<IEnumerable<Ticket>> GetSubTicketsAsync(Guid parentTicketId, CancellationToken cancellationToken = default);
    Task<bool> HasCircularReferenceAsync(Guid ticketId, Guid proposedParentId, CancellationToken cancellationToken = default);
    Task<int> GetNestingDepthAsync(Guid ticketId, CancellationToken cancellationToken = default);
    Task<int> GetNextTicketNumberAsync(Guid projectId, CancellationToken cancellationToken = default);
}

public class TicketRepository : Repository<Ticket>, ITicketRepository
{
    public TicketRepository(AiForgeDbContext context) : base(context)
    {
    }

    public async Task<Ticket?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(t => t.Project)
            .Include(t => t.Comments.OrderByDescending(c => c.CreatedAt))
            .Include(t => t.SubTickets)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<Ticket?> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(t => t.Project)
            .FirstOrDefaultAsync(t => t.Key == key, cancellationToken);
    }

    public async Task<IEnumerable<Ticket>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(t => t.ProjectId == projectId)
            .OrderByDescending(t => t.UpdatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Ticket>> SearchAsync(
        Guid? projectId = null,
        TicketStatus? status = null,
        TicketType? type = null,
        Priority? priority = null,
        string? searchText = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsQueryable();

        if (projectId.HasValue)
            query = query.Where(t => t.ProjectId == projectId.Value);

        if (status.HasValue)
            query = query.Where(t => t.Status == status.Value);

        if (type.HasValue)
            query = query.Where(t => t.Type == type.Value);

        if (priority.HasValue)
            query = query.Where(t => t.Priority == priority.Value);

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            var search = searchText.ToLower();
            query = query.Where(t =>
                t.Title.ToLower().Contains(search) ||
                (t.Description != null && t.Description.ToLower().Contains(search)) ||
                t.Key.ToLower().Contains(search));
        }

        return await query
            .Include(t => t.Project)
            .OrderByDescending(t => t.UpdatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Ticket>> GetSubTicketsAsync(Guid parentTicketId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(t => t.ParentTicketId == parentTicketId)
            .OrderBy(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> HasCircularReferenceAsync(Guid ticketId, Guid proposedParentId, CancellationToken cancellationToken = default)
    {
        // A ticket cannot be its own parent
        if (ticketId == proposedParentId)
            return true;

        // Check if proposedParent is a descendant of ticketId (would create cycle)
        var currentId = proposedParentId;
        var visited = new HashSet<Guid> { ticketId };

        while (currentId != Guid.Empty)
        {
            if (visited.Contains(currentId))
                return true;

            visited.Add(currentId);

            var ticket = await _dbSet
                .AsNoTracking()
                .Where(t => t.Id == currentId)
                .Select(t => t.ParentTicketId)
                .FirstOrDefaultAsync(cancellationToken);

            if (ticket == null)
                break;

            currentId = ticket.Value;
        }

        return false;
    }

    public async Task<int> GetNestingDepthAsync(Guid ticketId, CancellationToken cancellationToken = default)
    {
        var depth = 0;
        var currentId = ticketId;

        while (currentId != Guid.Empty)
        {
            var parentId = await _dbSet
                .AsNoTracking()
                .Where(t => t.Id == currentId)
                .Select(t => t.ParentTicketId)
                .FirstOrDefaultAsync(cancellationToken);

            if (parentId == null)
                break;

            depth++;
            currentId = parentId.Value;
        }

        return depth;
    }

    public async Task<int> GetNextTicketNumberAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        var maxNumber = await _dbSet
            .Where(t => t.ProjectId == projectId)
            .MaxAsync(t => (int?)t.Number, cancellationToken) ?? 0;

        return maxNumber + 1;
    }
}
