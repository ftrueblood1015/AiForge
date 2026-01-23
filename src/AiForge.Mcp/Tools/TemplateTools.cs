using System.ComponentModel;
using System.Text.Json;
using AiForge.Application.DTOs.PromptTemplate;
using AiForge.Application.Services;
using ModelContextProtocol.Server;

namespace AiForge.Mcp.Tools;

[McpServerToolType]
public class TemplateTools
{
    private readonly IPromptTemplateService _templateService;
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public TemplateTools(IPromptTemplateService templateService)
    {
        _templateService = templateService;
    }

    [McpServerTool(Name = "list_templates"), Description("List prompt templates with optional organization, project, or category filters")]
    public async Task<string> ListTemplates(
        [Description("Organization ID (GUID) to filter by")] string? organizationId = null,
        [Description("Project ID (GUID) to filter by")] string? projectId = null,
        [Description("Category filter (e.g., 'Workflow', 'Analysis', 'Documentation')")] string? category = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var orgId = string.IsNullOrEmpty(organizationId) ? null : (Guid?)Guid.Parse(organizationId);
            var projId = string.IsNullOrEmpty(projectId) ? null : (Guid?)Guid.Parse(projectId);

            var response = await _templateService.GetTemplatesAsync(orgId, projId, category, cancellationToken);

            return JsonSerializer.Serialize(new
            {
                success = true,
                totalCount = response.TotalCount,
                templates = response.Items.Select(t => new
                {
                    t.Id,
                    t.TemplateKey,
                    t.Name,
                    t.Description,
                    t.Category,
                    t.Variables,
                    t.Scope
                })
            }, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool(Name = "get_template"), Description("Get detailed prompt template information by ID or key")]
    public async Task<string> GetTemplate(
        [Description("Template ID (GUID) or template key")] string templateIdOrKey,
        [Description("Organization ID (GUID) - required when looking up by key")] string? organizationId = null,
        [Description("Project ID (GUID) - optional scope for key lookup (project-level takes precedence)")] string? projectId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            PromptTemplateDto? template;

            if (Guid.TryParse(templateIdOrKey, out var id))
            {
                template = await _templateService.GetByIdAsync(id, cancellationToken);
            }
            else
            {
                if (string.IsNullOrEmpty(organizationId))
                    return JsonSerializer.Serialize(new { error = "organizationId is required when looking up by key" });

                var orgId = Guid.Parse(organizationId);
                var projId = string.IsNullOrEmpty(projectId) ? null : (Guid?)Guid.Parse(projectId);
                template = await _templateService.GetByKeyAsync(templateIdOrKey, projId, orgId, cancellationToken);
            }

            if (template == null)
                return JsonSerializer.Serialize(new { error = $"Template '{templateIdOrKey}' not found" });

            return JsonSerializer.Serialize(new
            {
                success = true,
                template = new
                {
                    template.Id,
                    template.TemplateKey,
                    template.Name,
                    template.Description,
                    template.Template,
                    template.Variables,
                    template.Category,
                    template.OrganizationId,
                    template.ProjectId,
                    template.Scope,
                    template.CreatedAt,
                    template.UpdatedAt
                }
            }, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool(Name = "create_template"), Description("Create a new prompt template")]
    public async Task<string> CreateTemplate(
        [Description("Unique template key (e.g., 'code-review-prompt', 'commit-message')")] string templateKey,
        [Description("Display name for the template")] string name,
        [Description("Template content with {{variable}} placeholders")] string template,
        [Description("Category for grouping (e.g., 'Workflow', 'Analysis', 'Documentation')")] string category,
        [Description("Organization ID (GUID) for org-level template")] string? organizationId = null,
        [Description("Project ID (GUID) for project-level template")] string? projectId = null,
        [Description("Template description")] string? description = null,
        [Description("Variable names as comma-separated list (auto-detected if not provided)")] string? variables = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new CreatePromptTemplateRequest
            {
                TemplateKey = templateKey,
                Name = name,
                Description = description,
                Template = template,
                Category = category,
                Variables = string.IsNullOrEmpty(variables)
                    ? null
                    : variables.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList(),
                OrganizationId = string.IsNullOrEmpty(organizationId) ? null : Guid.Parse(organizationId),
                ProjectId = string.IsNullOrEmpty(projectId) ? null : Guid.Parse(projectId)
            };

            var created = await _templateService.CreateAsync(request, "mcp-session", cancellationToken);

            return JsonSerializer.Serialize(new
            {
                success = true,
                created.Id,
                created.TemplateKey,
                created.Name,
                created.Variables,
                created.Scope,
                message = $"Template '{created.Name}' created successfully"
            }, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool(Name = "render_template"), Description("Render a prompt template with variable substitution")]
    public async Task<string> RenderTemplate(
        [Description("Template key (e.g., 'code-review-prompt')")] string templateKey,
        [Description("Organization ID (GUID)")] string organizationId,
        [Description("Variables as JSON object (e.g., '{\"file\":\"main.cs\",\"language\":\"csharp\"}')")] string variablesJson,
        [Description("Project ID (GUID) - optional, for project-level override")] string? projectId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var orgId = Guid.Parse(organizationId);
            var projId = string.IsNullOrEmpty(projectId) ? null : (Guid?)Guid.Parse(projectId);

            Dictionary<string, string> variables;
            try
            {
                variables = JsonSerializer.Deserialize<Dictionary<string, string>>(variablesJson) ?? new();
            }
            catch
            {
                return JsonSerializer.Serialize(new { error = "Invalid JSON format for variables. Expected format: {\"var1\":\"value1\",\"var2\":\"value2\"}" });
            }

            var request = new RenderTemplateRequest { Variables = variables };
            var result = await _templateService.RenderByKeyAsync(templateKey, projId, orgId, request, cancellationToken);

            return JsonSerializer.Serialize(new
            {
                success = true,
                renderedContent = result.RenderedContent,
                missingVariables = result.MissingVariables,
                hasWarnings = result.MissingVariables.Count > 0,
                message = result.MissingVariables.Count > 0
                    ? $"Template rendered with {result.MissingVariables.Count} missing variable(s)"
                    : "Template rendered successfully"
            }, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool(Name = "update_template"), Description("Update an existing prompt template")]
    public async Task<string> UpdateTemplate(
        [Description("Template ID (GUID)")] string templateId,
        [Description("New name (optional)")] string? name = null,
        [Description("New description (optional)")] string? description = null,
        [Description("New template content (optional)")] string? template = null,
        [Description("New variables as comma-separated list (optional)")] string? variables = null,
        [Description("New category (optional)")] string? category = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var id = Guid.Parse(templateId);
            var request = new UpdatePromptTemplateRequest
            {
                Name = name,
                Description = description,
                Template = template,
                Variables = string.IsNullOrEmpty(variables)
                    ? null
                    : variables.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList(),
                Category = category
            };

            var updated = await _templateService.UpdateAsync(id, request, "mcp-session", cancellationToken);
            if (updated == null)
                return JsonSerializer.Serialize(new { error = $"Template '{templateId}' not found" });

            return JsonSerializer.Serialize(new
            {
                success = true,
                updated.Id,
                updated.TemplateKey,
                updated.Name,
                updated.Variables,
                message = $"Template '{updated.Name}' updated successfully"
            }, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }
}
