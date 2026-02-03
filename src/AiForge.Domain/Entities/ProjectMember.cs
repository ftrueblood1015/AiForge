using AiForge.Domain.Enums;

namespace AiForge.Domain.Entities;

public class ProjectMember
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Guid UserId { get; set; }
    public ProjectRole Role { get; set; }
    public DateTime AddedAt { get; set; }
    public Guid? AddedByUserId { get; set; }

    // Navigation
    public Project Project { get; set; } = null!;
}
