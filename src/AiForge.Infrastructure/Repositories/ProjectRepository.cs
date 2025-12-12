using AiForge.Domain.Entities;
using AiForge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AiForge.Infrastructure.Repositories;

public interface IProjectRepository
{
    Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Project?> GetByKeyAsync(string key, CancellationToken cancellationToken = default);
    Task<IEnumerable<Project>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Project> AddAsync(Project project, CancellationToken cancellationToken = default);
    Task UpdateAsync(Project project, CancellationToken cancellationToken = default);
    Task DeleteAsync(Project project, CancellationToken cancellationToken = default);
    Task<bool> ExistsByKeyAsync(string key, CancellationToken cancellationToken = default);
    Task<int> GetAndIncrementNextTicketNumberAsync(Guid projectId, CancellationToken cancellationToken = default);
}

public class ProjectRepository : Repository<Project>, IProjectRepository
{
    public ProjectRepository(AiForgeDbContext context) : base(context)
    {
    }

    public async Task<Project?> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(p => p.Key == key, cancellationToken);
    }

    public async Task<bool> ExistsByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(p => p.Key == key, cancellationToken);
    }

    public async Task<int> GetAndIncrementNextTicketNumberAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        var project = await _dbSet.FindAsync(new object[] { projectId }, cancellationToken)
            ?? throw new InvalidOperationException($"Project with ID {projectId} not found");

        var nextNumber = project.NextTicketNumber;
        project.NextTicketNumber++;
        project.UpdatedAt = DateTime.UtcNow;

        return nextNumber;
    }
}
