namespace AiForge.Application.DTOs.TestLink;

public class TestLinkDto
{
    public Guid Id { get; set; }
    public Guid TicketId { get; set; }
    public string? TicketKey { get; set; }
    public string TestFilePath { get; set; } = string.Empty;
    public string? TestName { get; set; }
    public string? TestedFunctionality { get; set; }
    public string? Outcome { get; set; }
    public string? LinkedFilePath { get; set; }
    public string? SessionId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastRunAt { get; set; }
}

public class LinkTestRequest
{
    public string TestFilePath { get; set; } = string.Empty;
    public string? TestName { get; set; }
    public string? TestedFunctionality { get; set; }
    public string? LinkedFilePath { get; set; }
    public string? Outcome { get; set; }  // Passed, Failed, Skipped, NotRun
    public string? SessionId { get; set; }
}

public class UpdateTestOutcomeRequest
{
    public string Outcome { get; set; } = string.Empty;
}

public class FileCoverageResponse
{
    public string FilePath { get; set; } = string.Empty;
    public List<TestLinkDto> Tests { get; set; } = new();
    public int TotalTests { get; set; }
}
