using AiForge.Application.DTOs.Plans;
using AiForge.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace AiForge.Api.Controllers;

[ApiController]
[Route("api")]
public class ImplementationPlansController : ControllerBase
{
    private readonly IImplementationPlanService _planService;

    public ImplementationPlansController(IImplementationPlanService planService)
    {
        _planService = planService;
    }

    #region Ticket-scoped endpoints

    /// <summary>
    /// Get all implementation plans for a ticket
    /// </summary>
    [HttpGet("tickets/{ticketId:guid}/plans")]
    [ProducesResponseType(typeof(IEnumerable<ImplementationPlanDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ImplementationPlanDto>>> GetByTicket(
        Guid ticketId,
        CancellationToken cancellationToken)
    {
        var plans = await _planService.GetByTicketIdAsync(ticketId, cancellationToken);
        return Ok(plans);
    }

    /// <summary>
    /// Get the current implementation plan for a ticket (latest approved or draft)
    /// </summary>
    [HttpGet("tickets/{ticketId:guid}/plans/current")]
    [ProducesResponseType(typeof(ImplementationPlanDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ImplementationPlanDto>> GetCurrentByTicket(
        Guid ticketId,
        CancellationToken cancellationToken)
    {
        var plan = await _planService.GetCurrentByTicketIdAsync(ticketId, cancellationToken);
        if (plan == null)
            return NotFound(new { error = $"No implementation plan found for ticket '{ticketId}'" });

        return Ok(plan);
    }

    /// <summary>
    /// Get the approved implementation plan for a ticket
    /// </summary>
    [HttpGet("tickets/{ticketId:guid}/plans/approved")]
    [ProducesResponseType(typeof(ImplementationPlanDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ImplementationPlanDto>> GetApprovedByTicket(
        Guid ticketId,
        CancellationToken cancellationToken)
    {
        var plan = await _planService.GetApprovedByTicketIdAsync(ticketId, cancellationToken);
        if (plan == null)
            return NotFound(new { error = $"No approved implementation plan found for ticket '{ticketId}'" });

        return Ok(plan);
    }

    /// <summary>
    /// Create a new implementation plan for a ticket
    /// </summary>
    [HttpPost("tickets/{ticketId:guid}/plans")]
    [ProducesResponseType(typeof(ImplementationPlanDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ImplementationPlanDto>> Create(
        Guid ticketId,
        [FromBody] CreateImplementationPlanRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Content))
            return BadRequest(new { error = "Content is required" });

        try
        {
            var plan = await _planService.CreateAsync(ticketId, request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = plan.Id }, plan);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    #endregion

    #region Plan-scoped endpoints

    /// <summary>
    /// Get an implementation plan by ID
    /// </summary>
    [HttpGet("plans/{id:guid}")]
    [ProducesResponseType(typeof(ImplementationPlanDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ImplementationPlanDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var plan = await _planService.GetByIdAsync(id, cancellationToken);
        if (plan == null)
            return NotFound(new { error = $"Implementation plan with ID '{id}' not found" });

        return Ok(plan);
    }

    /// <summary>
    /// Update a draft implementation plan
    /// </summary>
    [HttpPut("plans/{id:guid}")]
    [ProducesResponseType(typeof(ImplementationPlanDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ImplementationPlanDto>> Update(
        Guid id,
        [FromBody] UpdateImplementationPlanRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var plan = await _planService.UpdateAsync(id, request, cancellationToken);
            if (plan == null)
                return NotFound(new { error = $"Implementation plan with ID '{id}' not found" });

            return Ok(plan);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Delete a draft implementation plan
    /// </summary>
    [HttpDelete("plans/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var deleted = await _planService.DeleteAsync(id, cancellationToken);
            if (!deleted)
                return NotFound(new { error = $"Implementation plan with ID '{id}' not found" });

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Approve a draft implementation plan
    /// </summary>
    [HttpPost("plans/{id:guid}/approve")]
    [ProducesResponseType(typeof(ImplementationPlanDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ImplementationPlanDto>> Approve(
        Guid id,
        [FromBody] ApproveImplementationPlanRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var plan = await _planService.ApproveAsync(id, request, cancellationToken);
            if (plan == null)
                return NotFound(new { error = $"Implementation plan with ID '{id}' not found" });

            return Ok(plan);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Reject a draft implementation plan
    /// </summary>
    [HttpPost("plans/{id:guid}/reject")]
    [ProducesResponseType(typeof(ImplementationPlanDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ImplementationPlanDto>> Reject(
        Guid id,
        [FromBody] RejectImplementationPlanRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var plan = await _planService.RejectAsync(id, request, cancellationToken);
            if (plan == null)
                return NotFound(new { error = $"Implementation plan with ID '{id}' not found" });

            return Ok(plan);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Supersede an approved plan with a new version
    /// </summary>
    [HttpPost("plans/{id:guid}/supersede")]
    [ProducesResponseType(typeof(ImplementationPlanDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ImplementationPlanDto>> Supersede(
        Guid id,
        [FromBody] SupersedeImplementationPlanRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Content))
            return BadRequest(new { error = "Content is required for the new plan" });

        try
        {
            var newPlan = await _planService.SupersedeAsync(id, request, cancellationToken);
            if (newPlan == null)
                return NotFound(new { error = $"Implementation plan with ID '{id}' not found" });

            return CreatedAtAction(nameof(GetById), new { id = newPlan.Id }, newPlan);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    #endregion
}
