using System.Text.Json;
using System.Text.RegularExpressions;
using AiForge.Application.DTOs.PromptTemplate;
using AiForge.Domain.Entities;
using AiForge.Domain.Interfaces;
using AiForge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AiForge.Application.Services;

public interface IPromptTemplateService
{
    Task<PromptTemplateDto> CreateAsync(CreatePromptTemplateRequest request, string createdBy, CancellationToken cancellationToken = default);
    Task<PromptTemplateDto?> GetByIdAsync(Guid templateId, CancellationToken cancellationToken = default);
    Task<PromptTemplateDto?> GetByKeyAsync(string templateKey, Guid? projectId, Guid organizationId, CancellationToken cancellationToken = default);
    Task<PromptTemplateListResponse> GetTemplatesAsync(Guid? organizationId, Guid? projectId, string? category, CancellationToken cancellationToken = default);
    Task<PromptTemplateDto?> UpdateAsync(Guid templateId, UpdatePromptTemplateRequest request, string updatedBy, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid templateId, CancellationToken cancellationToken = default);
    Task<RenderTemplateResponse> RenderAsync(Guid templateId, RenderTemplateRequest request, CancellationToken cancellationToken = default);
    Task<RenderTemplateResponse> RenderByKeyAsync(string templateKey, Guid? projectId, Guid organizationId, RenderTemplateRequest request, CancellationToken cancellationToken = default);
}

public class PromptTemplateService : IPromptTemplateService
{
    private readonly AiForgeDbContext _context;
    private readonly IScopeResolver _scopeResolver;
    private readonly IUnitOfWork _unitOfWork;
    private static readonly Regex VariablePattern = new(@"\{\{(\w+)\}\}", RegexOptions.Compiled);

    public PromptTemplateService(AiForgeDbContext context, IScopeResolver scopeResolver, IUnitOfWork unitOfWork)
    {
        _context = context;
        _scopeResolver = scopeResolver;
        _unitOfWork = unitOfWork;
    }

    public async Task<PromptTemplateDto> CreateAsync(CreatePromptTemplateRequest request, string createdBy, CancellationToken cancellationToken = default)
    {
        if ((request.OrganizationId.HasValue && request.ProjectId.HasValue) ||
            (!request.OrganizationId.HasValue && !request.ProjectId.HasValue))
        {
            throw new InvalidOperationException("Exactly one of OrganizationId or ProjectId must be specified");
        }

        var existingTemplate = await _context.PromptTemplates
            .FirstOrDefaultAsync(t =>
                t.TemplateKey == request.TemplateKey &&
                ((request.OrganizationId.HasValue && t.OrganizationId == request.OrganizationId) ||
                 (request.ProjectId.HasValue && t.ProjectId == request.ProjectId)),
                cancellationToken);

        if (existingTemplate != null)
        {
            throw new InvalidOperationException($"Template with key '{request.TemplateKey}' already exists in this scope");
        }

        // Auto-detect variables from template if not provided
        var variables = request.Variables ?? ExtractVariables(request.Template);

        var template = new PromptTemplate
        {
            Id = Guid.NewGuid(),
            TemplateKey = request.TemplateKey,
            Name = request.Name,
            Description = request.Description,
            Content = request.Template,
            Variables = JsonSerializer.Serialize(variables),
            Category = request.Category,
            OrganizationId = request.OrganizationId,
            ProjectId = request.ProjectId,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };

        _context.PromptTemplates.Add(template);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(template);
    }

    public async Task<PromptTemplateDto?> GetByIdAsync(Guid templateId, CancellationToken cancellationToken = default)
    {
        var template = await _context.PromptTemplates
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == templateId, cancellationToken);

