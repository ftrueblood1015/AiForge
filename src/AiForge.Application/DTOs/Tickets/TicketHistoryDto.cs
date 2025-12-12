namespace AiForge.Application.DTOs.Tickets;

public class TicketHistoryDto
{
    public Guid Id { get; set; }
    public Guid TicketId { get; set; }
    public string Field { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string? ChangedBy { get; set; }
    public DateTime ChangedAt { get; set; }
}
