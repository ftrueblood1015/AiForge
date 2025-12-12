using System.ComponentModel;
using System.Text.Json;
using AiForge.Application.DTOs.Projects;
using AiForge.Application.Services;
using ModelContextProtocol.Server;

namespace AiForge.Mcp.Tools;

[McpServerToolType]
public class ProjectTools
{
    private readonly IProjectService _projectService;
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public ProjectTools(IProjectService projectService)
    {
        _projectService = projectService;
    }

    [McpServerTool(Name = "list_projects"), Description("List all projects")]
    public async Task<string> ListProjects(CancellationToken cancellationToken = default)
    {
        var projects = await _projectService.GetAllAsync(cancellationToken);
        var result = projects.Select(p => new
        {
            p.Id,
            p.Key,
            p.Name,
            p.Description,
            p.TicketCount,
            p.CreatedAt
        });

        return JsonSerializer.Serialize(result, JsonOptions);
    }

    [McpServerTool(Name = "get_project"), Description("Get detailed project information by key or ID")]
    public async Task<string> GetProject(
        [Description("Project key (e.g., DEMO, AIFORGE) or project ID (GUID)")] string projectKeyOrId,
        CancellationToken cancellationToken = default)
    {
        ProjectDto? project;

        if (Guid.TryParse(projectKeyOrId, out var id))
        {
            project = await _projectService.GetByIdAsync(id, cancellationToken);
        }
        else
        {
            project = await _projectService.GetByKeyAsync(projectKeyOrId.ToUpperInvariant(), cancellationToken);
        }

        if (project == null)
            return JsonSerializer.Serialize(new { error = $"Project '{projectKeyOrId}' not found" });

        return JsonSerializer.Serialize(new
        {
            project.Id,
            project.Key,
            project.Name,
            project.Description,
            project.TicketCount,
            project.CreatedAt,
            project.UpdatedAt
        }, JsonOptions);
    }

    [McpServerTool(Name = "create_project"), Description("Create a new project")]
    public async Task<string> CreateProject(
        [Description("Project key (uppercase, e.g., MYPROJ)")] string key,
        [Description("Project name")] string name,
        [Description("Project description (optional)")] string? description = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new CreateProjectRequest
            {
                Key = key.ToUpperInvariant(),
                Name = name,
                Description = description
            };

            var project = await _projectService.CreateAsync(request, cancellationToken);

            return JsonSerializer.Serialize(new
            {
                success = true,
                project.Id,
                project.Key,
                project.Name,
                message = $"Project {project.Key} created successfully"
            }, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }
}
