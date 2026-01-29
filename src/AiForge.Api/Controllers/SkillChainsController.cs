using AiForge.Application.DTOs.SkillChains;
using AiForge.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace AiForge.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SkillChainsController : ControllerBase
{
    private readonly ISkillChainService _chainService;

    public SkillChainsController(ISkillChainService chainService)
    {
        _chainService = chainService;
    }

    /// <summary>
    /// List skill chains with optional filters
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<SkillChainSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<SkillChainSummaryDto>>> GetChains(
        [FromQuery] Guid? organizationId,
        [FromQuery] Guid? projectId,
        [FromQuery] bool? publishedOnly,
        CancellationToken cancellationToken)
    {
        var chains = await _chainService.GetChainsAsync(organizationId, projectId, publishedOnly, cancellationToken);
        return Ok(chains);
    }

    /// <summary>
    /// Get skill chain by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(SkillChainDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SkillChainDto>> GetChainById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var chain = await _chainService.GetByIdAsync(id, cancellationToken);
        if (chain == null)
            return NotFound(new { error = $"Skill chain with ID '{id}' not found" });

        return Ok(chain);
    }

    /// <summary>
    /// Get skill chain by key with scope resolution
    /// </summary>
    [HttpGet("key/{key}")]
    [ProducesResponseType(typeof(SkillChainDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SkillChainDto>> GetChainByKey(
        string key,
        [FromQuery] Guid organizationId,
        [FromQuery] Guid? projectId,
        CancellationToken cancellationToken)
    {
        if (organizationId == Guid.Empty)
            return BadRequest(new { error = "organizationId is required" });

        var chain = await _chainService.GetByKeyAsync(key, organizationId, projectId, cancellationToken);
        if (chain == null)
            return NotFound(new { error = $"Skill chain with key '{key}' not found in the specified scope" });

        return Ok(chain);
    }

    /// <summary>
    /// Create a new skill chain
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(SkillChainDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SkillChainDto>> CreateChain(
        [FromBody] CreateSkillChainRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var createdBy = HttpContext.Items["ApiKeyId"]?.ToString() ?? "system";
            var chain = await _chainService.CreateAsync(request, createdBy, cancellationToken);
            return CreatedAtAction(nameof(GetChainById), new { id = chain.Id }, chain);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Update an existing skill chain
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(SkillChainDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SkillChainDto>> UpdateChain(
        Guid id,
        [FromBody] UpdateSkillChainRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var updatedBy = HttpContext.Items["ApiKeyId"]?.ToString() ?? "system";
            var chain = await _chainService.UpdateAsync(id, request, updatedBy, cancellationToken);
            if (chain == null)
                return NotFound(new { error = $"Skill chain with ID '{id}' not found" });

            return Ok(chain);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Delete a skill chain
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteChain(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var deleted = await _chainService.DeleteAsync(id, cancellationToken);
            if (!deleted)
                return NotFound(new { error = $"Skill chain with ID '{id}' not found" });

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Publish a skill chain
    /// </summary>
    [HttpPost("{id:guid}/publish")]
    [ProducesResponseType(typeof(SkillChainDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SkillChainDto>> PublishChain(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var updatedBy = HttpContext.Items["ApiKeyId"]?.ToString() ?? "system";
            var chain = await _chainService.PublishAsync(id, updatedBy, cancellationToken);
            if (chain == null)
                return NotFound(new { error = $"Skill chain with ID '{id}' not found" });

            return Ok(chain);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Unpublish a skill chain
    /// </summary>
    [HttpPost("{id:guid}/unpublish")]
    [ProducesResponseType(typeof(SkillChainDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SkillChainDto>> UnpublishChain(
        Guid id,
        CancellationToken cancellationToken)
    {
        var updatedBy = HttpContext.Items["ApiKeyId"]?.ToString() ?? "system";
        var chain = await _chainService.UnpublishAsync(id, updatedBy, cancellationToken);
        if (chain == null)
            return NotFound(new { error = $"Skill chain with ID '{id}' not found" });

        return Ok(chain);
    }

    /// <summary>
    /// Add a link to a skill chain
    /// </summary>
    [HttpPost("{id:guid}/links")]
    [ProducesResponseType(typeof(SkillChainLinkDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SkillChainLinkDto>> AddLink(
        Guid id,
        [FromBody] CreateSkillChainLinkRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var link = await _chainService.AddLinkAsync(id, request, cancellationToken);
            return CreatedAtAction(nameof(GetChainById), new { id }, link);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Update a chain link
    /// </summary>
    [HttpPut("links/{linkId:guid}")]
    [ProducesResponseType(typeof(SkillChainLinkDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SkillChainLinkDto>> UpdateLink(
        Guid linkId,
        [FromBody] UpdateSkillChainLinkRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var link = await _chainService.UpdateLinkAsync(linkId, request, cancellationToken);
            if (link == null)
                return NotFound(new { error = $"Chain link with ID '{linkId}' not found" });

            return Ok(link);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Remove a chain link
    /// </summary>
    [HttpDelete("links/{linkId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveLink(
        Guid linkId,
        CancellationToken cancellationToken)
    {
        var removed = await _chainService.RemoveLinkAsync(linkId, cancellationToken);
        if (!removed)
            return NotFound(new { error = $"Chain link with ID '{linkId}' not found" });

        return NoContent();
    }

    /// <summary>
    /// Reorder links in a chain
    /// </summary>
    [HttpPut("{id:guid}/links/reorder")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ReorderLinks(
        Guid id,
        [FromBody] ReorderLinksRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var reordered = await _chainService.ReorderLinksAsync(id, request.LinkIdsInOrder, cancellationToken);
            if (!reordered)
                return NotFound(new { error = $"Skill chain with ID '{id}' not found" });

            return Ok(new { message = "Links reordered successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
