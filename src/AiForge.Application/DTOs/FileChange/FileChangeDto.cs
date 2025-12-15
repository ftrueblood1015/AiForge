namespace AiForge.Application.DTOs.FileChange;

public class FileChangeDto
{
    public Guid Id { get; set; }
    public Guid TicketId { get; set; }
    public string? TicketKey { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string ChangeType { get; set; } = string.Empty;
    public string? OldFilePath { get; set; }
    public string? ChangeReason { get; set; }
    public int? LinesAdded { get; set; }
    public int? LinesRemoved { get; set; }
    public string? SessionId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class LogFileChangeRequest
{
    public string FilePath { get; set; } = string.Empty;
    public string ChangeType { get; set; } = string.Empty;  // Created, Modified, Deleted, Renamed
    public string? OldFilePath { get; set; }
    public string? ChangeReason { get; set; }
    public int? LinesAdded { get; set; }
    public int? LinesRemoved { get; set; }
    public string? SessionId { get; set; }
}

public class FileHistoryResponse
{
    public string FilePath { get; set; } = string.Empty;
    public List<FileChangeDto> Changes { get; set; } = new();
    public int TotalTickets { get; set; }
}

public class HotFileDto
{
    public string FilePath { get; set; } = string.Empty;
    public int ChangeCount { get; set; }
    public DateTime LastModified { get; set; }
}
