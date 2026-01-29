using AiForge.Application.DTOs.SkillChains;
using AiForge.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace AiForge.Api.Controllers;

[ApiController]
[Route("api/skillchain-executions")]
public class SkillChainExecutionsController : ControllerBase
{
    private readonly ISkillChainExecutionService _executionService;

    public SkillChainExecutionsController(ISkillChainExecutionService executionService)
    {
        _executionService = executionService;
    }

    /// <summary>
    /// List chain executions with optional filters
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<SkillChainExecutionSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<SkillChainExecutionSummaryDto>>> GetExecutions(
        [FromQuery] Guid? chainId,
        [FromQuery] Guid? ticketId,
        [FromQuery] string? status,
        CancellationToken cancellationToken)
    {
        var executions = await _executionService.GetExecutionsAsync(chainId, ticketId, status, cancellationToken);
        return Ok(executions);
    }

    /// <summary>
    /// Get execution by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(SkillChainExecutionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SkillChainExecutionDto>> GetExecution(
        Guid id,
        CancellationToken cancellationToken)
    {
        var execution = await _executionService.GetExecutionAsync(id, cancellationToken);
        if (execution == null)
            return NotFound(new { error = $"Execution with ID '{id}' not found" });

        return Ok(execution);
    }

    /// <summary>
    /// Start a new chain execution
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(SkillChainExecutionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SkillChainExecutionDto>> StartExecution(
        [FromBody] StartChainExecutionRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            request.StartedBy ??= HttpContext.Items["ApiKeyId"]?.ToString() ?? "system";
            var execution = await _executionService.StartExecutionAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetExecution), new { id = execution.Id }, execution);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Pause a running execution
    /// </summary>
    [HttpPost("{id:guid}/pause")]
    [ProducesResponseType(typeof(SkillChainExecutionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SkillChainExecutionDto>> PauseExecution(
        Guid id,
        [FromBody] PauseRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var pausedBy = HttpContext.Items["ApiKeyId"]?.ToString() ?? "system";
            var execution = await _executionService.PauseExecutionAsync(id, request.Reason, pausedBy, cancellationToken);
            if (execution == null)
                return NotFound(new { error = $"Execution with ID '{id}' not found" });

            return Ok(execution);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Resume a paused execution
    /// </summary>
    [HttpPost("{id:guid}/resume")]
    [ProducesResponseType(typeof(SkillChainExecutionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SkillChainExecutionDto>> ResumeExecution(
        Guid id,
        [FromBody] ResumeExecutionRequest? request,
        CancellationToken cancellationToken)
    {
        try
        {
            request ??= new ResumeExecutionRequest();
            request.ResumedBy ??= HttpContext.Items["ApiKeyId"]?.ToString() ?? "system";
            var execution = await _executionService.ResumeExecutionAsync(id, request, cancellationToken);
            if (execution == null)
                return NotFound(new { error = $"Execution with ID '{id}' not found" });

            return Ok(execution);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Cancel an execution
    /// </summary>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(typeof(SkillChainExecutionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SkillChainExecutionDto>> CancelExecution(
        Guid id,
        [FromBody] CancelRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var cancelledBy = HttpContext.Items["ApiKeyId"]?.ToString() ?? "system";
            var execution = await _executionService.CancelExecutionAsync(id, request.Reason, cancelledBy, cancellationToken);
            if (execution == null)
                return NotFound(new { error = $"Execution with ID '{id}' not found" });

            return Ok(execution);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Record the outcome of a link execution
    /// </summary>
    [HttpPost("{id:guid}/record-outcome")]
    [ProducesResponseType(typeof(SkillChainLinkExecutionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SkillChainLinkExecutionDto>> RecordLinkOutcome(
        Guid id,
        [FromBody] RecordLinkOutcomeRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            request.ExecutedBy ??= HttpContext.Items["ApiKeyId"]?.ToString() ?? "system";
            var linkExec = await _executionService.RecordLinkOutcomeAsync(id, request, cancellationToken);
            return Ok(linkExec);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Advance execution to the next link based on the last outcome
    /// </summary>
    [HttpPost("{id:guid}/advance")]
    [ProducesResponseType(typeof(SkillChainExecutionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SkillChainExecutionDto>> AdvanceExecution(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var execution = await _executionService.AdvanceExecutionAsync(id, cancellationToken);
            if (execution == null)
                return NotFound(new { error = $"Execution with ID '{id}' not found" });

            return Ok(execution);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get executions requiring human intervention
    /// </summary>
    [HttpGet("pending-interventions")]
    [ProducesResponseType(typeof(IEnumerable<SkillChainExecutionSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<SkillChainExecutionSummaryDto>>> GetPendingInterventions(
        [FromQuery] Guid? projectId,
        CancellationToken cancellationToken)
    {
        var executions = await _executionService.GetPendingInterventionsAsync(projectId, cancellationToken);
        return Ok(executions);
    }

    /// <summary>
    /// Resolve a human intervention
    /// </summary>
    [HttpPost("{id:guid}/resolve")]
    [ProducesResponseType(typeof(SkillChainExecutionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SkillChainExecutionDto>> ResolveIntervention(
        Guid id,
        [FromBody] ResolveInterventionRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            request.ResolvedBy ??= HttpContext.Items["ApiKeyId"]?.ToString() ?? "system";
            var execution = await _executionService.ResolveInterventionAsync(id, request, cancellationToken);
            if (execution == null)
                return NotFound(new { error = $"Execution with ID '{id}' not found" });

            return Ok(execution);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}

// Simple request DTOs for the controller
public class PauseRequest
{
    public string Reason { get; set; } = string.Empty;
}

public class CancelRequest
{
    public string Reason { get; set; } = string.Empty;
}
