using AiForge.Application.DTOs.Estimation;
using AiForge.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace AiForge.Api.Controllers;

[ApiController]
[Route("api/tickets/{ticketId:guid}/estimation")]
public class EffortEstimationController : ControllerBase
{
    private readonly IEffortEstimationService _estimationService;

    public EffortEstimationController(IEffortEstimationService estimationService)
    {
        _estimationService = estimationService;
    }

    /// <summary>
    /// Get the latest effort estimation for a ticket
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(EffortEstimationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EffortEstimationDto>> GetLatestEstimation(
        Guid ticketId,
        CancellationToken cancellationToken)
    {
        var estimation = await _estimationService.GetLatestEstimationAsync(ticketId, cancellationToken);
        if (estimation == null)
            return NotFound(new { error = $"No estimation found for ticket '{ticketId}'" });

        return Ok(estimation);
    }

    /// <summary>
    /// Get estimation history for a ticket (all versions)
    /// </summary>
    [HttpGet("history")]
    [ProducesResponseType(typeof(EstimationHistoryResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<EstimationHistoryResponse>> GetEstimationHistory(
        Guid ticketId,
        CancellationToken cancellationToken)
    {
        var history = await _estimationService.GetEstimationHistoryAsync(ticketId, cancellationToken);
        return Ok(history);
    }

    /// <summary>
    /// Create an initial effort estimation for a ticket
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(EffortEstimationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<EffortEstimationDto>> CreateEstimation(
        Guid ticketId,
        [FromBody] CreateEstimationRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var estimation = await _estimationService.CreateEstimationAsync(ticketId, request, cancellationToken);
            return CreatedAtAction(nameof(GetLatestEstimation), new { ticketId }, estimation);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
        {
            return Conflict(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Revise an effort estimation (creates a new version)
    /// </summary>
    [HttpPut]
    [ProducesResponseType(typeof(EffortEstimationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<EffortEstimationDto>> ReviseEstimation(
        Guid ticketId,
        [FromBody] ReviseEstimationRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var estimation = await _estimationService.ReviseEstimationAsync(ticketId, request, cancellationToken);
            return Ok(estimation);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Record actual effort when ticket is completed
    /// </summary>
    [HttpPost("actual")]
    [ProducesResponseType(typeof(EffortEstimationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<EffortEstimationDto>> RecordActualEffort(
        Guid ticketId,
        [FromBody] RecordActualEffortRequest request,
        CancellationToken cancellationToken)
    {
        var estimation = await _estimationService.RecordActualEffortAsync(ticketId, request, cancellationToken);
        if (estimation == null)
            return NotFound(new { error = $"No estimation found for ticket '{ticketId}'. Create an estimation first." });

        return Ok(estimation);
    }
}
