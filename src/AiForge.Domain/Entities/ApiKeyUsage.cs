namespace AiForge.Domain.Entities;

public class ApiKeyUsage
{
    public Guid Id { get; set; }
    public Guid ApiKeyId { get; set; }
    public DateTime WindowStart { get; set; }                    // Start of the current minute window
    public int RequestCount { get; set; }                        // Requests in this window

    // Navigation
    public ApiKey ApiKey { get; set; } = null!;
}
