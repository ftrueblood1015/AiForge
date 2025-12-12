namespace AiForge.Domain.Entities;

public class HandoffVersion
{
    public Guid Id { get; set; }
    public Guid HandoffId { get; set; }
    public int Version { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? StructuredContext { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation
    public HandoffDocument Handoff { get; set; } = null!;
}
