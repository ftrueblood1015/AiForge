using System.ComponentModel;
using System.Text.Json;
using AiForge.Application.DTOs.Agent;
using AiForge.Application.Services;
using ModelContextProtocol.Server;

namespace AiForge.Mcp.Tools;

[McpServerToolType]
public class AgentTools
{
    private readonly IAgentService _agentService;
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public AgentTools(IAgentService agentService)
    {
        _agentService = agentService;
    }

    [McpServerTool(Name = "list_agents"), Description("List agents with optional organization, project, or other filters")]
    public async Task<string> ListAgents(
        [Description("Organization ID (GUID) to filter by")] string? organizationId = null,
        [Description("Project ID (GUID) to filter by")] string? projectId = null,
        [Description("Agent type filter: Claude, GPT, Gemini, Custom")] string? agentType = null,
        [Description("Status filter: Idle, Working, Paused, Disabled, Error")] string? status = null,
        [Description("Filter by enabled status")] bool? isEnabled = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var orgId = string.IsNullOrEmpty(organizationId) ? null : (Guid?)Guid.Parse(organizationId);
            var projId = string.IsNullOrEmpty(projectId) ? null : (Guid?)Guid.Parse(projectId);

            var response = await _agentService.GetAgentsAsync(orgId, projId, agentType, status, isEnabled, cancellationToken);

            return JsonSerializer.Serialize(new
            {
                success = true,
                totalCount = response.TotalCount,
                agents = response.Items.Select(a => new
                {
                    a.Id,
                    a.AgentKey,
                    a.Name,
                    a.Description,
                    a.AgentType,
                    a.Status,
                    a.Scope,
                    a.IsEnabled
                })
            }, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool(Name = "get_agent"), Description("Get detailed agent information by ID or key")]
    public async Task<string> GetAgent(
        [Description("Agent ID (GUID) or agent key")] string agentIdOrKey,
        [Description("Organization ID (GUID) - required when looking up by key")] string? organizationId = null,
        [Description("Project ID (GUID) - optional scope for key lookup")] string? projectId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            AgentDto? agent;

            if (Guid.TryParse(agentIdOrKey, out var id))
            {
                agent = await _agentService.GetAgentByIdAsync(id, cancellationToken);
            }
            else
            {
                if (string.IsNullOrEmpty(organizationId))
                    return JsonSerializer.Serialize(new { error = "organizationId is required when looking up by key" });

                var orgId = Guid.Parse(organizationId);
                var projId = string.IsNullOrEmpty(projectId) ? null : (Guid?)Guid.Parse(projectId);
                agent = await _agentService.GetAgentByKeyAsync(agentIdOrKey, projId, orgId, cancellationToken);
            }

            if (agent == null)
                return JsonSerializer.Serialize(new { error = $"Agent '{agentIdOrKey}' not found" });

            return JsonSerializer.Serialize(new
            {
                success = true,
                agent = new
                {
                    agent.Id,
                    agent.AgentKey,
                    agent.Name,
                    agent.Description,
                    agent.SystemPrompt,
                    agent.Instructions,
                    agent.AgentType,
                    agent.Capabilities,
                    agent.Status,
                    agent.OrganizationId,
                    agent.ProjectId,
                    agent.Scope,
                    agent.IsEnabled,
                    agent.CreatedAt,
                    agent.UpdatedAt
                }
            }, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool(Name = "create_agent"), Description("Create a new agent configuration")]
    public async Task<string> CreateAgent(
        [Description("Unique agent key (e.g., 'code-reviewer', 'planner')")] string agentKey,
        [Description("Display name for the agent")] string name,
        [Description("Agent type: Claude, GPT, Gemini, Custom")] string agentType,
        [Description("Organization ID (GUID) for org-level agent")] string? organizationId = null,
        [Description("Project ID (GUID) for project-level agent")] string? projectId = null,
        [Description("Agent description")] string? description = null,
        [Description("System prompt for the agent")] string? systemPrompt = null,
        [Description("Detailed instructions (markdown)")] string? instructions = null,
        [Description("Capabilities as comma-separated list (e.g., 'coding,review,planning')")] string? capabilities = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new CreateAgentRequest
            {
                AgentKey = agentKey,
                Name = name,
                Description = description,
                SystemPrompt = systemPrompt,
                Instructions = instructions,
                AgentType = agentType,
                Capabilities = string.IsNullOrEmpty(capabilities)
                    ? null
                    : capabilities.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList(),
                OrganizationId = string.IsNullOrEmpty(organizationId) ? null : Guid.Parse(organizationId),
                ProjectId = string.IsNullOrEmpty(projectId) ? null : Guid.Parse(projectId)
            };

            var agent = await _agentService.CreateAgentAsync(request, "mcp-session", cancellationToken);

            return JsonSerializer.Serialize(new
            {
                success = true,
                agent.Id,
                agent.AgentKey,
                agent.Name,
                agent.Scope,
                message = $"Agent '{agent.Name}' created successfully"
            }, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool(Name = "update_agent"), Description("Update an existing agent's configuration")]
    public async Task<string> UpdateAgent(
        [Description("Agent ID (GUID)")] string agentId,
        [Description("New name (optional)")] string? name = null,
        [Description("New description (optional)")] string? description = null,
        [Description("New system prompt (optional)")] string? systemPrompt = null,
        [Description("New instructions (optional)")] string? instructions = null,
        [Description("New agent type (optional)")] string? agentType = null,
        [Description("New capabilities as comma-separated list (optional)")] string? capabilities = null,
        [Description("New status: Idle, Working, Paused, Disabled, Error (optional)")] string? status = null,
        [Description("Enable or disable the agent (optional)")] bool? isEnabled = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var id = Guid.Parse(agentId);
            var request = new UpdateAgentRequest
            {
                Name = name,
                Description = description,
                SystemPrompt = systemPrompt,
                Instructions = instructions,
                AgentType = agentType,
                Capabilities = string.IsNullOrEmpty(capabilities)
                    ? null
                    : capabilities.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList(),
                Status = status,
                IsEnabled = isEnabled
            };

            var agent = await _agentService.UpdateAgentAsync(id, request, "mcp-session", cancellationToken);
            if (agent == null)
                return JsonSerializer.Serialize(new { error = $"Agent '{agentId}' not found" });

            return JsonSerializer.Serialize(new
            {
                success = true,
                agent.Id,
                agent.AgentKey,
                agent.Name,
                agent.Status,
                agent.IsEnabled,
                message = $"Agent '{agent.Name}' updated successfully"
            }, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }
}
