using AiForge.Domain.Entities;
using AiForge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AiForge.Infrastructure.Repositories;

public interface IFileChangeRepository
{
    Task<FileChange?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<FileChange>> GetByTicketIdAsync(Guid ticketId, CancellationToken cancellationToken = default);
    Task<IEnumerable<FileChange>> GetByFilePathAsync(string filePath, CancellationToken cancellationToken = default);
    Task<IEnumerable<(string FilePath, int ChangeCount, DateTime LastModified)>> GetHotFilesAsync(int limit, CancellationToken cancellationToken = default);
    Task<FileChange> AddAsync(FileChange fileChange, CancellationToken cancellationToken = default);
}

public class FileChangeRepository : Repository<FileChange>, IFileChangeRepository
{
    public FileChangeRepository(AiForgeDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<FileChange>> GetByTicketIdAsync(Guid ticketId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(f => f.TicketId == ticketId)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<FileChange>> GetByFilePathAsync(string filePath, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(f => f.Ticket)
            .Where(f => f.FilePath == filePath)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<(string FilePath, int ChangeCount, DateTime LastModified)>> GetHotFilesAsync(int limit, CancellationToken cancellationToken = default)
    {
        var result = await _dbSet
            .GroupBy(f => f.FilePath)
            .Select(g => new
            {
                FilePath = g.Key,
                ChangeCount = g.Count(),
                LastModified = g.Max(f => f.CreatedAt)
            })
            .OrderByDescending(x => x.ChangeCount)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return result.Select(x => (x.FilePath, x.ChangeCount, x.LastModified));
    }
}
