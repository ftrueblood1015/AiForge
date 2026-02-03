using AiForge.Domain.Enums;

namespace AiForge.Application.DTOs.Tickets;

public class TicketDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string ProjectKey { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public int Number { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TicketType Type { get; set; }
    public TicketStatus Status { get; set; }
    public Priority Priority { get; set; }
    public Guid? ParentTicketId { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public string? CurrentHandoffSummary { get; set; }

    // Auto-generated summaries
    public string? ProgressSummary { get; set; }
    public string? DecisionSummary { get; set; }
    public string? OutcomeStatistics { get; set; }
    public DateTime? SummaryUpdatedAt { get; set; }

    public int SubTicketCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Lightweight DTO for sub-ticket list items
/// </summary>
public class SubTicketSummaryDto
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public TicketStatus Status { get; set; }
    public TicketType Type { get; set; }
    public Priority Priority { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class TicketDetailDto : TicketDto
{
    public List<SubTicketSummaryDto> SubTickets { get; set; } = [];
    public int CompletedSubTicketCount { get; set; }
    public decimal SubTicketProgress { get; set; }
    public SubTicketSummaryDto? ParentTicket { get; set; }
    public int CommentCount { get; set; }
}

public class CreateTicketRequest
{
    public string ProjectKey { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TicketType Type { get; set; } = TicketType.Task;
    public Priority Priority { get; set; } = Priority.Medium;
    public Guid? ParentTicketId { get; set; }
    /// <summary>
    /// Optional: User who created the ticket. If not specified, auto-set from authenticated user.
    /// Used by MCP/service accounts to specify creator.
    /// </summary>
    public Guid? CreatedByUserId { get; set; }
    /// <summary>
    /// Optional: User to assign the ticket to.
    /// </summary>
    public Guid? AssignedToUserId { get; set; }
}

public class UpdateTicketRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public TicketType? Type { get; set; }
    public Priority? Priority { get; set; }
    public Guid? ParentTicketId { get; set; }
    /// <summary>
    /// Optional: User to assign the ticket to. Set to empty GUID to unassign.
    /// </summary>
    public Guid? AssignedToUserId { get; set; }
    /// <summary>
    /// When true, AssignedToUserId of null means "unassign". When false/absent, null means "no change".
    /// </summary>
    public bool ClearAssignee { get; set; }
}

public class TransitionTicketRequest
{
    public TicketStatus Status { get; set; }
}

public class TicketSearchRequest
{
    public Guid? ProjectId { get; set; }
    public TicketStatus? Status { get; set; }
    public TicketType? Type { get; set; }
    public Priority? Priority { get; set; }
    public string? Search { get; set; }
}

/// <summary>
/// Request to create a sub-ticket under a parent ticket
/// </summary>
public class CreateSubTicketRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TicketType Type { get; set; } = TicketType.Task;
    public Priority Priority { get; set; } = Priority.Medium;
}

/// <summary>
/// Request to move a ticket to a different parent (or promote to top-level)
/// </summary>
public class MoveSubTicketRequest
{
    /// <summary>
    /// New parent ticket ID. Set to null to promote to top-level ticket.
    /// </summary>
    public Guid? NewParentTicketId { get; set; }
}
