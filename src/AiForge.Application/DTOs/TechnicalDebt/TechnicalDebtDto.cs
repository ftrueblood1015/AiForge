namespace AiForge.Application.DTOs.TechnicalDebt;

public class TechnicalDebtDto
{
    public Guid Id { get; set; }
    public Guid OriginatingTicketId { get; set; }
    public string? OriginatingTicketKey { get; set; }
    public Guid? ResolutionTicketId { get; set; }
    public string? ResolutionTicketKey { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Rationale { get; set; }
    public string? AffectedFiles { get; set; }
    public string? SessionId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
}

public class CreateDebtRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;  // Performance, Security, Maintainability, Testing, Documentation, Architecture
    public string Severity { get; set; } = string.Empty;  // Low, Medium, High, Critical
    public string? Rationale { get; set; }
    public string? AffectedFiles { get; set; }
    public string? SessionId { get; set; }
}

public class UpdateDebtRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Category { get; set; }
    public string? Severity { get; set; }
    public string? Status { get; set; }
    public string? Rationale { get; set; }
    public string? AffectedFiles { get; set; }
}

public class ResolveDebtRequest
{
    public Guid? ResolutionTicketId { get; set; }
}

public class DebtBacklogResponse
{
    public List<TechnicalDebtDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
}

public class DebtSummaryResponse
{
    public int TotalOpen { get; set; }
    public int TotalResolved { get; set; }
    public Dictionary<string, int> ByCategory { get; set; } = new();
    public Dictionary<string, int> BySeverity { get; set; } = new();
}
