using AiForge.Domain.Interfaces;

namespace AiForge.Infrastructure.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly AiForgeDbContext _context;
    private bool _disposed;

    public UnitOfWork(AiForgeDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
