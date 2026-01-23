namespace AiForge.Application.DTOs.Agent;

public class AgentDto
{
    public Guid Id { get; set; }
    public string AgentKey { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? SystemPrompt { get; set; }
    public string? Instructions { get; set; }
    public string AgentType { get; set; } = string.Empty;
    public List<string> Capabilities { get; set; } = new();
    public string Status { get; set; } = string.Empty;
    public Guid? OrganizationId { get; set; }
    public Guid? ProjectId { get; set; }
    public string Scope { get; set; } = string.Empty;  // "Organization" or "Project"
    public bool IsEnabled { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}

public class AgentListItemDto
{
    public Guid Id { get; set; }
    public string AgentKey { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string AgentType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Scope { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
}

public class CreateAgentRequest
{
    public string AgentKey { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? SystemPrompt { get; set; }
    public string? Instructions { get; set; }
    public string AgentType { get; set; } = "Claude";  // Claude, GPT, Gemini, Custom
    public List<string>? Capabilities { get; set; }
    public Guid? OrganizationId { get; set; }
    public Guid? ProjectId { get; set; }
}

public class UpdateAgentRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? SystemPrompt { get; set; }
    public string? Instructions { get; set; }
    public string? AgentType { get; set; }
    public List<string>? Capabilities { get; set; }
    public string? Status { get; set; }
    public bool? IsEnabled { get; set; }
}

public class AgentListResponse
{
    public List<AgentListItemDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
}
