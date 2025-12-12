using AiForge.Application.DTOs.Projects;
using AiForge.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace AiForge.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _projectService;

    public ProjectsController(IProjectService projectService)
    {
        _projectService = projectService;
    }

    /// <summary>
    /// List all projects
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProjectDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ProjectDto>>> GetAll(CancellationToken cancellationToken)
    {
        var projects = await _projectService.GetAllAsync(cancellationToken);
        return Ok(projects);
    }

    /// <summary>
    /// Get project by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProjectDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProjectDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var project = await _projectService.GetByIdAsync(id, cancellationToken);
        if (project == null)
            return NotFound(new { error = $"Project with ID '{id}' not found" });

        return Ok(project);
    }

    /// <summary>
    /// Get project by key (e.g., "AIFORGE")
    /// </summary>
    [HttpGet("key/{key}")]
    [ProducesResponseType(typeof(ProjectDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProjectDto>> GetByKey(string key, CancellationToken cancellationToken)
    {
        var project = await _projectService.GetByKeyAsync(key.ToUpperInvariant(), cancellationToken);
        if (project == null)
            return NotFound(new { error = $"Project with key '{key}' not found" });

        return Ok(project);
    }

    /// <summary>
    /// Create a new project
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ProjectDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProjectDto>> Create([FromBody] CreateProjectRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Key))
            return BadRequest(new { error = "Project key is required" });

        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { error = "Project name is required" });

        try
        {
            var project = await _projectService.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = project.Id }, project);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Update a project
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ProjectDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProjectDto>> Update(Guid id, [FromBody] UpdateProjectRequest request, CancellationToken cancellationToken)
    {
        var project = await _projectService.UpdateAsync(id, request, cancellationToken);
        if (project == null)
            return NotFound(new { error = $"Project with ID '{id}' not found" });

        return Ok(project);
    }

    /// <summary>
    /// Delete a project
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _projectService.DeleteAsync(id, cancellationToken);
        if (!deleted)
            return NotFound(new { error = $"Project with ID '{id}' not found" });

        return NoContent();
    }
}
