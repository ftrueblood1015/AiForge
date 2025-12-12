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
}
