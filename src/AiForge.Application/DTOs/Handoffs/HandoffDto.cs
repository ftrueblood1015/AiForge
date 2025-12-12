using AiForge.Domain.Enums;

namespace AiForge.Application.DTOs.Handoffs;

public class HandoffDto
{
    public Guid Id { get; set; }
    public Guid TicketId { get; set; }
    public string? SessionId { get; set; }
    public string Title { get; set; } = string.Empty;
    public HandoffType Type { get; set; }
    public string Summary { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class HandoffDetailDto : HandoffDto
{
    public string Content { get; set; } = string.Empty;
    public StructuredContextDto? StructuredContext { get; set; }
    public Guid? SupersededById { get; set; }
    public List<FileSnapshotDto> FileSnapshots { get; set; } = new();
    public int VersionCount { get; set; }
}

public class StructuredContextDto
{
    public List<string> Assumptions { get; set; } = new();
    public List<DecisionDto> DecisionsMade { get; set; } = new();
    public List<string> OpenQuestions { get; set; } = new();
    public List<string> Blockers { get; set; } = new();
    public List<string> FilesModified { get; set; } = new();
    public List<string> TestsAdded { get; set; } = new();
    public List<string> NextSteps { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}

public class DecisionDto
{
    public string Decision { get; set; } = string.Empty;
    public string Rationale { get; set; } = string.Empty;
}

public class FileSnapshotDto
{
    public Guid Id { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string? ContentBefore { get; set; }
    public string? ContentAfter { get; set; }
    public string Language { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CreateHandoffRequest
{
    public Guid TicketId { get; set; }
    public string? SessionId { get; set; }
    public string Title { get; set; } = string.Empty;
    public HandoffType Type { get; set; }
    public string Summary { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public StructuredContextDto? Context { get; set; }
}

public class UpdateHandoffRequest
{
    public string? Title { get; set; }
    public string? Summary { get; set; }
    public string? Content { get; set; }
    public StructuredContextDto? Context { get; set; }
}

public class CreateFileSnapshotRequest
{
    public string FilePath { get; set; } = string.Empty;
    public string? ContentBefore { get; set; }
    public string? ContentAfter { get; set; }
    public string Language { get; set; } = string.Empty;
}
