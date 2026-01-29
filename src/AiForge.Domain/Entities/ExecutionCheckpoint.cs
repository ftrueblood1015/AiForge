namespace AiForge.Domain.Entities;

/// <summary>
/// Stores checkpoint data after each successful skill chain link completion.
/// Enables efficient resume without full context reload.
/// </summary>
public class ExecutionCheckpoint
{
    public Guid Id { get; set; }
    public Guid ExecutionId { get; set; }
    public Guid LinkId { get; set; }
    public int Position { get; set; }                          // Link position at checkpoint time
    public string CheckpointData { get; set; } = string.Empty; // JSON: key outputs from this step
    public DateTime CreatedAt { get; set; }

    // Navigation
    public SkillChainExecution Execution { get; set; } = null!;
    public SkillChainLink Link { get; set; } = null!;
}
