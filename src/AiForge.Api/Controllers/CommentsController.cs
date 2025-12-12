using AiForge.Application.DTOs.Comments;
using AiForge.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace AiForge.Api.Controllers;

[ApiController]
[Route("api/tickets/{ticketId:guid}/comments")]
public class CommentsController : ControllerBase
{
    private readonly ICommentService _commentService;

    public CommentsController(ICommentService commentService)
    {
        _commentService = commentService;
    }

    /// <summary>
    /// Get all comments for a ticket
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CommentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CommentDto>>> GetByTicketId(Guid ticketId, CancellationToken cancellationToken)
    {
        var comments = await _commentService.GetByTicketIdAsync(ticketId, cancellationToken);
        return Ok(comments);
    }

    /// <summary>
    /// Add a comment to a ticket
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CommentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CommentDto>> Create(
        Guid ticketId,
        [FromBody] CreateCommentRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Content))
            return BadRequest(new { error = "Comment content is required" });

        try
        {
            var comment = await _commentService.CreateAsync(ticketId, request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { ticketId, id = comment.Id }, comment);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get a specific comment
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CommentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CommentDto>> GetById(Guid ticketId, Guid id, CancellationToken cancellationToken)
    {
        var comment = await _commentService.GetByIdAsync(id, cancellationToken);
        if (comment == null || comment.TicketId != ticketId)
            return NotFound(new { error = $"Comment with ID '{id}' not found" });

        return Ok(comment);
    }
}

/// <summary>
/// Root-level comments controller for update/delete operations
/// </summary>
[ApiController]
[Route("api/comments")]
public class CommentsManagementController : ControllerBase
{
    private readonly ICommentService _commentService;

    public CommentsManagementController(ICommentService commentService)
    {
        _commentService = commentService;
    }

    /// <summary>
    /// Update a comment
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(CommentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CommentDto>> Update(
        Guid id,
        [FromBody] UpdateCommentRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Content))
            return BadRequest(new { error = "Comment content is required" });

        var comment = await _commentService.UpdateAsync(id, request, cancellationToken);
        if (comment == null)
            return NotFound(new { error = $"Comment with ID '{id}' not found" });

        return Ok(comment);
    }

    /// <summary>
    /// Delete a comment
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _commentService.DeleteAsync(id, cancellationToken);
        if (!deleted)
            return NotFound(new { error = $"Comment with ID '{id}' not found" });

        return NoContent();
    }
}
