namespace AiForge.Application.DTOs.Analytics.Patterns;

public class RecurringIssueDto
{
    public string Pattern { get; set; } = string.Empty;
    public int OccurrenceCount { get; set; }
    public List<RelatedTicket> RelatedTickets { get; set; } = new();
}

public class RelatedTicket
{
    public Guid TicketId { get; set; }
    public string TicketKey { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class RecurringIssueRequest
{
    public Guid? ProjectId { get; set; }
    public string? TicketType { get; set; } // Bug, Feature, etc.
    public int MinOccurrences { get; set; } = 2;
}
