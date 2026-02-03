using AiForge.Application.DTOs.SkillChains;
using AiForge.Application.Extensions;
using AiForge.Application.Interfaces;
using AiForge.Domain.Entities;
using AiForge.Domain.Enums;
using AiForge.Domain.Interfaces;
using AiForge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AiForge.Application.Services;

public interface ISkillChainService
{
    // Chain CRUD
    Task<SkillChainDto> CreateAsync(CreateSkillChainRequest request, string createdBy, CancellationToken ct = default);
    Task<SkillChainDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<SkillChainDto?> GetByKeyAsync(string chainKey, Guid organizationId, Guid? projectId, CancellationToken ct = default);
    Task<IEnumerable<SkillChainSummaryDto>> GetChainsAsync(Guid? organizationId, Guid? projectId, bool? publishedOnly, CancellationToken ct = default);
    Task<SkillChainDto?> UpdateAsync(Guid id, UpdateSkillChainRequest request, string updatedBy, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
    Task<SkillChainDto?> PublishAsync(Guid id, string updatedBy, CancellationToken ct = default);
    Task<SkillChainDto?> UnpublishAsync(Guid id, string updatedBy, CancellationToken ct = default);

    // Link management
    Task<SkillChainLinkDto> AddLinkAsync(Guid chainId, CreateSkillChainLinkRequest request, CancellationToken ct = default);
    Task<SkillChainLinkDto?> UpdateLinkAsync(Guid linkId, UpdateSkillChainLinkRequest request, CancellationToken ct = default);
    Task<bool> RemoveLinkAsync(Guid linkId, CancellationToken ct = default);
    Task<bool> ReorderLinksAsync(Guid chainId, List<Guid> linkIdsInOrder, CancellationToken ct = default);
}

public class SkillChainService : ISkillChainService
{
    private readonly AiForgeDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserContext _userContext;
    private readonly IProjectMemberService _projectMemberService;

    public SkillChainService(
        AiForgeDbContext context,
        IUnitOfWork unitOfWork,
        IUserContext userContext,
        IProjectMemberService projectMemberService)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _userContext = userContext;
        _projectMemberService = projectMemberService;
    }

    public async Task<SkillChainDto> CreateAsync(CreateSkillChainRequest request, string createdBy, CancellationToken ct = default)
    {
        // Validate scope
        if ((request.OrganizationId.HasValue && request.ProjectId.HasValue) ||
            (!request.OrganizationId.HasValue && !request.ProjectId.HasValue))
        {
            throw new InvalidOperationException("Exactly one of OrganizationId or ProjectId must be specified");
        }

        // Check for duplicate key
        var existingChain = await _context.SkillChains
            .FirstOrDefaultAsync(sc =>
                sc.ChainKey == request.ChainKey &&
                ((request.OrganizationId.HasValue && sc.OrganizationId == request.OrganizationId) ||
                 (request.ProjectId.HasValue && sc.ProjectId == request.ProjectId)),
                ct);

        if (existingChain != null)
        {
            throw new InvalidOperationException($"Skill chain with key '{request.ChainKey}' already exists in this scope");
        }

        var chain = new SkillChain
        {
            Id = Guid.NewGuid(),
            ChainKey = request.ChainKey,
            Name = request.Name,
            Description = request.Description,
            InputSchema = request.InputSchema,
            MaxTotalFailures = request.MaxTotalFailures,
            OrganizationId = request.OrganizationId,
            ProjectId = request.ProjectId,
            IsPublished = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy,
            UpdatedAt = DateTime.UtcNow
        };

        _context.SkillChains.Add(chain);
        await _unitOfWork.SaveChangesAsync(ct);

        return await GetByIdAsync(chain.Id, ct) ?? throw new InvalidOperationException("Failed to retrieve created chain");
    }

    public async Task<SkillChainDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var chain = await _context.SkillChains
            .AsNoTracking()
            .Include(sc => sc.Links.OrderBy(l => l.Position))
                .ThenInclude(l => l.Skill)
            .Include(sc => sc.Links)
                .ThenInclude(l => l.Agent)
            .FirstOrDefaultAsync(sc => sc.Id == id, ct);

        if (chain == null) return null;

        // Check project access (org-level chains are visible to all)
        var accessibleProjects = await _projectMemberService.GetAccessibleProjectIdsOrNullAsync(_userContext, ct);
        if (accessibleProjects != null && chain.ProjectId.HasValue && !accessibleProjects.Contains(chain.ProjectId.Value))
            return null;

