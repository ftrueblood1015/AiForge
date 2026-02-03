namespace AiForge.Domain.Entities;

public class ApiKey
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;              // The actual GUID key
    public string Name { get; set; } = string.Empty;             // Friendly name
    public bool IsActive { get; set; } = true;
    public int RateLimitPerMinute { get; set; }                  // Max requests per minute (0 = unlimited)
    public Guid? UserId { get; set; }                            // User who owns this key (null = service account)
    public bool IsServiceAccount { get; set; }                   // Service accounts have elevated access
    public string? Scopes { get; set; }                          // JSON array of allowed scopes (future use)
    public DateTime CreatedAt { get; set; }
    public DateTime? LastUsedAt { get; set; }

    // Navigation
    public ICollection<ApiKeyUsage> Usages { get; set; } = new List<ApiKeyUsage>();
}
