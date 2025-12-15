using AiForge.Domain.Enums;

namespace AiForge.Domain.Entities;

public class TechnicalDebt
{
    public Guid Id { get; set; }
    public Guid OriginatingTicketId { get; set; }
    public Guid? ResolutionTicketId { get; set; }

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DebtCategory Category { get; set; }
    public DebtSeverity Severity { get; set; }
    public DebtStatus Status { get; set; } = DebtStatus.Open;
    public string? Rationale { get; set; }  // Why shortcut was taken
    public string? AffectedFiles { get; set; }  // Comma-separated
    public string? SessionId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }

    // Navigation
    public Ticket OriginatingTicket { get; set; } = null!;
    public Ticket? ResolutionTicket { get; set; }
}
