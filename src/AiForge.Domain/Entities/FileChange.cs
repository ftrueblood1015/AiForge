using AiForge.Domain.Enums;

namespace AiForge.Domain.Entities;

public class FileChange
{
    public Guid Id { get; set; }
    public Guid TicketId { get; set; }

    public string FilePath { get; set; } = string.Empty;
    public FileChangeType ChangeType { get; set; }
    public string? OldFilePath { get; set; }  // For renames
    public string? ChangeReason { get; set; }
    public int? LinesAdded { get; set; }
    public int? LinesRemoved { get; set; }
    public string? SessionId { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation
    public Ticket Ticket { get; set; } = null!;
}
