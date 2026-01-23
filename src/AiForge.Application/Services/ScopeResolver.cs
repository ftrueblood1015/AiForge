using AiForge.Domain.Entities;
using AiForge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AiForge.Application.Services;

/// <summary>
/// Resolves configuration entities (Agents, Skills, Templates) based on scope hierarchy.
/// Project-level configurations take precedence over organization-level.
/// </summary>
public interface IScopeResolver
{
    /// <summary>
    /// Resolves an Agent by key, checking project-level first, then falling back to organization-level.
    /// </summary>
    Task<Agent?> ResolveAgentAsync(string agentKey, Guid? projectId, Guid organizationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resolves a Skill by key, checking project-level first, then falling back to organization-level.
    /// </summary>
    Task<Skill?> ResolveSkillAsync(string skillKey, Guid? projectId, Guid organizationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resolves a PromptTemplate by key, checking project-level first, then falling back to organization-level.
    /// </summary>
    Task<PromptTemplate?> ResolveTemplateAsync(string templateKey, Guid? projectId, Guid organizationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all Agents available in scope (both project-level and organization-level).
    /// Project-level entities with the same key shadow organization-level ones.
    /// </summary>
    Task<IEnumerable<Agent>> GetAgentsInScopeAsync(Guid? projectId, Guid organizationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all Skills available in scope (both project-level and organization-level).
    /// Project-level entities with the same key shadow organization-level ones.
    /// </summary>
    Task<IEnumerable<Skill>> GetSkillsInScopeAsync(Guid? projectId, Guid organizationId, bool publishedOnly = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all PromptTemplates available in scope (both project-level and organization-level).
    /// Project-level entities with the same key shadow organization-level ones.
    /// </summary>
    Task<IEnumerable<PromptTemplate>> GetTemplatesInScopeAsync(Guid? projectId, Guid organizationId, CancellationToken cancellationToken = default);
}

public class ScopeResolver : IScopeResolver
{
    private readonly AiForgeDbContext _context;

    public ScopeResolver(AiForgeDbContext context)
    {
        _context = context;
    }

    public async Task<Agent?> ResolveAgentAsync(string agentKey, Guid? projectId, Guid organizationId, CancellationToken cancellationToken = default)
    {
        // 1. Try project-level first (if project context exists)
        if (projectId.HasValue)
        {
            var projectAgent = await _context.Agents
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.AgentKey == agentKey && a.ProjectId == projectId && a.IsEnabled, cancellationToken);

            if (projectAgent != null)
                return projectAgent;
        }

        // 2. Fall back to organization-level
        return await _context.Agents
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.AgentKey == agentKey && a.OrganizationId == organizationId && a.IsEnabled, cancellationToken);
    }

    public async Task<Skill?> ResolveSkillAsync(string skillKey, Guid? projectId, Guid organizationId, CancellationToken cancellationToken = default)
    {
        // 1. Try project-level first (if project context exists)
        if (projectId.HasValue)
        {
            var projectSkill = await _context.Skills
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.SkillKey == skillKey && s.ProjectId == projectId && s.IsPublished, cancellationToken);

            if (projectSkill != null)
                return projectSkill;
        }

        // 2. Fall back to organization-level
        return await _context.Skills
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.SkillKey == skillKey && s.OrganizationId == organizationId && s.IsPublished, cancellationToken);
    }

    public async Task<PromptTemplate?> ResolveTemplateAsync(string templateKey, Guid? projectId, Guid organizationId, CancellationToken cancellationToken = default)
    {
        // 1. Try project-level first (if project context exists)
        if (projectId.HasValue)
        {
            var projectTemplate = await _context.PromptTemplates
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.TemplateKey == templateKey && t.ProjectId == projectId, cancellationToken);

            if (projectTemplate != null)
                return projectTemplate;
        }

        // 2. Fall back to organization-level
        return await _context.PromptTemplates
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.TemplateKey == templateKey && t.OrganizationId == organizationId, cancellationToken);
    }

    public async Task<IEnumerable<Agent>> GetAgentsInScopeAsync(Guid? projectId, Guid organizationId, CancellationToken cancellationToken = default)
    {
        // Get organization-level agents
        var orgAgents = await _context.Agents
            .AsNoTracking()
            .Where(a => a.OrganizationId == organizationId && a.IsEnabled)
            .ToListAsync(cancellationToken);

        if (!projectId.HasValue)
            return orgAgents;

        // Get project-level agents
        var projectAgents = await _context.Agents
            .AsNoTracking()
            .Where(a => a.ProjectId == projectId && a.IsEnabled)
            .ToListAsync(cancellationToken);

        // Merge: project-level shadows org-level with same key
        var projectKeys = projectAgents.Select(a => a.AgentKey).ToHashSet();
        var filteredOrgAgents = orgAgents.Where(a => !projectKeys.Contains(a.AgentKey));

        return projectAgents.Concat(filteredOrgAgents).OrderBy(a => a.Name);
    }

    public async Task<IEnumerable<Skill>> GetSkillsInScopeAsync(Guid? projectId, Guid organizationId, bool publishedOnly = false, CancellationToken cancellationToken = default)
    {
        // Get organization-level skills
        var orgQuery = _context.Skills
            .AsNoTracking()
            .Where(s => s.OrganizationId == organizationId);

        if (publishedOnly)
            orgQuery = orgQuery.Where(s => s.IsPublished);

        var orgSkills = await orgQuery.ToListAsync(cancellationToken);

        if (!projectId.HasValue)
            return orgSkills;

        // Get project-level skills
        var projectQuery = _context.Skills
            .AsNoTracking()
            .Where(s => s.ProjectId == projectId);

        if (publishedOnly)
            projectQuery = projectQuery.Where(s => s.IsPublished);

        var projectSkills = await projectQuery.ToListAsync(cancellationToken);

        // Merge: project-level shadows org-level with same key
        var projectKeys = projectSkills.Select(s => s.SkillKey).ToHashSet();
        var filteredOrgSkills = orgSkills.Where(s => !projectKeys.Contains(s.SkillKey));

        return projectSkills.Concat(filteredOrgSkills).OrderBy(s => s.Name);
    }

    public async Task<IEnumerable<PromptTemplate>> GetTemplatesInScopeAsync(Guid? projectId, Guid organizationId, CancellationToken cancellationToken = default)
    {
        // Get organization-level templates
        var orgTemplates = await _context.PromptTemplates
            .AsNoTracking()
            .Where(t => t.OrganizationId == organizationId)
            .ToListAsync(cancellationToken);

        if (!projectId.HasValue)
            return orgTemplates;

        // Get project-level templates
        var projectTemplates = await _context.PromptTemplates
            .AsNoTracking()
            .Where(t => t.ProjectId == projectId)
            .ToListAsync(cancellationToken);

        // Merge: project-level shadows org-level with same key
        var projectKeys = projectTemplates.Select(t => t.TemplateKey).ToHashSet();
        var filteredOrgTemplates = orgTemplates.Where(t => !projectKeys.Contains(t.TemplateKey));

        return projectTemplates.Concat(filteredOrgTemplates).OrderBy(t => t.Name);
    }
}
