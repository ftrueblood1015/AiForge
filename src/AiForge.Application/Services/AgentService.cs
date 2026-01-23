using System.Text.Json;
using AiForge.Application.DTOs.Agent;
using AiForge.Domain.Entities;
using AiForge.Domain.Enums;
using AiForge.Domain.Interfaces;
using AiForge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AiForge.Application.Services;

public interface IAgentService
{
    Task<AgentDto> CreateAgentAsync(CreateAgentRequest request, string createdBy, CancellationToken cancellationToken = default);
    Task<AgentDto?> GetAgentByIdAsync(Guid agentId, CancellationToken cancellationToken = default);
    Task<AgentDto?> GetAgentByKeyAsync(string agentKey, Guid? projectId, Guid organizationId, CancellationToken cancellationToken = default);
    Task<AgentListResponse> GetAgentsAsync(Guid? organizationId, Guid? projectId, CancellationToken cancellationToken = default);
    Task<AgentDto?> UpdateAgentAsync(Guid agentId, UpdateAgentRequest request, string updatedBy, CancellationToken cancellationToken = default);
    Task<bool> DeleteAgentAsync(Guid agentId, CancellationToken cancellationToken = default);
}

public class AgentService : IAgentService
{
    private readonly AiForgeDbContext _context;
    private readonly IScopeResolver _scopeResolver;
    private readonly IUnitOfWork _unitOfWork;

    public AgentService(AiForgeDbContext context, IScopeResolver scopeResolver, IUnitOfWork unitOfWork)
    {
        _context = context;
        _scopeResolver = scopeResolver;
        _unitOfWork = unitOfWork;
    }

    public async Task<AgentDto> CreateAgentAsync(CreateAgentRequest request, string createdBy, CancellationToken cancellationToken = default)
    {
        // Validate scope: exactly one of OrganizationId or ProjectId must be set
        if ((request.OrganizationId.HasValue && request.ProjectId.HasValue) ||
            (!request.OrganizationId.HasValue && !request.ProjectId.HasValue))
        {
            throw new InvalidOperationException("Exactly one of OrganizationId or ProjectId must be specified");
        }

        // Check for duplicate key within scope
        var existingAgent = await _context.Agents
            .FirstOrDefaultAsync(a =>
                a.AgentKey == request.AgentKey &&
                ((request.OrganizationId.HasValue && a.OrganizationId == request.OrganizationId) ||
                 (request.ProjectId.HasValue && a.ProjectId == request.ProjectId)),
                cancellationToken);

        if (existingAgent != null)
        {
            throw new InvalidOperationException($"Agent with key '{request.AgentKey}' already exists in this scope");
        }

        var agent = new Agent
        {
            Id = Guid.NewGuid(),
            AgentKey = request.AgentKey,
            Name = request.Name,
            Description = request.Description,
            SystemPrompt = request.SystemPrompt,
            Instructions = request.Instructions,
            AgentType = Enum.Parse<AgentType>(request.AgentType, ignoreCase: true),
            Capabilities = request.Capabilities != null ? JsonSerializer.Serialize(request.Capabilities) : null,
            Status = AgentStatus.Idle,
            OrganizationId = request.OrganizationId,
            ProjectId = request.ProjectId,
            IsEnabled = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };

        _context.Agents.Add(agent);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(agent);
    }

    public async Task<AgentDto?> GetAgentByIdAsync(Guid agentId, CancellationToken cancellationToken = default)
    {
        var agent = await _context.Agents
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == agentId, cancellationToken);

