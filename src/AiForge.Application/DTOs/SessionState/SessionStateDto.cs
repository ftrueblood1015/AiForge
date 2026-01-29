namespace AiForge.Application.DTOs.SessionState;

public class SessionStateDto
{
    public Guid Id { get; set; }
    public string SessionId { get; set; } = string.Empty;
    public Guid? TicketId { get; set; }
    public string? TicketKey { get; set; }
    public Guid? QueueId { get; set; }
    public string? QueueName { get; set; }
    public string CurrentPhase { get; set; } = string.Empty;
    public string? WorkingSummary { get; set; }
    public Dictionary<string, object>? Checkpoint { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsExpired => ExpiresAt < DateTime.UtcNow;
}

public class SaveSessionStateRequest
{
    public string SessionId { get; set; } = string.Empty;
    public Guid? TicketId { get; set; }
    public Guid? QueueId { get; set; }
    public string CurrentPhase { get; set; } = "Researching";
    public string? WorkingSummary { get; set; }
    public Dictionary<string, object>? Checkpoint { get; set; }
    public int ExpiresInHours { get; set; } = 24;
}
