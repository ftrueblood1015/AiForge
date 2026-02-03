using AiForge.Domain.Enums;

namespace AiForge.Domain.Entities;

public class Ticket
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string Key { get; set; } = string.Empty;              // e.g., "AIFORGE-123"
    public int Number { get; set; }                              // Sequential within project
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TicketType Type { get; set; }
    public TicketStatus Status { get; set; }
    public Priority Priority { get; set; }
    public Guid? ParentTicketId { get; set; }                    // For sub-tasks
    public Guid? CreatedByUserId { get; set; }                   // User who created the ticket
    public Guid? AssignedToUserId { get; set; }                  // User assigned to work on ticket
    public string? CurrentHandoffSummary { get; set; }           // Quick context

    // Auto-generated summaries for efficient context retrieval
    public string? ProgressSummary { get; set; }                   // Summary of progress entries
    public string? DecisionSummary { get; set; }                   // Summary of decisions made
    public string? OutcomeStatistics { get; set; }                 // JSON: {"Success":5,"Failure":1,...}
    public DateTime? SummaryUpdatedAt { get; set; }                // Last summary update timestamp

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation
    public Project Project { get; set; } = null!;
    public Ticket? ParentTicket { get; set; }
    public ICollection<Ticket> SubTickets { get; set; } = new List<Ticket>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<TicketHistory> History { get; set; } = new List<TicketHistory>();
    public ICollection<PlanningSession> PlanningSessions { get; set; } = new List<PlanningSession>();
    public ICollection<ReasoningLog> ReasoningLogs { get; set; } = new List<ReasoningLog>();
    public ICollection<ProgressEntry> ProgressEntries { get; set; } = new List<ProgressEntry>();
    public ICollection<HandoffDocument> Handoffs { get; set; } = new List<HandoffDocument>();
    public ICollection<ImplementationPlan> ImplementationPlans { get; set; } = new List<ImplementationPlan>();
    public ICollection<EffortEstimation> EffortEstimations { get; set; } = new List<EffortEstimation>();
    public ICollection<FileChange> FileChanges { get; set; } = new List<FileChange>();
    public ICollection<TestLink> TestLinks { get; set; } = new List<TestLink>();
    public ICollection<TechnicalDebt> OriginatedDebts { get; set; } = new List<TechnicalDebt>();
    public ICollection<TechnicalDebt> ResolvedDebts { get; set; } = new List<TechnicalDebt>();
    public ICollection<SkillChainExecution> ChainExecutions { get; set; } = new List<SkillChainExecution>();
}
