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
        services.AddScoped<IImplementationPlanService, ImplementationPlanService>();
        services.AddScoped<IEffortEstimationService, EffortEstimationService>();

        // Code Intelligence Services
        services.AddScoped<IFileChangeService, FileChangeService>();
        services.AddScoped<ITestLinkService, TestLinkService>();
        services.AddScoped<ITechnicalDebtService, TechnicalDebtService>();

        // Analytics Services
        services.AddScoped<IAnalyticsService, AnalyticsService>();

        // Configuration & Scope Services
        services.AddScoped<IScopeResolver, ScopeResolver>();
        services.AddScoped<IAgentService, AgentService>();
        services.AddScoped<ISkillService, SkillService>();
        services.AddScoped<IPromptTemplateService, PromptTemplateService>();

        // Work Queue Services
        services.AddScoped<IWorkQueueService, WorkQueueService>();

        return services;
    }
}
