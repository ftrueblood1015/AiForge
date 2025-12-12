namespace AiForge.Domain.Entities;

public class FileSnapshot
{
    public Guid Id { get; set; }
    public Guid HandoffId { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string? ContentBefore { get; set; }                   // Nullable for new files
    public string? ContentAfter { get; set; }                    // Nullable for deleted files
    public string Language { get; set; } = string.Empty;         // For syntax highlighting
    public DateTime CreatedAt { get; set; }

    // Navigation
    public HandoffDocument Handoff { get; set; } = null!;
}