        return template != null ? MapToDto(template) : null;
    }

    public async Task<PromptTemplateDto?> GetByKeyAsync(string templateKey, Guid? projectId, Guid organizationId, CancellationToken cancellationToken = default)
    {
        var template = await _scopeResolver.ResolveTemplateAsync(templateKey, projectId, organizationId, cancellationToken);
        return template != null ? MapToDto(template) : null;
    }

    public async Task<PromptTemplateListResponse> GetTemplatesAsync(Guid? organizationId, Guid? projectId, string? category, CancellationToken cancellationToken = default)
    {
        IQueryable<PromptTemplate> query = _context.PromptTemplates.AsNoTracking();

        if (organizationId.HasValue && projectId.HasValue)
        {
            query = query.Where(t => t.OrganizationId == organizationId || t.ProjectId == projectId);
        }
        else if (organizationId.HasValue)
        {
            query = query.Where(t => t.OrganizationId == organizationId);
        }
        else if (projectId.HasValue)
        {
            query = query.Where(t => t.ProjectId == projectId);
        }

        if (!string.IsNullOrEmpty(category))
        {
            query = query.Where(t => t.Category == category);
        }

        var templates = await query.OrderBy(t => t.Name).ToListAsync(cancellationToken);

        return new PromptTemplateListResponse
        {
            Items = templates.Select(MapToListItemDto).ToList(),
            TotalCount = templates.Count
        };
    }

    public async Task<PromptTemplateDto?> UpdateAsync(Guid templateId, UpdatePromptTemplateRequest request, string updatedBy, CancellationToken cancellationToken = default)
    {
        var template = await _context.PromptTemplates.FirstOrDefaultAsync(t => t.Id == templateId, cancellationToken);
        if (template == null)
            return null;

        if (!string.IsNullOrEmpty(request.Name))
            template.Name = request.Name;

        if (request.Description != null)
            template.Description = request.Description;

        if (request.Template != null)
        {
            template.Content = request.Template;
            // Re-extract variables if template content changed and variables not explicitly provided
            if (request.Variables == null)
            {
                var variables = ExtractVariables(request.Template);
                template.Variables = JsonSerializer.Serialize(variables);
            }
        }

        if (request.Variables != null)
            template.Variables = JsonSerializer.Serialize(request.Variables);

        if (!string.IsNullOrEmpty(request.Category))
            template.Category = request.Category;

        template.UpdatedAt = DateTime.UtcNow;
        template.UpdatedBy = updatedBy;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(template);
    }

    public async Task<bool> DeleteAsync(Guid templateId, CancellationToken cancellationToken = default)
    {
        var template = await _context.PromptTemplates.FirstOrDefaultAsync(t => t.Id == templateId, cancellationToken);
        if (template == null)
            return false;

        _context.PromptTemplates.Remove(template);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<RenderTemplateResponse> RenderAsync(Guid templateId, RenderTemplateRequest request, CancellationToken cancellationToken = default)
    {
        var template = await _context.PromptTemplates
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == templateId, cancellationToken);

        if (template == null)
            throw new InvalidOperationException($"Template with ID '{templateId}' not found");

        return RenderTemplate(template, request.Variables);
    }

    public async Task<RenderTemplateResponse> RenderByKeyAsync(string templateKey, Guid? projectId, Guid organizationId, RenderTemplateRequest request, CancellationToken cancellationToken = default)
    {
        var template = await _scopeResolver.ResolveTemplateAsync(templateKey, projectId, organizationId, cancellationToken);

        if (template == null)
            throw new InvalidOperationException($"Template with key '{templateKey}' not found");

        return RenderTemplate(template, request.Variables);
    }

    private static RenderTemplateResponse RenderTemplate(PromptTemplate template, Dictionary<string, string> variables)
    {
        var templateVariables = ParseVariables(template.Variables);
        var missingVariables = new List<string>();
        var renderedContent = template.Content;

        foreach (var variable in templateVariables)
        {
            var placeholder = $"{{{{{variable}}}}}";
            if (variables.TryGetValue(variable, out var value))
            {
                renderedContent = renderedContent.Replace(placeholder, value);
            }
            else
            {
                missingVariables.Add(variable);
            }
        }

        return new RenderTemplateResponse
        {
            RenderedContent = renderedContent,
            MissingVariables = missingVariables
        };
    }

    private static List<string> ExtractVariables(string template)
    {
        var matches = VariablePattern.Matches(template);
        return matches.Select(m => m.Groups[1].Value).Distinct().ToList();
    }

    private static PromptTemplateDto MapToDto(PromptTemplate template)
    {
        return new PromptTemplateDto
        {
            Id = template.Id,
            TemplateKey = template.TemplateKey,
            Name = template.Name,
            Description = template.Description,
            Template = template.Content,
            Variables = ParseVariables(template.Variables),
            Category = template.Category ?? string.Empty,
            OrganizationId = template.OrganizationId,
            ProjectId = template.ProjectId,
            Scope = template.OrganizationId.HasValue ? "Organization" : "Project",
            IsPublished = true, // No IsPublished in entity, assume published
            CreatedAt = template.CreatedAt,
            CreatedBy = template.CreatedBy,
            UpdatedAt = template.UpdatedAt,
            UpdatedBy = template.UpdatedBy
        };
    }

    private static PromptTemplateListItemDto MapToListItemDto(PromptTemplate template)
    {
        return new PromptTemplateListItemDto
        {
            Id = template.Id,
            TemplateKey = template.TemplateKey,
            Name = template.Name,
            Description = template.Description,
            Category = template.Category ?? string.Empty,
            Variables = ParseVariables(template.Variables),
            Scope = template.OrganizationId.HasValue ? "Organization" : "Project",
            IsPublished = true // No IsPublished in entity, assume published
        };
    }

    private static List<string> ParseVariables(string? variablesJson)
    {
        if (string.IsNullOrEmpty(variablesJson))
            return new List<string>();

        try
        {
            return JsonSerializer.Deserialize<List<string>>(variablesJson) ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }
}
