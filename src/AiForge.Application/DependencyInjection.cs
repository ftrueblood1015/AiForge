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

        return services;
    }
}
