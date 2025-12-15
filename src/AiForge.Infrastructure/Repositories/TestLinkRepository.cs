using AiForge.Domain.Entities;
using AiForge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AiForge.Infrastructure.Repositories;

public interface ITestLinkRepository
{
    Task<TestLink?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<TestLink>> GetByTicketIdAsync(Guid ticketId, CancellationToken cancellationToken = default);
    Task<IEnumerable<TestLink>> GetByTestFilePathAsync(string testFilePath, CancellationToken cancellationToken = default);
    Task<IEnumerable<TestLink>> GetByLinkedFilePathAsync(string linkedFilePath, CancellationToken cancellationToken = default);
    Task<IEnumerable<string>> GetFilesWithoutTestsAsync(CancellationToken cancellationToken = default);
    Task<TestLink> AddAsync(TestLink testLink, CancellationToken cancellationToken = default);
    Task UpdateAsync(TestLink testLink, CancellationToken cancellationToken = default);
}

public class TestLinkRepository : Repository<TestLink>, ITestLinkRepository
{
    public TestLinkRepository(AiForgeDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<TestLink>> GetByTicketIdAsync(Guid ticketId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(t => t.TicketId == ticketId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<TestLink>> GetByTestFilePathAsync(string testFilePath, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(t => t.Ticket)
            .Where(t => t.TestFilePath == testFilePath)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<TestLink>> GetByLinkedFilePathAsync(string linkedFilePath, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(t => t.Ticket)
            .Where(t => t.LinkedFilePath == linkedFilePath)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<string>> GetFilesWithoutTestsAsync(CancellationToken cancellationToken = default)
    {
        // Get all unique file paths from FileChanges that don't have TestLinks
        var fileChanges = _context.FileChanges;
        var testLinks = _context.TestLinks;

        var filesWithTests = await testLinks
            .Where(t => t.LinkedFilePath != null)
            .Select(t => t.LinkedFilePath!)
            .Distinct()
            .ToListAsync(cancellationToken);

        var allFiles = await fileChanges
            .Select(f => f.FilePath)
            .Distinct()
            .ToListAsync(cancellationToken);

        return allFiles.Except(filesWithTests);
    }
}
