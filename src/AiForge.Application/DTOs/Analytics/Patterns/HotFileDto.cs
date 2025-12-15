namespace AiForge.Application.DTOs.Analytics.Patterns;

public class AnalyticsHotFileDto
{
    public string FilePath { get; set; } = string.Empty;
    public int ModificationCount { get; set; }
    public int TicketCount { get; set; }
    public int TotalLinesAdded { get; set; }
    public int TotalLinesRemoved { get; set; }
    public DateTime? FirstModified { get; set; }
    public DateTime? LastModified { get; set; }
    public List<string> RecentTicketKeys { get; set; } = new();
}

public class HotFileRequest
{
    public Guid? ProjectId { get; set; }
    public DateTime? Since { get; set; }
    public int TopN { get; set; } = 20;
}
