using System.ComponentModel;
using System.Text.Json;
using AiForge.Application.DTOs.Skill;
using AiForge.Application.Services;
using ModelContextProtocol.Server;

namespace AiForge.Mcp.Tools;

[McpServerToolType]
public class SkillTools
{
    private readonly ISkillService _skillService;
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public SkillTools(ISkillService skillService)
    {
        _skillService = skillService;
    }

    [McpServerTool(Name = "list_skills"), Description("List skills with optional organization, project, or category filters")]
    public async Task<string> ListSkills(
        [Description("Organization ID (GUID) to filter by")] string? organizationId = null,
        [Description("Project ID (GUID) to filter by")] string? projectId = null,
        [Description("Category filter: Workflow, Analysis, Documentation, Generation, Testing, Custom")] string? category = null,
        [Description("Only return published skills")] bool? publishedOnly = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var orgId = string.IsNullOrEmpty(organizationId) ? null : (Guid?)Guid.Parse(organizationId);
            var projId = string.IsNullOrEmpty(projectId) ? null : (Guid?)Guid.Parse(projectId);

            var response = await _skillService.GetSkillsAsync(orgId, projId, category, publishedOnly, cancellationToken);

            return JsonSerializer.Serialize(new
            {
                success = true,
                totalCount = response.TotalCount,
                skills = response.Items.Select(s => new
                {
                    s.Id,
                    s.SkillKey,
                    s.Name,
                    s.Description,
                    s.Category,
                    s.Scope,
                    s.IsPublished
                })
            }, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool(Name = "get_skill"), Description("Get detailed skill information by ID or key")]
    public async Task<string> GetSkill(
        [Description("Skill ID (GUID) or skill key")] string skillIdOrKey,
        [Description("Organization ID (GUID) - required when looking up by key")] string? organizationId = null,
        [Description("Project ID (GUID) - optional scope for key lookup (project-level takes precedence)")] string? projectId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            SkillDto? skill;

            if (Guid.TryParse(skillIdOrKey, out var id))
            {
                skill = await _skillService.GetSkillByIdAsync(id, cancellationToken);
            }
            else
            {
                if (string.IsNullOrEmpty(organizationId))
                    return JsonSerializer.Serialize(new { error = "organizationId is required when looking up by key" });

                var orgId = Guid.Parse(organizationId);
                var projId = string.IsNullOrEmpty(projectId) ? null : (Guid?)Guid.Parse(projectId);
                skill = await _skillService.GetSkillByKeyAsync(skillIdOrKey, projId, orgId, cancellationToken);
            }

            if (skill == null)
                return JsonSerializer.Serialize(new { error = $"Skill '{skillIdOrKey}' not found" });

            return JsonSerializer.Serialize(new
            {
                success = true,
                skill = new
                {
                    skill.Id,
                    skill.SkillKey,
                    skill.Name,
                    skill.Description,
                    skill.Content,
                    skill.Category,
                    skill.OrganizationId,
                    skill.ProjectId,
                    skill.Scope,
                    skill.IsPublished,
                    skill.CreatedAt,
                    skill.UpdatedAt
                }
            }, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool(Name = "create_skill"), Description("Create a new skill/slash command")]
    public async Task<string> CreateSkill(
        [Description("Unique skill key (e.g., 'code-review', 'commit-msg')")] string skillKey,
        [Description("Display name for the skill")] string name,
        [Description("Skill content/prompt (markdown)")] string content,
        [Description("Category: Workflow, Analysis, Documentation, Generation, Testing, Custom")] string category,
        [Description("Organization ID (GUID) for org-level skill")] string? organizationId = null,
        [Description("Project ID (GUID) for project-level skill")] string? projectId = null,
        [Description("Skill description")] string? description = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new CreateSkillRequest
            {
                SkillKey = skillKey,
                Name = name,
                Description = description,
                Content = content,
                Category = category,
                OrganizationId = string.IsNullOrEmpty(organizationId) ? null : Guid.Parse(organizationId),
                ProjectId = string.IsNullOrEmpty(projectId) ? null : Guid.Parse(projectId)
            };

            var skill = await _skillService.CreateSkillAsync(request, "mcp-session", cancellationToken);

            return JsonSerializer.Serialize(new
            {
                success = true,
                skill.Id,
                skill.SkillKey,
                skill.Name,
                skill.Scope,
                skill.IsPublished,
                message = $"Skill '{skill.Name}' created successfully"
            }, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool(Name = "update_skill"), Description("Update an existing skill's configuration")]
    public async Task<string> UpdateSkill(
        [Description("Skill ID (GUID)")] string skillId,
        [Description("New name (optional)")] string? name = null,
        [Description("New description (optional)")] string? description = null,
        [Description("New content (optional)")] string? content = null,
        [Description("New category (optional)")] string? category = null,
        [Description("Publish or unpublish the skill (optional)")] bool? isPublished = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var id = Guid.Parse(skillId);
            var request = new UpdateSkillRequest
            {
                Name = name,
                Description = description,
                Content = content,
                Category = category,
                IsPublished = isPublished
            };

            var skill = await _skillService.UpdateSkillAsync(id, request, "mcp-session", cancellationToken);
            if (skill == null)
                return JsonSerializer.Serialize(new { error = $"Skill '{skillId}' not found" });

            return JsonSerializer.Serialize(new
            {
                success = true,
                skill.Id,
                skill.SkillKey,
                skill.Name,
                skill.IsPublished,
                message = $"Skill '{skill.Name}' updated successfully"
            }, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool(Name = "publish_skill"), Description("Publish a skill to make it available for use")]
    public async Task<string> PublishSkill(
        [Description("Skill ID (GUID)")] string skillId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var id = Guid.Parse(skillId);
            var skill = await _skillService.PublishSkillAsync(id, "mcp-session", cancellationToken);

            if (skill == null)
                return JsonSerializer.Serialize(new { error = $"Skill '{skillId}' not found" });

            return JsonSerializer.Serialize(new
            {
                success = true,
                skill.Id,
                skill.SkillKey,
                skill.Name,
                skill.IsPublished,
                message = $"Skill '{skill.Name}' published successfully"
            }, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool(Name = "unpublish_skill"), Description("Unpublish a skill to hide it from use")]
    public async Task<string> UnpublishSkill(
        [Description("Skill ID (GUID)")] string skillId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var id = Guid.Parse(skillId);
            var skill = await _skillService.UnpublishSkillAsync(id, "mcp-session", cancellationToken);

            if (skill == null)
                return JsonSerializer.Serialize(new { error = $"Skill '{skillId}' not found" });

            return JsonSerializer.Serialize(new
            {
                success = true,
                skill.Id,
                skill.SkillKey,
                skill.Name,
                skill.IsPublished,
                message = $"Skill '{skill.Name}' unpublished successfully"
            }, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool(Name = "invoke_skill"), Description("Get skill content for invocation (returns the skill's prompt/content)")]
    public async Task<string> InvokeSkill(
        [Description("Skill key (e.g., 'code-review')")] string skillKey,
        [Description("Organization ID (GUID)")] string organizationId,
        [Description("Project ID (GUID) - optional, for project-level override")] string? projectId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var orgId = Guid.Parse(organizationId);
            var projId = string.IsNullOrEmpty(projectId) ? null : (Guid?)Guid.Parse(projectId);

            var skill = await _skillService.GetSkillByKeyAsync(skillKey, projId, orgId, cancellationToken);

            if (skill == null)
                return JsonSerializer.Serialize(new { error = $"Skill '{skillKey}' not found" });

            if (!skill.IsPublished)
                return JsonSerializer.Serialize(new { error = $"Skill '{skillKey}' is not published" });

            return JsonSerializer.Serialize(new
            {
                success = true,
                skill.SkillKey,
                skill.Name,
                skill.Content,
                skill.Category,
                skill.Scope,
                message = $"Skill '{skill.Name}' invoked - content returned"
            }, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }
}
