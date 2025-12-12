namespace AiForge.Domain.Entities;

public class Comment
{
    public Guid Id { get; set; }
    public Guid TicketId { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsAiGenerated { get; set; }
    public string? SessionId { get; set; }                       // Claude session if AI
    public DateTime CreatedAt { get; set; }

    // Navigation
    public Ticket Ticket { get; set; } = null!;
}
