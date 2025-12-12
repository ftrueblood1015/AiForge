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
    public string? CurrentHandoffSummary { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class TicketDetailDto : TicketDto
{
    public List<TicketDto> SubTickets { get; set; } = new();
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
}

public class UpdateTicketRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public TicketType? Type { get; set; }
    public Priority? Priority { get; set; }
    public Guid? ParentTicketId { get; set; }
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
