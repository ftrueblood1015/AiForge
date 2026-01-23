namespace AiForge.Domain.ValueObjects;

public class ContextHelper
{
    public string CurrentFocus { get; set; } = string.Empty;
    public List<string> KeyDecisions { get; set; } = [];
    public List<string> BlockersResolved { get; set; } = [];
    public List<string> NextSteps { get; set; } = [];
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Extensible metadata as JSON string for future needs
    /// </summary>
    public string? MetadataJson { get; set; }

    /// <summary>
    /// Validates that serialized size is under 2KB
    /// </summary>
    public bool IsValidSize()
    {
        var json = System.Text.Json.JsonSerializer.Serialize(this);
        return System.Text.Encoding.UTF8.GetByteCount(json) <= 2048;
    }
}
