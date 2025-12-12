using AiForge.Application.Mapping;
using AiForge.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AiForge.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Add AutoMapper
        services.AddAutoMapper(typeof(MappingProfile));

        // Add Services
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<ITicketService, TicketService>();
        services.AddScoped<ICommentService, CommentService>();
        services.AddScoped<ITicketHistoryService, TicketHistoryService>();

        // AI Feature Services
        services.AddScoped<IPlanningService, PlanningService>();
        services.AddScoped<IHandoffService, HandoffService>();
        services.AddScoped<IAiContextService, AiContextService>();
        services.AddScoped<ISearchService, SearchService>();

        return services;
    }
}
