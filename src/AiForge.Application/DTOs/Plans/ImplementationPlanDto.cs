namespace AiForge.Application.DTOs.Plans;

public class ImplementationPlanDto
{
    public Guid Id { get; set; }
    public Guid TicketId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int Version { get; set; }
    public string? EstimatedEffort { get; set; }
    public List<string> AffectedFiles { get; set; } = new();
    public List<string> DependencyTicketIds { get; set; } = new();
    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public Guid? SupersededById { get; set; }
    public DateTime? SupersededAt { get; set; }
    public string? RejectedBy { get; set; }
    public DateTime? RejectedAt { get; set; }
    public string? RejectionReason { get; set; }

    public bool IsDraft => Status == "Draft";
    public bool IsApproved => Status == "Approved";
    public bool IsSuperseded => Status == "Superseded";
    public bool IsRejected => Status == "Rejected";
}

public class CreateImplementationPlanRequest
{
    public string Content { get; set; } = string.Empty;
    public string? EstimatedEffort { get; set; }
    public List<string>? AffectedFiles { get; set; }
    public List<string>? DependencyTicketIds { get; set; }
    public string? CreatedBy { get; set; }
}

public class UpdateImplementationPlanRequest
{
    public string? Content { get; set; }
    public string? EstimatedEffort { get; set; }
    public List<string>? AffectedFiles { get; set; }
    public List<string>? DependencyTicketIds { get; set; }
}

public class ApproveImplementationPlanRequest
{
    public string? ApprovedBy { get; set; }
}

public class RejectImplementationPlanRequest
{
    public string? RejectedBy { get; set; }
    public string? RejectionReason { get; set; }
}

public class SupersedeImplementationPlanRequest
{
    public string Content { get; set; } = string.Empty;
    public string? EstimatedEffort { get; set; }
    public List<string>? AffectedFiles { get; set; }
    public List<string>? DependencyTicketIds { get; set; }
    public string? CreatedBy { get; set; }
}
