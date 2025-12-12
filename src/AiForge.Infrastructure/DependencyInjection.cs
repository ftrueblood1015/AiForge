using AiForge.Domain.Interfaces;
using AiForge.Infrastructure.Data;
using AiForge.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AiForge.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Add DbContext
        services.AddDbContext<AiForgeDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(AiForgeDbContext).Assembly.FullName)));

        // Add Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Add Repositories
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<ITicketRepository, TicketRepository>();
        services.AddScoped<ICommentRepository, CommentRepository>();
        services.AddScoped<ITicketHistoryRepository, TicketHistoryRepository>();
        services.AddScoped<IApiKeyRepository, ApiKeyRepository>();

        // AI Feature Repositories
        services.AddScoped<IPlanningSessionRepository, PlanningSessionRepository>();
        services.AddScoped<IReasoningLogRepository, ReasoningLogRepository>();
        services.AddScoped<IProgressEntryRepository, ProgressEntryRepository>();
        services.AddScoped<IHandoffRepository, HandoffRepository>();

        return services;
    }
}
