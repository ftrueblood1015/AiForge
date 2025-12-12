using AiForge.Application.DTOs.Handoffs;
using AiForge.Application.Services;
using AiForge.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace AiForge.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HandoffsController : ControllerBase
{
    private readonly IHandoffService _handoffService;

    public HandoffsController(IHandoffService handoffService)
    {
        _handoffService = handoffService;
    }

    /// <summary>
    /// Search handoffs with optional filters
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<HandoffDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<HandoffDto>>> Search(
        [FromQuery] Guid? ticketId,
        [FromQuery] HandoffType? type,
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        var handoffs = await _handoffService.SearchAsync(ticketId, type, search, cancellationToken);
        return Ok(handoffs);
    }

    /// <summary>
    /// Get handoff by ID with full details
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(HandoffDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<HandoffDetailDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var handoff = await _handoffService.GetByIdAsync(id, cancellationToken);
        if (handoff == null)
            return NotFound(new { error = $"Handoff with ID '{id}' not found" });

        return Ok(handoff);
    }

    /// <summary>
    /// Get the latest active handoff for a ticket
    /// </summary>
    [HttpGet("ticket/{ticketId:guid}/latest")]
    [ProducesResponseType(typeof(HandoffDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<HandoffDetailDto>> GetLatestByTicket(Guid ticketId, CancellationToken cancellationToken)
    {
        var handoff = await _handoffService.GetLatestActiveByTicketIdAsync(ticketId, cancellationToken);
        if (handoff == null)
            return NotFound(new { error = $"No active handoff found for ticket '{ticketId}'" });

        return Ok(handoff);
    }

    /// <summary>
    /// Get all handoffs for a ticket
    /// </summary>
    [HttpGet("ticket/{ticketId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<HandoffDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<HandoffDto>>> GetByTicket(Guid ticketId, CancellationToken cancellationToken)
    {
        var handoffs = await _handoffService.GetByTicketIdAsync(ticketId, cancellationToken);
        return Ok(handoffs);
    }

    /// <summary>
    /// Create a new handoff document
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(HandoffDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<HandoffDetailDto>> Create(
        [FromBody] CreateHandoffRequest request,
        CancellationToken cancellationToken)
    {
        if (request.TicketId == Guid.Empty)
            return BadRequest(new { error = "TicketId is required" });

        if (string.IsNullOrWhiteSpace(request.Title))
            return BadRequest(new { error = "Title is required" });

        if (string.IsNullOrWhiteSpace(request.Summary))
            return BadRequest(new { error = "Summary is required" });

        try
        {
            var handoff = await _handoffService.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = handoff.Id }, handoff);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Update a handoff document
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(HandoffDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<HandoffDetailDto>> Update(
        Guid id,
        [FromBody] UpdateHandoffRequest request,
        CancellationToken cancellationToken)
    {
        var handoff = await _handoffService.UpdateAsync(id, request, cancellationToken);
        if (handoff == null)
            return NotFound(new { error = $"Handoff with ID '{id}' not found" });

        return Ok(handoff);
    }

    #region File Snapshots

    /// <summary>
    /// Get file snapshots for a handoff
    /// </summary>
    [HttpGet("{handoffId:guid}/snapshots")]
    [ProducesResponseType(typeof(IEnumerable<FileSnapshotDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<FileSnapshotDto>>> GetFileSnapshots(
        Guid handoffId,
        CancellationToken cancellationToken)
    {
        var snapshots = await _handoffService.GetFileSnapshotsAsync(handoffId, cancellationToken);
        return Ok(snapshots);
    }

    /// <summary>
    /// Add a file snapshot to a handoff
    /// </summary>
    [HttpPost("{handoffId:guid}/snapshots")]
    [ProducesResponseType(typeof(FileSnapshotDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<FileSnapshotDto>> AddFileSnapshot(
        Guid handoffId,
        [FromBody] CreateFileSnapshotRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.FilePath))
            return BadRequest(new { error = "FilePath is required" });

        try
        {
            var snapshot = await _handoffService.AddFileSnapshotAsync(handoffId, request, cancellationToken);
            return StatusCode(StatusCodes.Status201Created, snapshot);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    #endregion
}
