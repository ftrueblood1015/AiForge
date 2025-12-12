using AiForge.Application.DTOs.Planning;
using AiForge.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace AiForge.Api.Controllers;

/// <summary>
/// AI Context endpoints for Claude integration
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AiController : ControllerBase
{
    private readonly IAiContextService _contextService;

    public AiController(IAiContextService contextService)
    {
        _contextService = contextService;
    }

    /// <summary>
    /// Get aggregated context for Claude to resume work on a ticket.
    /// Returns ticket details, latest handoff, recent reasoning logs, and recent progress entries.
    /// </summary>
    [HttpGet("context/{ticketId:guid}")]
    [ProducesResponseType(typeof(AiContextDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AiContextDto>> GetContext(Guid ticketId, CancellationToken cancellationToken)
    {
        var context = await _contextService.GetContextByTicketIdAsync(ticketId, cancellationToken);
        if (context == null)
            return NotFound(new { error = $"Ticket with ID '{ticketId}' not found" });

        return Ok(context);
    }

    /// <summary>
    /// Start a new Claude session for a ticket.
    /// This can be used to track when Claude begins working on a ticket.
    /// </summary>
    [HttpPost("session/start")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> StartSession(
        [FromBody] StartSessionRequest request,
        CancellationToken cancellationToken)
    {
        if (request.TicketId == Guid.Empty)
            return BadRequest(new { error = "TicketId is required" });

        if (string.IsNullOrWhiteSpace(request.SessionId))
            return BadRequest(new { error = "SessionId is required" });

        await _contextService.StartSessionAsync(request, cancellationToken);
        return Ok(new { message = "Session started", ticketId = request.TicketId, sessionId = request.SessionId });
    }

    /// <summary>
    /// End a Claude session.
    /// This can be used to track when Claude finishes working on a ticket.
    /// </summary>
    [HttpPost("session/end")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> EndSession(
        [FromBody] EndSessionRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.SessionId))
            return BadRequest(new { error = "SessionId is required" });

        await _contextService.EndSessionAsync(request, cancellationToken);
        return Ok(new { message = "Session ended", sessionId = request.SessionId });
    }
}
