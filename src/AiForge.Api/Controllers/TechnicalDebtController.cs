using AiForge.Application.DTOs.TechnicalDebt;
using AiForge.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace AiForge.Api.Controllers;

[ApiController]
public class TechnicalDebtController : ControllerBase
{
    private readonly ITechnicalDebtService _debtService;

    public TechnicalDebtController(ITechnicalDebtService debtService)
    {
        _debtService = debtService;
    }

    /// <summary>
    /// Flag technical debt for a ticket
    /// </summary>
    [HttpPost("api/tickets/{ticketId:guid}/debt")]
    [ProducesResponseType(typeof(TechnicalDebtDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TechnicalDebtDto>> FlagDebt(
        Guid ticketId,
        [FromBody] CreateDebtRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var debt = await _debtService.FlagDebtAsync(ticketId, request, cancellationToken);
            return CreatedAtAction(nameof(GetDebtById), new { id = debt.Id }, debt);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get technical debt backlog with optional filters
    /// </summary>
    [HttpGet("api/debt")]
    [ProducesResponseType(typeof(DebtBacklogResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<DebtBacklogResponse>> GetDebtBacklog(
        [FromQuery] string? status,
        [FromQuery] string? category,
        [FromQuery] string? severity,
        CancellationToken cancellationToken)
    {
        var backlog = await _debtService.GetDebtBacklogAsync(status, category, severity, cancellationToken);
        return Ok(backlog);
    }

    /// <summary>
    /// Get a specific debt item by ID
    /// </summary>
    [HttpGet("api/debt/{id:guid}")]
    [ProducesResponseType(typeof(TechnicalDebtDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TechnicalDebtDto>> GetDebtById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var debt = await _debtService.GetDebtByIdAsync(id, cancellationToken);
        if (debt == null)
            return NotFound(new { error = $"Technical debt with ID '{id}' not found" });

        return Ok(debt);
    }

    /// <summary>
    /// Update a debt item
    /// </summary>
    [HttpPatch("api/debt/{id:guid}")]
    [ProducesResponseType(typeof(TechnicalDebtDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TechnicalDebtDto>> UpdateDebt(
        Guid id,
        [FromBody] UpdateDebtRequest request,
        CancellationToken cancellationToken)
    {
        var debt = await _debtService.UpdateDebtAsync(id, request, cancellationToken);
        if (debt == null)
            return NotFound(new { error = $"Technical debt with ID '{id}' not found" });

        return Ok(debt);
    }

    /// <summary>
    /// Resolve a debt item
    /// </summary>
    [HttpPost("api/debt/{id:guid}/resolve")]
    [ProducesResponseType(typeof(TechnicalDebtDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TechnicalDebtDto>> ResolveDebt(
        Guid id,
        [FromBody] ResolveDebtRequest request,
        CancellationToken cancellationToken)
    {
        var debt = await _debtService.ResolveDebtAsync(id, request, cancellationToken);
        if (debt == null)
            return NotFound(new { error = $"Technical debt with ID '{id}' not found" });

        return Ok(debt);
    }

    /// <summary>
    /// Get technical debt summary (counts by category and severity)
    /// </summary>
    [HttpGet("api/debt/summary")]
    [ProducesResponseType(typeof(DebtSummaryResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<DebtSummaryResponse>> GetDebtSummary(
        CancellationToken cancellationToken)
    {
        var summary = await _debtService.GetDebtSummaryAsync(cancellationToken);
        return Ok(summary);
    }

    /// <summary>
    /// Get technical debt for a specific ticket
    /// </summary>
    [HttpGet("api/tickets/{ticketId:guid}/debt")]
    [ProducesResponseType(typeof(IEnumerable<TechnicalDebtDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TechnicalDebtDto>>> GetDebtByTicket(
        Guid ticketId,
        CancellationToken cancellationToken)
    {
        var debts = await _debtService.GetDebtByTicketAsync(ticketId, cancellationToken);
        return Ok(debts);
    }
}