        return agent != null ? MapToDto(agent) : null;
    }

    public async Task<AgentDto?> GetAgentByKeyAsync(string agentKey, Guid? projectId, Guid organizationId, CancellationToken cancellationToken = default)
    {
        var agent = await _scopeResolver.ResolveAgentAsync(agentKey, projectId, organizationId, cancellationToken);
        return agent != null ? MapToDto(agent) : null;
    }

    public async Task<AgentListResponse> GetAgentsAsync(Guid? organizationId, Guid? projectId, CancellationToken cancellationToken = default)
    {
        IQueryable<Agent> query = _context.Agents.AsNoTracking();

        if (organizationId.HasValue && projectId.HasValue)
        {
            // Get both org-level and project-level agents
            query = query.Where(a => a.OrganizationId == organizationId || a.ProjectId == projectId);
        }
        else if (organizationId.HasValue)
        {
            query = query.Where(a => a.OrganizationId == organizationId);
        }
        else if (projectId.HasValue)
        {
            query = query.Where(a => a.ProjectId == projectId);
        }

        var agents = await query.OrderBy(a => a.Name).ToListAsync(cancellationToken);

        return new AgentListResponse
        {
            Items = agents.Select(MapToListItemDto).ToList(),
            TotalCount = agents.Count
        };
    }

    public async Task<AgentDto?> UpdateAgentAsync(Guid agentId, UpdateAgentRequest request, string updatedBy, CancellationToken cancellationToken = default)
    {
        var agent = await _context.Agents.FirstOrDefaultAsync(a => a.Id == agentId, cancellationToken);
        if (agent == null)
            return null;

        if (!string.IsNullOrEmpty(request.Name))
            agent.Name = request.Name;

        if (request.Description != null)
            agent.Description = request.Description;

        if (request.SystemPrompt != null)
            agent.SystemPrompt = request.SystemPrompt;

        if (request.Instructions != null)
            agent.Instructions = request.Instructions;

        if (!string.IsNullOrEmpty(request.AgentType))
            agent.AgentType = Enum.Parse<AgentType>(request.AgentType, ignoreCase: true);

        if (request.Capabilities != null)
            agent.Capabilities = JsonSerializer.Serialize(request.Capabilities);

        if (!string.IsNullOrEmpty(request.Status))
            agent.Status = Enum.Parse<AgentStatus>(request.Status, ignoreCase: true);

        if (request.IsEnabled.HasValue)
            agent.IsEnabled = request.IsEnabled.Value;

        agent.UpdatedAt = DateTime.UtcNow;
        agent.UpdatedBy = updatedBy;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(agent);
    }

    public async Task<bool> DeleteAgentAsync(Guid agentId, CancellationToken cancellationToken = default)
    {
        var agent = await _context.Agents.FirstOrDefaultAsync(a => a.Id == agentId, cancellationToken);
        if (agent == null)
            return false;

        _context.Agents.Remove(agent);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static AgentDto MapToDto(Agent agent)
    {
        return new AgentDto
        {
            Id = agent.Id,
            AgentKey = agent.AgentKey,
            Name = agent.Name,
            Description = agent.Description,
            SystemPrompt = agent.SystemPrompt,
            Instructions = agent.Instructions,
            AgentType = agent.AgentType.ToString(),
            Capabilities = ParseCapabilities(agent.Capabilities),
            Status = agent.Status.ToString(),
            OrganizationId = agent.OrganizationId,
            ProjectId = agent.ProjectId,
            Scope = agent.OrganizationId.HasValue ? "Organization" : "Project",
            IsEnabled = agent.IsEnabled,
            CreatedAt = agent.CreatedAt,
            CreatedBy = agent.CreatedBy,
            UpdatedAt = agent.UpdatedAt,
            UpdatedBy = agent.UpdatedBy
        };
    }

    private static AgentListItemDto MapToListItemDto(Agent agent)
    {
        return new AgentListItemDto
        {
            Id = agent.Id,
            AgentKey = agent.AgentKey,
            Name = agent.Name,
            Description = agent.Description,
            AgentType = agent.AgentType.ToString(),
            Status = agent.Status.ToString(),
            Scope = agent.OrganizationId.HasValue ? "Organization" : "Project",
            IsEnabled = agent.IsEnabled
        };
    }

    private static List<string> ParseCapabilities(string? capabilitiesJson)
    {
        if (string.IsNullOrEmpty(capabilitiesJson))
            return new List<string>();

        try
        {
            return JsonSerializer.Deserialize<List<string>>(capabilitiesJson) ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }
}
