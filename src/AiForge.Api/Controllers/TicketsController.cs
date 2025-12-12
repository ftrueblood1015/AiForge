using AiForge.Application.DTOs.Tickets;
using AiForge.Application.Services;
using AiForge.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace AiForge.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TicketsController : ControllerBase
{
    private readonly ITicketService _ticketService;
    private readonly ITicketHistoryService _historyService;

    public TicketsController(ITicketService ticketService, ITicketHistoryService historyService)
    {
        _ticketService = ticketService;
        _historyService = historyService;
    }

    /// <summary>
    /// Search tickets with optional filters
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TicketDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TicketDto>>> Search(
        [FromQuery] Guid? projectId,
        [FromQuery] TicketStatus? status,
        [FromQuery] TicketType? type,
        [FromQuery] Priority? priority,
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        var request = new TicketSearchRequest
        {
            ProjectId = projectId,
            Status = status,
            Type = type,
            Priority = priority,
            Search = search
        };

        var tickets = await _ticketService.SearchAsync(request, cancellationToken);
        return Ok(tickets);
    }

    /// <summary>
    /// Get ticket by ID with full details
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TicketDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TicketDetailDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var ticket = await _ticketService.GetByIdAsync(id, cancellationToken);
        if (ticket == null)
            return NotFound(new { error = $"Ticket with ID '{id}' not found" });

        return Ok(ticket);
    }

    /// <summary>
    /// Get ticket by key (e.g., "AIFORGE-123")
    /// </summary>
    [HttpGet("key/{key}")]
    [ProducesResponseType(typeof(TicketDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TicketDetailDto>> GetByKey(string key, CancellationToken cancellationToken)
    {
        var ticket = await _ticketService.GetByKeyAsync(key.ToUpperInvariant(), cancellationToken);
        if (ticket == null)
            return NotFound(new { error = $"Ticket with key '{key}' not found" });

        return Ok(ticket);
    }

    /// <summary>
    /// Create a new ticket
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(TicketDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TicketDto>> Create([FromBody] CreateTicketRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.ProjectKey))
            return BadRequest(new { error = "Project key is required" });

        if (string.IsNullOrWhiteSpace(request.Title))
            return BadRequest(new { error = "Title is required" });

        try
        {
            var ticket = await _ticketService.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = ticket.Id }, ticket);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Update a ticket
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(TicketDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TicketDto>> Update(
        Guid id,
        [FromBody] UpdateTicketRequest request,
        [FromQuery] string? changedBy,
        CancellationToken cancellationToken)
    {
        var ticket = await _ticketService.UpdateAsync(id, request, changedBy, cancellationToken);
        if (ticket == null)
            return NotFound(new { error = $"Ticket with ID '{id}' not found" });

        return Ok(ticket);
    }

    /// <summary>
    /// Transition ticket status
    /// </summary>
    [HttpPost("{id:guid}/transition")]
    [ProducesResponseType(typeof(TicketDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TicketDto>> Transition(
        Guid id,
        [FromBody] TransitionTicketRequest request,
        [FromQuery] string? changedBy,
        CancellationToken cancellationToken)
    {
        var ticket = await _ticketService.TransitionAsync(id, request, changedBy, cancellationToken);
        if (ticket == null)
            return NotFound(new { error = $"Ticket with ID '{id}' not found" });

        return Ok(ticket);
    }

    /// <summary>
    /// Delete a ticket
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _ticketService.DeleteAsync(id, cancellationToken);
        if (!deleted)
            return NotFound(new { error = $"Ticket with ID '{id}' not found" });

        return NoContent();
    }

    /// <summary>
    /// Get ticket change history
    /// </summary>
    [HttpGet("{id:guid}/history")]
    [ProducesResponseType(typeof(IEnumerable<TicketHistoryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TicketHistoryDto>>> GetHistory(Guid id, CancellationToken cancellationToken)
    {
        var history = await _historyService.GetByTicketIdAsync(id, cancellationToken);
        return Ok(history);
    }
}
