using AiForge.Domain.Enums;

namespace AiForge.Domain.Entities;

public class TestLink
{
    public Guid Id { get; set; }
    public Guid TicketId { get; set; }

    public string TestFilePath { get; set; } = string.Empty;
    public string? TestName { get; set; }  // Specific test case name
    public string? TestedFunctionality { get; set; }
    public TestOutcome? Outcome { get; set; }
    public string? LinkedFilePath { get; set; }  // Code file being tested
    public string? SessionId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastRunAt { get; set; }

    // Navigation
    public Ticket Ticket { get; set; } = null!;
}
