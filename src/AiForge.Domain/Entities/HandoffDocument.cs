using AiForge.Domain.Enums;

namespace AiForge.Domain.Entities;

public class HandoffDocument
{
    public Guid Id { get; set; }
    public Guid TicketId { get; set; }
    public string? SessionId { get; set; }
    public string Title { get; set; } = string.Empty;
    public HandoffType Type { get; set; }
    public string Summary { get; set; } = string.Empty;          // Short, for lists
    public string Content { get; set; } = string.Empty;          // Full markdown
    public string? StructuredContext { get; set; }               // JSON blob
    public bool IsActive { get; set; } = true;                   // False if superseded
    public Guid? SupersededById { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation
    public Ticket Ticket { get; set; } = null!;
    public HandoffDocument? SupersededBy { get; set; }
    public ICollection<HandoffVersion> Versions { get; set; } = new List<HandoffVersion>();
    public ICollection<FileSnapshot> FileSnapshots { get; set; } = new List<FileSnapshot>();
}
