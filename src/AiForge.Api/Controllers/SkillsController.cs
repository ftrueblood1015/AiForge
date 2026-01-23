using AiForge.Application.DTOs.Skill;
using AiForge.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace AiForge.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SkillsController : ControllerBase
{
    private readonly ISkillService _skillService;

    public SkillsController(ISkillService skillService)
    {
        _skillService = skillService;
    }

    /// <summary>
    /// List skills with optional organization/project/category filters
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(SkillListResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<SkillListResponse>> GetSkills(
        [FromQuery] Guid? organizationId,
        [FromQuery] Guid? projectId,
        [FromQuery] string? category,
        [FromQuery] bool? publishedOnly,
        CancellationToken cancellationToken)
    {
        var skills = await _skillService.GetSkillsAsync(organizationId, projectId, category, publishedOnly, cancellationToken);
        return Ok(skills);
    }

    /// <summary>
    /// Get skill by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(SkillDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SkillDto>> GetSkillById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var skill = await _skillService.GetSkillByIdAsync(id, cancellationToken);
        if (skill == null)
            return NotFound(new { error = $"Skill with ID '{id}' not found" });

        return Ok(skill);
    }

    /// <summary>
    /// Get skill by key with scope resolution (project-level takes precedence over org-level)
    /// </summary>
    [HttpGet("key/{key}")]
    [ProducesResponseType(typeof(SkillDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SkillDto>> GetSkillByKey(
        string key,
        [FromQuery] Guid organizationId,
        [FromQuery] Guid? projectId,
        CancellationToken cancellationToken)
    {
        if (organizationId == Guid.Empty)
            return BadRequest(new { error = "organizationId is required" });

        var skill = await _skillService.GetSkillByKeyAsync(key, projectId, organizationId, cancellationToken);
        if (skill == null)
            return NotFound(new { error = $"Skill with key '{key}' not found in the specified scope" });

        return Ok(skill);
    }

    /// <summary>
    /// Create a new skill
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(SkillDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SkillDto>> CreateSkill(
        [FromBody] CreateSkillRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var createdBy = HttpContext.Items["ApiKeyId"]?.ToString() ?? "system";
            var skill = await _skillService.CreateSkillAsync(request, createdBy, cancellationToken);
            return CreatedAtAction(nameof(GetSkillById), new { id = skill.Id }, skill);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Update an existing skill
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(SkillDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SkillDto>> UpdateSkill(
        Guid id,
        [FromBody] UpdateSkillRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var updatedBy = HttpContext.Items["ApiKeyId"]?.ToString() ?? "system";
            var skill = await _skillService.UpdateSkillAsync(id, request, updatedBy, cancellationToken);
            if (skill == null)
                return NotFound(new { error = $"Skill with ID '{id}' not found" });

            return Ok(skill);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Publish a skill (make it available for use)
    /// </summary>
    [HttpPost("{id:guid}/publish")]
    [ProducesResponseType(typeof(SkillDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SkillDto>> PublishSkill(
        Guid id,
        CancellationToken cancellationToken)
    {
        var updatedBy = HttpContext.Items["ApiKeyId"]?.ToString() ?? "system";
        var skill = await _skillService.PublishSkillAsync(id, updatedBy, cancellationToken);
        if (skill == null)
            return NotFound(new { error = $"Skill with ID '{id}' not found" });

        return Ok(skill);
    }

    /// <summary>
    /// Unpublish a skill (hide from use)
    /// </summary>
    [HttpPost("{id:guid}/unpublish")]
    [ProducesResponseType(typeof(SkillDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SkillDto>> UnpublishSkill(
        Guid id,
        CancellationToken cancellationToken)
    {
        var updatedBy = HttpContext.Items["ApiKeyId"]?.ToString() ?? "system";
        var skill = await _skillService.UnpublishSkillAsync(id, updatedBy, cancellationToken);
        if (skill == null)
            return NotFound(new { error = $"Skill with ID '{id}' not found" });

        return Ok(skill);
    }

    /// <summary>
    /// Delete a skill
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSkill(
        Guid id,
        CancellationToken cancellationToken)
    {
        var deleted = await _skillService.DeleteSkillAsync(id, cancellationToken);
        if (!deleted)
            return NotFound(new { error = $"Skill with ID '{id}' not found" });

        return NoContent();
    }
}
