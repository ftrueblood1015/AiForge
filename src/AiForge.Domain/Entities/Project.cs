namespace AiForge.Domain.Entities;

public class Project
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;              // e.g., "AIFORGE", "MYPROJ"
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int NextTicketNumber { get; set; } = 1;               // For generating PROJ-1, PROJ-2, etc.

    // Navigation
    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