        return MapToDto(chain);
    }

    public async Task<SkillChainDto?> GetByKeyAsync(string chainKey, Guid organizationId, Guid? projectId, CancellationToken ct = default)
    {
        // Try project-level first, then org-level
        SkillChain? chain = null;

        if (projectId.HasValue)
        {
            chain = await _context.SkillChains
                .AsNoTracking()
                .Include(sc => sc.Links.OrderBy(l => l.Position))
                    .ThenInclude(l => l.Skill)
                .Include(sc => sc.Links)
                    .ThenInclude(l => l.Agent)
                .FirstOrDefaultAsync(sc => sc.ChainKey == chainKey && sc.ProjectId == projectId, ct);
        }

        chain ??= await _context.SkillChains
            .AsNoTracking()
            .Include(sc => sc.Links.OrderBy(l => l.Position))
                .ThenInclude(l => l.Skill)
            .Include(sc => sc.Links)
                .ThenInclude(l => l.Agent)
            .FirstOrDefaultAsync(sc => sc.ChainKey == chainKey && sc.OrganizationId == organizationId, ct);

        return chain != null ? MapToDto(chain) : null;
    }

    public async Task<IEnumerable<SkillChainSummaryDto>> GetChainsAsync(Guid? organizationId, Guid? projectId, bool? publishedOnly, CancellationToken ct = default)
    {
        var accessibleProjects = await _projectMemberService.GetAccessibleProjectIdsOrNullAsync(_userContext, ct);

        IQueryable<SkillChain> query = _context.SkillChains.AsNoTracking();

        // Apply project access filtering
        if (accessibleProjects != null)
        {
            // User can see:
            // 1. Organization-level chains (ProjectId is null)
            // 2. Project-level chains for their accessible projects
            query = query.Where(sc => sc.ProjectId == null || accessibleProjects.Contains(sc.ProjectId.Value));
        }

        if (organizationId.HasValue && projectId.HasValue)
        {
            query = query.Where(sc => sc.OrganizationId == organizationId || sc.ProjectId == projectId);
        }
        else if (organizationId.HasValue)
        {
            query = query.Where(sc => sc.OrganizationId == organizationId);
        }
        else if (projectId.HasValue)
        {
            query = query.Where(sc => sc.ProjectId == projectId);
        }

        if (publishedOnly == true)
        {
            query = query.Where(sc => sc.IsPublished);
        }

        var chains = await query
            .Include(sc => sc.Project)
            .Include(sc => sc.Links)
            .Include(sc => sc.Executions)
            .OrderBy(sc => sc.Name)
            .ToListAsync(ct);

        return chains.Select(MapToSummaryDto);
    }

    public async Task<SkillChainDto?> UpdateAsync(Guid id, UpdateSkillChainRequest request, string updatedBy, CancellationToken ct = default)
    {
        var chain = await _context.SkillChains.FirstOrDefaultAsync(sc => sc.Id == id, ct);
        if (chain == null) return null;

        if (!string.IsNullOrEmpty(request.Name))
            chain.Name = request.Name;

        if (request.Description != null)
            chain.Description = request.Description;

        if (request.InputSchema != null)
            chain.InputSchema = request.InputSchema;

        if (request.MaxTotalFailures.HasValue)
            chain.MaxTotalFailures = request.MaxTotalFailures.Value;

        if (request.IsPublished.HasValue)
            chain.IsPublished = request.IsPublished.Value;

        chain.UpdatedAt = DateTime.UtcNow;
        chain.UpdatedBy = updatedBy;

        await _unitOfWork.SaveChangesAsync(ct);

        return await GetByIdAsync(id, ct);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var chain = await _context.SkillChains
            .Include(sc => sc.Executions)
            .FirstOrDefaultAsync(sc => sc.Id == id, ct);

        if (chain == null) return false;

        if (chain.Executions.Any(e => e.Status == ChainExecutionStatus.Running))
        {
            throw new InvalidOperationException("Cannot delete chain with running executions");
        }

        _context.SkillChains.Remove(chain);
        await _unitOfWork.SaveChangesAsync(ct);

        return true;
    }

    public async Task<SkillChainDto?> PublishAsync(Guid id, string updatedBy, CancellationToken ct = default)
    {
        var chain = await _context.SkillChains
            .Include(sc => sc.Links)
            .FirstOrDefaultAsync(sc => sc.Id == id, ct);

        if (chain == null) return null;

        if (!chain.Links.Any())
        {
            throw new InvalidOperationException("Cannot publish a chain with no links");
        }

        chain.IsPublished = true;
        chain.UpdatedAt = DateTime.UtcNow;
        chain.UpdatedBy = updatedBy;

        await _unitOfWork.SaveChangesAsync(ct);

        return await GetByIdAsync(id, ct);
    }

    public async Task<SkillChainDto?> UnpublishAsync(Guid id, string updatedBy, CancellationToken ct = default)
    {
        var chain = await _context.SkillChains.FirstOrDefaultAsync(sc => sc.Id == id, ct);
        if (chain == null) return null;

        chain.IsPublished = false;
        chain.UpdatedAt = DateTime.UtcNow;
        chain.UpdatedBy = updatedBy;

        await _unitOfWork.SaveChangesAsync(ct);

        return await GetByIdAsync(id, ct);
    }

    public async Task<SkillChainLinkDto> AddLinkAsync(Guid chainId, CreateSkillChainLinkRequest request, CancellationToken ct = default)
    {
        var chain = await _context.SkillChains
            .Include(sc => sc.Links)
            .FirstOrDefaultAsync(sc => sc.Id == chainId, ct);

        if (chain == null)
            throw new InvalidOperationException($"Chain with ID '{chainId}' not found");

        // Validate skill exists
        var skillExists = await _context.Skills.AnyAsync(s => s.Id == request.SkillId, ct);
        if (!skillExists)
            throw new InvalidOperationException($"Skill with ID '{request.SkillId}' not found");

        // Validate agent exists if specified
        if (request.AgentId.HasValue)
        {
            var agentExists = await _context.Agents.AnyAsync(a => a.Id == request.AgentId, ct);
            if (!agentExists)
                throw new InvalidOperationException($"Agent with ID '{request.AgentId}' not found");
        }

        // Determine position
        int position = request.Position ?? chain.Links.Count;

        // If inserting at a specific position, shift other links
        if (request.Position.HasValue && request.Position.Value < chain.Links.Count)
        {
            foreach (var existingLink in chain.Links.Where(l => l.Position >= request.Position.Value))
            {
                existingLink.Position++;
            }
        }

        var link = new SkillChainLink
        {
            Id = Guid.NewGuid(),
            SkillChainId = chainId,
            Position = position,
            Name = request.Name,
            Description = request.Description,
            SkillId = request.SkillId,
            AgentId = request.AgentId,
            MaxRetries = request.MaxRetries,
            OnSuccessTransition = Enum.Parse<TransitionType>(request.OnSuccessTransition, ignoreCase: true),
            OnSuccessTargetLinkId = request.OnSuccessTargetLinkId,
            OnFailureTransition = Enum.Parse<TransitionType>(request.OnFailureTransition, ignoreCase: true),
            OnFailureTargetLinkId = request.OnFailureTargetLinkId,
            LinkConfig = request.LinkConfig
        };

        _context.SkillChainLinks.Add(link);
        chain.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(ct);

        // Reload with navigation properties
        var savedLink = await _context.SkillChainLinks
            .AsNoTracking()
            .Include(l => l.Skill)
            .Include(l => l.Agent)
            .FirstOrDefaultAsync(l => l.Id == link.Id, ct);

        return MapToLinkDto(savedLink!);
    }

    public async Task<SkillChainLinkDto?> UpdateLinkAsync(Guid linkId, UpdateSkillChainLinkRequest request, CancellationToken ct = default)
    {
        var link = await _context.SkillChainLinks
            .Include(l => l.SkillChain)
            .FirstOrDefaultAsync(l => l.Id == linkId, ct);

        if (link == null) return null;

        if (!string.IsNullOrEmpty(request.Name))
            link.Name = request.Name;

        if (request.Description != null)
            link.Description = request.Description;

        if (request.SkillId.HasValue)
        {
            var skillExists = await _context.Skills.AnyAsync(s => s.Id == request.SkillId, ct);
            if (!skillExists)
                throw new InvalidOperationException($"Skill with ID '{request.SkillId}' not found");
            link.SkillId = request.SkillId.Value;
        }

        if (request.AgentId.HasValue)
        {
            var agentExists = await _context.Agents.AnyAsync(a => a.Id == request.AgentId, ct);
            if (!agentExists)
                throw new InvalidOperationException($"Agent with ID '{request.AgentId}' not found");
            link.AgentId = request.AgentId.Value;
        }

        if (request.MaxRetries.HasValue)
            link.MaxRetries = request.MaxRetries.Value;

        if (!string.IsNullOrEmpty(request.OnSuccessTransition))
            link.OnSuccessTransition = Enum.Parse<TransitionType>(request.OnSuccessTransition, ignoreCase: true);

        if (request.OnSuccessTargetLinkId.HasValue)
            link.OnSuccessTargetLinkId = request.OnSuccessTargetLinkId.Value;

        if (!string.IsNullOrEmpty(request.OnFailureTransition))
            link.OnFailureTransition = Enum.Parse<TransitionType>(request.OnFailureTransition, ignoreCase: true);

        if (request.OnFailureTargetLinkId.HasValue)
            link.OnFailureTargetLinkId = request.OnFailureTargetLinkId.Value;

        if (request.LinkConfig != null)
            link.LinkConfig = request.LinkConfig;

        link.SkillChain.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(ct);

        // Reload with navigation properties
        var savedLink = await _context.SkillChainLinks
            .AsNoTracking()
            .Include(l => l.Skill)
            .Include(l => l.Agent)
            .FirstOrDefaultAsync(l => l.Id == linkId, ct);

        return MapToLinkDto(savedLink!);
    }

    public async Task<bool> RemoveLinkAsync(Guid linkId, CancellationToken ct = default)
    {
        var link = await _context.SkillChainLinks
            .Include(l => l.SkillChain)
                .ThenInclude(sc => sc.Links)
            .FirstOrDefaultAsync(l => l.Id == linkId, ct);

        if (link == null) return false;

        var chain = link.SkillChain;
        var removedPosition = link.Position;

        // Shift positions of links after the removed one
        foreach (var otherLink in chain.Links.Where(l => l.Position > removedPosition))
        {
            otherLink.Position--;
        }

        _context.SkillChainLinks.Remove(link);
        chain.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(ct);

        return true;
    }

    public async Task<bool> ReorderLinksAsync(Guid chainId, List<Guid> linkIdsInOrder, CancellationToken ct = default)
    {
        var chain = await _context.SkillChains
            .Include(sc => sc.Links)
            .FirstOrDefaultAsync(sc => sc.Id == chainId, ct);

        if (chain == null) return false;

        if (linkIdsInOrder.Count != chain.Links.Count)
            throw new InvalidOperationException("Link IDs count must match the number of links in the chain");

        var linkDict = chain.Links.ToDictionary(l => l.Id);

        for (int i = 0; i < linkIdsInOrder.Count; i++)
        {
            if (!linkDict.TryGetValue(linkIdsInOrder[i], out var link))
                throw new InvalidOperationException($"Link with ID '{linkIdsInOrder[i]}' not found in chain");

            link.Position = i;
        }

        chain.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);

        return true;
    }

    private static SkillChainDto MapToDto(SkillChain chain)
    {
        return new SkillChainDto
        {
            Id = chain.Id,
            ChainKey = chain.ChainKey,
            Name = chain.Name,
            Description = chain.Description,
            InputSchema = chain.InputSchema,
            MaxTotalFailures = chain.MaxTotalFailures,
            OrganizationId = chain.OrganizationId,
            ProjectId = chain.ProjectId,
            Scope = chain.OrganizationId.HasValue ? "Organization" : "Project",
            IsPublished = chain.IsPublished,
            Links = chain.Links.OrderBy(l => l.Position).Select(MapToLinkDto).ToList(),
            CreatedAt = chain.CreatedAt,
            CreatedBy = chain.CreatedBy,
            UpdatedAt = chain.UpdatedAt,
            UpdatedBy = chain.UpdatedBy
        };
    }

    private static SkillChainSummaryDto MapToSummaryDto(SkillChain chain)
    {
        return new SkillChainSummaryDto
        {
            Id = chain.Id,
            ChainKey = chain.ChainKey,
            Name = chain.Name,
            Description = chain.Description,
            Scope = chain.OrganizationId.HasValue ? "Organization" : "Project",
            ProjectName = chain.Project?.Name,
            IsPublished = chain.IsPublished,
            LinkCount = chain.Links.Count,
            ExecutionCount = chain.Executions.Count,
            CreatedAt = chain.CreatedAt
        };
    }

    private static SkillChainLinkDto MapToLinkDto(SkillChainLink link)
    {
        return new SkillChainLinkDto
        {
            Id = link.Id,
            SkillChainId = link.SkillChainId,
            Position = link.Position,
            Name = link.Name,
            Description = link.Description,
            SkillId = link.SkillId,
            SkillName = link.Skill?.Name,
            SkillKey = link.Skill?.SkillKey,
            AgentId = link.AgentId,
            AgentName = link.Agent?.Name,
            AgentKey = link.Agent?.AgentKey,
            MaxRetries = link.MaxRetries,
            OnSuccessTransition = link.OnSuccessTransition.ToString(),
            OnSuccessTargetLinkId = link.OnSuccessTargetLinkId,
            OnFailureTransition = link.OnFailureTransition.ToString(),
            OnFailureTargetLinkId = link.OnFailureTargetLinkId,
            LinkConfig = link.LinkConfig
        };
    }
}
