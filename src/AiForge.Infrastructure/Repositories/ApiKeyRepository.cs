using AiForge.Domain.Entities;
using AiForge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AiForge.Infrastructure.Repositories;

public interface IApiKeyRepository
{
    Task<ApiKey?> GetByKeyAsync(string key, CancellationToken cancellationToken = default);
    Task<ApiKey?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<ApiKey>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ApiKey> AddAsync(ApiKey apiKey, CancellationToken cancellationToken = default);
    Task UpdateAsync(ApiKey apiKey, CancellationToken cancellationToken = default);
    Task<ApiKeyUsage?> GetUsageAsync(Guid apiKeyId, DateTime windowStart, CancellationToken cancellationToken = default);
    Task<ApiKeyUsage> AddUsageAsync(ApiKeyUsage usage, CancellationToken cancellationToken = default);
    Task CleanupOldUsagesAsync(DateTime cutoff, CancellationToken cancellationToken = default);
}

public class ApiKeyRepository : Repository<ApiKey>, IApiKeyRepository
{
    public ApiKeyRepository(AiForgeDbContext context) : base(context)
    {
    }

    public async Task<ApiKey?> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(a => a.Key == key && a.IsActive, cancellationToken);
    }

    public async Task<ApiKeyUsage?> GetUsageAsync(Guid apiKeyId, DateTime windowStart, CancellationToken cancellationToken = default)
    {
        return await _context.ApiKeyUsages
            .FirstOrDefaultAsync(u => u.ApiKeyId == apiKeyId && u.WindowStart == windowStart, cancellationToken);
    }

    public async Task<ApiKeyUsage> AddUsageAsync(ApiKeyUsage usage, CancellationToken cancellationToken = default)
    {
        await _context.ApiKeyUsages.AddAsync(usage, cancellationToken);
        return usage;
    }

    public async Task CleanupOldUsagesAsync(DateTime cutoff, CancellationToken cancellationToken = default)
    {
        var oldUsages = await _context.ApiKeyUsages
            .Where(u => u.WindowStart < cutoff)
            .ToListAsync(cancellationToken);

        _context.ApiKeyUsages.RemoveRange(oldUsages);
    }
}
