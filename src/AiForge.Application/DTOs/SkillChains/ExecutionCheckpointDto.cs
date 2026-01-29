namespace AiForge.Application.DTOs.SkillChains;

public class ExecutionCheckpointDto
{
    public Guid Id { get; set; }
    public Guid ExecutionId { get; set; }
    public Guid LinkId { get; set; }
    public string? LinkName { get; set; }
    public int Position { get; set; }
    public string CheckpointData { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
