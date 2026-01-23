using AiForge.Application.DTOs.WorkQueues;
using AiForge.Application.Services;
using AiForge.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace AiForge.Api.Controllers;

[ApiController]
[Route("api/projects/{projectId:guid}/queues")]
public class WorkQueuesController : ControllerBase
{
    private readonly IWorkQueueService _queueService;

    public WorkQueuesController(IWorkQueueService queueService)
    {
        _queueService = queueService;
    }

    /// <summary>
    /// List all work queues for a project
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<WorkQueueDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<WorkQueueDto>>> GetByProject(
        Guid projectId,
        [FromQuery] WorkQueueStatus? status,
        CancellationToken ct)
    {
        var queues = await _queueService.GetByProjectAsync(projectId, status, ct);
        return Ok(queues);
    }

    /// <summary>
    /// Get a work queue by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(WorkQueueDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WorkQueueDetailDto>> GetById(Guid projectId, Guid id, CancellationToken ct)
    {
        var queue = await _queueService.GetByIdAsync(id, ct);
        if (queue == null || queue.ProjectId != projectId)
            return NotFound(new { error = $"Queue {id} not found in project {projectId}" });

        return Ok(queue);
    }

    /// <summary>
    /// Create a new work queue
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(WorkQueueDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<WorkQueueDto>> Create(
        Guid projectId,
        [FromBody] CreateWorkQueueRequest request,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { error = "Queue name is required" });

        try
        {
            var createdBy = User.Identity?.Name ?? "system";
            var queue = await _queueService.CreateAsync(projectId, request, createdBy, ct);
            return CreatedAtAction(nameof(GetById), new { projectId, id = queue.Id }, queue);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Update a work queue
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(WorkQueueDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WorkQueueDto>> Update(
        Guid projectId,
        Guid id,
        [FromBody] UpdateWorkQueueRequest request,
        CancellationToken ct)
    {
        var updatedBy = User.Identity?.Name ?? "system";
        var queue = await _queueService.UpdateAsync(id, request, updatedBy, ct);
        if (queue == null)
            return NotFound(new { error = $"Queue {id} not found" });

        return Ok(queue);
    }

    /// <summary>
    /// Delete a work queue
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid projectId, Guid id, CancellationToken ct)
    {
        var deleted = await _queueService.DeleteAsync(id, ct);
        if (!deleted)
            return NotFound(new { error = $"Queue {id} not found" });

        return NoContent();
    }

    /// <summary>
    /// Checkout a queue for focused work
    /// </summary>
    [HttpPost("{id:guid}/checkout")]
    [ProducesResponseType(typeof(WorkQueueDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WorkQueueDetailDto>> Checkout(
        Guid projectId,
        Guid id,
        [FromBody] CheckoutRequest? request,
        CancellationToken ct)
    {
        try
        {
            var checkedOutBy = User.Identity?.Name ?? "system";
            var (queue, conflict) = await _queueService.CheckoutAsync(id, request ?? new CheckoutRequest(), checkedOutBy, ct);

            if (conflict != null)
            {
                return Conflict(new
                {
                    error = "Queue is already checked out",
                    checkedOutBy = conflict.CheckedOutBy,
                    expiresAt = conflict.ExpiresAt
                });
            }

            return Ok(queue);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Release checkout on a queue
    /// </summary>
    [HttpPost("{id:guid}/release")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Release(Guid projectId, Guid id, CancellationToken ct)
    {
        try
        {
            var releasedBy = User.Identity?.Name ?? "system";
            var released = await _queueService.ReleaseAsync(id, releasedBy, ct);
            if (!released)
                return NotFound(new { error = $"Queue {id} not found" });

            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get items in a queue
    /// </summary>
    [HttpGet("{id:guid}/items")]
    [ProducesResponseType(typeof(List<WorkQueueItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<WorkQueueItemDto>>> GetItems(Guid projectId, Guid id, CancellationToken ct)
    {
        var items = await _queueService.GetItemsAsync(id, ct);
        return Ok(items);
    }

    /// <summary>
    /// Add an item to a queue
    /// </summary>
    [HttpPost("{id:guid}/items")]
    [ProducesResponseType(typeof(WorkQueueItemDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WorkQueueItemDto>> AddItem(
        Guid projectId,
        Guid id,
        [FromBody] AddQueueItemRequest request,
        CancellationToken ct)
    {
        try
        {
            var addedBy = User.Identity?.Name ?? "system";
            var item = await _queueService.AddItemAsync(id, request, addedBy, ct);
            if (item == null)
                return NotFound(new { error = $"Queue {id} not found" });

            return CreatedAtAction(nameof(GetItems), new { projectId, id }, item);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Update an item in a queue
    /// </summary>
    [HttpPatch("{queueId:guid}/items/{itemId:guid}")]
    [ProducesResponseType(typeof(WorkQueueItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WorkQueueItemDto>> UpdateItem(
        Guid projectId,
        Guid queueId,
        Guid itemId,
        [FromBody] UpdateQueueItemRequest request,
        CancellationToken ct)
    {
        var item = await _queueService.UpdateItemAsync(queueId, itemId, request, ct);
        if (item == null)
            return NotFound(new { error = $"Item {itemId} not found in queue {queueId}" });

        return Ok(item);
    }

    /// <summary>
    /// Remove an item from a queue
    /// </summary>
    [HttpDelete("{queueId:guid}/items/{itemId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveItem(Guid projectId, Guid queueId, Guid itemId, CancellationToken ct)
    {
        var removed = await _queueService.RemoveItemAsync(queueId, itemId, ct);
        if (!removed)
            return NotFound(new { error = $"Item {itemId} not found in queue {queueId}" });

        return NoContent();
    }

    /// <summary>
    /// Reorder items in a queue
    /// </summary>
    [HttpPost("{id:guid}/items/reorder")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReorderItems(
        Guid projectId,
        Guid id,
        [FromBody] ReorderItemsRequest request,
        CancellationToken ct)
    {
        var reordered = await _queueService.ReorderItemsAsync(id, request, ct);
        if (!reordered)
            return NotFound(new { error = $"Queue {id} not found or has no items" });

        return NoContent();
    }

    /// <summary>
    /// Get the context helper for a queue (optionally tiered)
    /// </summary>
    [HttpGet("{id:guid}/context")]
    [ProducesResponseType(typeof(ContextHelperDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(TieredContextResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetContext(
        Guid projectId,
        Guid id,
        [FromQuery] int? tier,
        CancellationToken ct)
    {
        // If tier is specified, return tiered context
        if (tier.HasValue)
        {
            if (tier.Value < 1 || tier.Value > 4)
                return BadRequest(new { error = "Tier must be between 1 and 4" });

            var tieredContext = await _queueService.GetTieredContextAsync(id, tier.Value, ct);
            if (tieredContext == null)
                return NotFound(new { error = $"Queue {id} not found" });

            return Ok(tieredContext);
        }

        // Otherwise return just the context helper
        var context = await _queueService.GetContextAsync(id, ct);
        if (context == null)
            return NotFound(new { error = $"Queue {id} not found" });

        return Ok(context);
    }

    /// <summary>
    /// Update the context helper for a queue
    /// </summary>
    [HttpPatch("{id:guid}/context")]
    [ProducesResponseType(typeof(ContextHelperDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContextHelperDto>> UpdateContext(
        Guid projectId,
        Guid id,
        [FromBody] UpdateContextRequest request,
        CancellationToken ct)
    {
        try
        {
            var context = await _queueService.UpdateContextAsync(id, request, ct);
            if (context == null)
                return NotFound(new { error = $"Queue {id} not found" });

            return Ok(context);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
