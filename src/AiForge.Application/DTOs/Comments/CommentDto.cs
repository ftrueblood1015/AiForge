namespace AiForge.Application.DTOs.Comments;

public class CommentDto
{
    public Guid Id { get; set; }
    public Guid TicketId { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsAiGenerated { get; set; }
    public string? SessionId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateCommentRequest
{
    public string Content { get; set; } = string.Empty;
    public bool IsAiGenerated { get; set; }
    public string? SessionId { get; set; }
}

public class UpdateCommentRequest
{
    public string Content { get; set; } = string.Empty;
}
