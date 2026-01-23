namespace AiForge.Application.DTOs.PromptTemplate;

public class PromptTemplateDto
{
    public Guid Id { get; set; }
    public string TemplateKey { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Template { get; set; } = string.Empty;
    public List<string> Variables { get; set; } = new();
    public string Category { get; set; } = string.Empty;
    public Guid? OrganizationId { get; set; }
    public Guid? ProjectId { get; set; }
    public string Scope { get; set; } = string.Empty;
    public bool IsPublished { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}

public class PromptTemplateListItemDto
{
    public Guid Id { get; set; }
    public string TemplateKey { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
    public List<string> Variables { get; set; } = new();
    public string Scope { get; set; } = string.Empty;
    public bool IsPublished { get; set; }
}

public class CreatePromptTemplateRequest
{
    public string TemplateKey { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Template { get; set; } = string.Empty;
    public List<string>? Variables { get; set; }
    public string Category { get; set; } = string.Empty;
    public Guid? OrganizationId { get; set; }
    public Guid? ProjectId { get; set; }
}

public class UpdatePromptTemplateRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Template { get; set; }
    public List<string>? Variables { get; set; }
    public string? Category { get; set; }
    public bool? IsPublished { get; set; }
}

public class PromptTemplateListResponse
{
    public List<PromptTemplateListItemDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
}

public class RenderTemplateRequest
{
    public Dictionary<string, string> Variables { get; set; } = new();
}

public class RenderTemplateResponse
{
    public string RenderedContent { get; set; } = string.Empty;
    public List<string> MissingVariables { get; set; } = new();
}
