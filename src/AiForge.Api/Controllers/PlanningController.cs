using AiForge.Application.DTOs.Planning;
using AiForge.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace AiForge.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlanningController : ControllerBase
{
    private readonly IPlanningService _planningService;

    public PlanningController(IPlanningService planningService)
    {
        _planningService = planningService;
    }

    #region Planning Sessions

    /// <summary>
    /// Get planning session by ID
    /// </summary>
    [HttpGet("sessions/{id:guid}")]
    [ProducesResponseType(typeof(PlanningSessionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PlanningSessionDto>> GetSession(Guid id, CancellationToken cancellationToken)
    {
        var session = await _planningService.GetSessionByIdAsync(id, cancellationToken);
        if (session == null)
            return NotFound(new { error = $"Planning session with ID '{id}' not found" });

        return Ok(session);
    }

    /// <summary>
    /// Get all planning sessions for a ticket
    /// </summary>
    [HttpGet("sessions")]
    [ProducesResponseType(typeof(IEnumerable<PlanningSessionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PlanningSessionDto>>> GetSessionsByTicket(
        [FromQuery] Guid ticketId,
        CancellationToken cancellationToken)
    {
        var sessions = await _planningService.GetSessionsByTicketIdAsync(ticketId, cancellationToken);
        return Ok(sessions);
    }

    /// <summary>
    /// Create a new planning session
    /// </summary>
    [HttpPost("sessions")]
    [ProducesResponseType(typeof(PlanningSessionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PlanningSessionDto>> CreateSession(
        [FromBody] CreatePlanningSessionRequest request,
        CancellationToken cancellationToken)
    {
        if (request.TicketId == Guid.Empty)
            return BadRequest(new { error = "TicketId is required" });

        try
        {
            var session = await _planningService.CreateSessionAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetSession), new { id = session.Id }, session);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Update a planning session
    /// </summary>
    [HttpPut("sessions/{id:guid}")]
    [ProducesResponseType(typeof(PlanningSessionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PlanningSessionDto>> UpdateSession(
        Guid id,
        [FromBody] UpdatePlanningSessionRequest request,
        CancellationToken cancellationToken)
    {
        var session = await _planningService.UpdateSessionAsync(id, request, cancellationToken);
        if (session == null)
            return NotFound(new { error = $"Planning session with ID '{id}' not found" });

        return Ok(session);
    }

    /// <summary>
    /// Complete a planning session
    /// </summary>
    [HttpPost("sessions/{id:guid}/complete")]
    [ProducesResponseType(typeof(PlanningSessionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PlanningSessionDto>> CompleteSession(
        Guid id,
        [FromBody] CompletePlanningSessionRequest request,
        CancellationToken cancellationToken)
    {
        var session = await _planningService.CompleteSessionAsync(id, request, cancellationToken);
        if (session == null)
            return NotFound(new { error = $"Planning session with ID '{id}' not found" });

        return Ok(session);
    }

    #endregion

    #region Reasoning Logs

    /// <summary>
    /// Get reasoning logs for a ticket
    /// </summary>
    [HttpGet("reasoning")]
    [ProducesResponseType(typeof(IEnumerable<ReasoningLogDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ReasoningLogDto>>> GetReasoningLogs(
        [FromQuery] Guid ticketId,
        [FromQuery] int? limit,
        CancellationToken cancellationToken)
    {
        var logs = await _planningService.GetReasoningLogsByTicketIdAsync(ticketId, limit, cancellationToken);
        return Ok(logs);
    }

    /// <summary>
    /// Create a reasoning log entry
    /// </summary>
    [HttpPost("reasoning")]
    [ProducesResponseType(typeof(ReasoningLogDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ReasoningLogDto>> CreateReasoningLog(
        [FromBody] CreateReasoningLogRequest request,
        CancellationToken cancellationToken)
    {
        if (request.TicketId == Guid.Empty)
            return BadRequest(new { error = "TicketId is required" });

        if (string.IsNullOrWhiteSpace(request.DecisionPoint))
            return BadRequest(new { error = "DecisionPoint is required" });

        try
        {
            var log = await _planningService.CreateReasoningLogAsync(request, cancellationToken);
            return StatusCode(StatusCodes.Status201Created, log);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    #endregion

    #region Progress Entries

    /// <summary>
    /// Get progress entries for a ticket
    /// </summary>
    [HttpGet("progress")]
    [ProducesResponseType(typeof(IEnumerable<ProgressEntryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ProgressEntryDto>>> GetProgressEntries(
        [FromQuery] Guid ticketId,
        [FromQuery] int? limit,
        CancellationToken cancellationToken)
    {
        var entries = await _planningService.GetProgressEntriesByTicketIdAsync(ticketId, limit, cancellationToken);
        return Ok(entries);
    }

    /// <summary>
    /// Create a progress entry
    /// </summary>
    [HttpPost("progress")]
    [ProducesResponseType(typeof(ProgressEntryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProgressEntryDto>> CreateProgressEntry(
        [FromBody] CreateProgressEntryRequest request,
        CancellationToken cancellationToken)
    {
        if (request.TicketId == Guid.Empty)
            return BadRequest(new { error = "TicketId is required" });

        if (string.IsNullOrWhiteSpace(request.Content))
            return BadRequest(new { error = "Content is required" });

        try
        {
            var entry = await _planningService.CreateProgressEntryAsync(request, cancellationToken);
            return StatusCode(StatusCodes.Status201Created, entry);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    #endregion

    #region Aggregated Data

    /// <summary>
    /// Get all planning data for a ticket (sessions, reasoning, progress)
    /// </summary>
    [HttpGet("data")]
    [ProducesResponseType(typeof(PlanningDataDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<PlanningDataDto>> GetPlanningData(
        [FromQuery] Guid ticketId,
        CancellationToken cancellationToken)
    {
        var data = await _planningService.GetPlanningDataByTicketIdAsync(ticketId, cancellationToken);
        return Ok(data);
    }

    #endregion
}
