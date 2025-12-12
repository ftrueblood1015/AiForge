using AiForge.Domain.Enums;

namespace AiForge.Application.DTOs.Planning;

public class ProgressEntryDto
{
    public Guid Id { get; set; }
    public Guid TicketId { get; set; }
    public string? SessionId { get; set; }
    public string Content { get; set; } = string.Empty;
    public ProgressOutcome Outcome { get; set; }
    public List<string> FilesAffected { get; set; } = new();
    public string? ErrorDetails { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateProgressEntryRequest
{
    public Guid TicketId { get; set; }
    public string? SessionId { get; set; }
    public string Content { get; set; } = string.Empty;
    public ProgressOutcome Outcome { get; set; } = ProgressOutcome.Success;
    public List<string>? FilesAffected { get; set; }
    public string? ErrorDetails { get; set; }
}
