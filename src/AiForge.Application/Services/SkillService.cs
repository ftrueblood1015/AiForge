using AiForge.Application.DTOs.Skill;
using AiForge.Application.Extensions;
using AiForge.Application.Interfaces;
using AiForge.Domain.Entities;
using AiForge.Domain.Enums;
using AiForge.Domain.Interfaces;
using AiForge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AiForge.Application.Services;

public interface ISkillService
{
    Task<SkillDto> CreateSkillAsync(CreateSkillRequest request, string createdBy, CancellationToken cancellationToken = default);
    Task<SkillDto?> GetSkillByIdAsync(Guid skillId, CancellationToken cancellationToken = default);
    Task<SkillDto?> GetSkillByKeyAsync(string skillKey, Guid? projectId, Guid organizationId, CancellationToken cancellationToken = default);
    Task<SkillListResponse> GetSkillsAsync(Guid? organizationId, Guid? projectId, string? category, bool? publishedOnly, CancellationToken cancellationToken = default);
    Task<SkillDto?> UpdateSkillAsync(Guid skillId, UpdateSkillRequest request, string updatedBy, CancellationToken cancellationToken = default);
    Task<SkillDto?> PublishSkillAsync(Guid skillId, string updatedBy, CancellationToken cancellationToken = default);
    Task<SkillDto?> UnpublishSkillAsync(Guid skillId, string updatedBy, CancellationToken cancellationToken = default);
    Task<bool> DeleteSkillAsync(Guid skillId, CancellationToken cancellationToken = default);
}

public class SkillService : ISkillService
{
    private readonly AiForgeDbContext _context;
    private readonly IScopeResolver _scopeResolver;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserContext _userContext;
    private readonly IProjectMemberService _projectMemberService;

    public SkillService(
        AiForgeDbContext context,
        IScopeResolver scopeResolver,
        IUnitOfWork unitOfWork,
        IUserContext userContext,
        IProjectMemberService projectMemberService)
    {
        _context = context;
        _scopeResolver = scopeResolver;
        _unitOfWork = unitOfWork;
        _userContext = userContext;
        _projectMemberService = projectMemberService;
    }

    public async Task<SkillDto> CreateSkillAsync(CreateSkillRequest request, string createdBy, CancellationToken cancellationToken = default)
    {
        // Validate scope: exactly one of OrganizationId or ProjectId must be set
        if ((request.OrganizationId.HasValue && request.ProjectId.HasValue) ||
            (!request.OrganizationId.HasValue && !request.ProjectId.HasValue))
        {
            throw new InvalidOperationException("Exactly one of OrganizationId or ProjectId must be specified");
        }

        // Check for duplicate key within scope
        var existingSkill = await _context.Skills
            .FirstOrDefaultAsync(s =>
                s.SkillKey == request.SkillKey &&
                ((request.OrganizationId.HasValue && s.OrganizationId == request.OrganizationId) ||
                 (request.ProjectId.HasValue && s.ProjectId == request.ProjectId)),
                cancellationToken);

        if (existingSkill != null)
        {
            throw new InvalidOperationException($"Skill with key '{request.SkillKey}' already exists in this scope");
        }

        var skill = new Skill
        {
            Id = Guid.NewGuid(),
            SkillKey = request.SkillKey,
            Name = request.Name,
            Description = request.Description,
            Content = request.Content,
            Category = Enum.Parse<SkillCategory>(request.Category, ignoreCase: true),
            OrganizationId = request.OrganizationId,
            ProjectId = request.ProjectId,
            IsPublished = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };

        _context.Skills.Add(skill);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(skill);
    }

    public async Task<SkillDto?> GetSkillByIdAsync(Guid skillId, CancellationToken cancellationToken = default)
    {
        var skill = await _context.Skills
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == skillId, cancellationToken);

        if (skill == null) return null;

        // Check project access (org-level skills are visible to all)
        var accessibleProjects = await _projectMemberService.GetAccessibleProjectIdsOrNullAsync(_userContext, cancellationToken);
        if (accessibleProjects != null && skill.ProjectId.HasValue && !accessibleProjects.Contains(skill.ProjectId.Value))
            return null;

        return MapToDto(skill);
    }

    public async Task<SkillDto?> GetSkillByKeyAsync(string skillKey, Guid? projectId, Guid organizationId, CancellationToken cancellationToken = default)
    {
        var skill = await _scopeResolver.ResolveSkillAsync(skillKey, projectId, organizationId, cancellationToken);
        return skill != null ? MapToDto(skill) : null;
    }

    public async Task<SkillListResponse> GetSkillsAsync(Guid? organizationId, Guid? projectId, string? category, bool? publishedOnly, CancellationToken cancellationToken = default)
    {
        var accessibleProjects = await _projectMemberService.GetAccessibleProjectIdsOrNullAsync(_userContext, cancellationToken);

        IQueryable<Skill> query = _context.Skills
            .Include(s => s.Project)
            .AsNoTracking();

        // Apply project access filtering
        if (accessibleProjects != null)
        {
            // User can see:
            // 1. Organization-level skills (ProjectId is null)
            // 2. Project-level skills for their accessible projects
            query = query.Where(s => s.ProjectId == null || accessibleProjects.Contains(s.ProjectId.Value));
        }

        if (organizationId.HasValue && projectId.HasValue)
        {
            query = query.Where(s => s.OrganizationId == organizationId || s.ProjectId == projectId);
        }
        else if (organizationId.HasValue)
        {
            query = query.Where(s => s.OrganizationId == organizationId);
        }
        else if (projectId.HasValue)
        {
            query = query.Where(s => s.ProjectId == projectId);
        }

        if (!string.IsNullOrEmpty(category))
        {
            var categoryEnum = Enum.Parse<SkillCategory>(category, ignoreCase: true);
            query = query.Where(s => s.Category == categoryEnum);
        }

        if (publishedOnly == true)
        {
            query = query.Where(s => s.IsPublished);
        }

        var skills = await query.OrderBy(s => s.Name).ToListAsync(cancellationToken);

        return new SkillListResponse
        {
            Items = skills.Select(MapToListItemDto).ToList(),
            TotalCount = skills.Count
        };
    }

    public async Task<SkillDto?> UpdateSkillAsync(Guid skillId, UpdateSkillRequest request, string updatedBy, CancellationToken cancellationToken = default)
    {
        var skill = await _context.Skills.FirstOrDefaultAsync(s => s.Id == skillId, cancellationToken);
        if (skill == null)
            return null;

        if (!string.IsNullOrEmpty(request.Name))
            skill.Name = request.Name;

        if (request.Description != null)
            skill.Description = request.Description;

        if (request.Content != null)
            skill.Content = request.Content;

        if (!string.IsNullOrEmpty(request.Category))
            skill.Category = Enum.Parse<SkillCategory>(request.Category, ignoreCase: true);

        if (request.IsPublished.HasValue)
            skill.IsPublished = request.IsPublished.Value;

        skill.UpdatedAt = DateTime.UtcNow;
        skill.UpdatedBy = updatedBy;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(skill);
    }

    public async Task<SkillDto?> PublishSkillAsync(Guid skillId, string updatedBy, CancellationToken cancellationToken = default)
    {
        var skill = await _context.Skills.FirstOrDefaultAsync(s => s.Id == skillId, cancellationToken);
        if (skill == null)
            return null;

        skill.IsPublished = true;
        skill.UpdatedAt = DateTime.UtcNow;
        skill.UpdatedBy = updatedBy;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(skill);
    }

    public async Task<SkillDto?> UnpublishSkillAsync(Guid skillId, string updatedBy, CancellationToken cancellationToken = default)
    {
        var skill = await _context.Skills.FirstOrDefaultAsync(s => s.Id == skillId, cancellationToken);
        if (skill == null)
            return null;

        skill.IsPublished = false;
        skill.UpdatedAt = DateTime.UtcNow;
        skill.UpdatedBy = updatedBy;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(skill);
    }

    public async Task<bool> DeleteSkillAsync(Guid skillId, CancellationToken cancellationToken = default)
    {
        var skill = await _context.Skills.FirstOrDefaultAsync(s => s.Id == skillId, cancellationToken);
        if (skill == null)
            return false;

        _context.Skills.Remove(skill);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static SkillDto MapToDto(Skill skill)
    {
        return new SkillDto
        {
            Id = skill.Id,
            SkillKey = skill.SkillKey,
            Name = skill.Name,
            Description = skill.Description,
            Content = skill.Content,
            Category = skill.Category.ToString(),
            OrganizationId = skill.OrganizationId,
            ProjectId = skill.ProjectId,
            Scope = skill.OrganizationId.HasValue ? "Organization" : "Project",
            IsPublished = skill.IsPublished,
            CreatedAt = skill.CreatedAt,
            CreatedBy = skill.CreatedBy,
            UpdatedAt = skill.UpdatedAt,
            UpdatedBy = skill.UpdatedBy
        };
    }

    private static SkillListItemDto MapToListItemDto(Skill skill)
    {
        return new SkillListItemDto
        {
            Id = skill.Id,
            SkillKey = skill.SkillKey,
            Name = skill.Name,
            Description = skill.Description,
            Category = skill.Category.ToString(),
            Scope = skill.OrganizationId.HasValue ? "Organization" : "Project",
            ProjectName = skill.Project?.Name,
            IsPublished = skill.IsPublished
        };
    }
}
