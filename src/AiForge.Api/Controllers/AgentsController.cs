using AiForge.Application.DTOs.Agent;
using AiForge.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace AiForge.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AgentsController : ControllerBase
{
    private readonly IAgentService _agentService;

    public AgentsController(IAgentService agentService)
    {
        _agentService = agentService;
    }

    /// <summary>
    /// List agents with optional organization/project filters
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(AgentListResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<AgentListResponse>> GetAgents(
        [FromQuery] Guid? organizationId,
        [FromQuery] Guid? projectId,
        CancellationToken cancellationToken)
    {
        var agents = await _agentService.GetAgentsAsync(organizationId, projectId, cancellationToken);
        return Ok(agents);
    }

    /// <summary>
    /// Get agent by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AgentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AgentDto>> GetAgentById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var agent = await _agentService.GetAgentByIdAsync(id, cancellationToken);
        if (agent == null)
            return NotFound(new { error = $"Agent with ID '{id}' not found" });

        return Ok(agent);
    }

    /// <summary>
    /// Get agent by key with scope resolution (project-level takes precedence over org-level)
    /// </summary>
    [HttpGet("key/{key}")]
    [ProducesResponseType(typeof(AgentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AgentDto>> GetAgentByKey(
        string key,
        [FromQuery] Guid organizationId,
        [FromQuery] Guid? projectId,
        CancellationToken cancellationToken)
    {
        if (organizationId == Guid.Empty)
            return BadRequest(new { error = "organizationId is required" });

        var agent = await _agentService.GetAgentByKeyAsync(key, projectId, organizationId, cancellationToken);
        if (agent == null)
            return NotFound(new { error = $"Agent with key '{key}' not found in the specified scope" });

        return Ok(agent);
    }

    /// <summary>
    /// Create a new agent
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(AgentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AgentDto>> CreateAgent(
        [FromBody] CreateAgentRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var createdBy = HttpContext.Items["ApiKeyId"]?.ToString() ?? "system";
            var agent = await _agentService.CreateAgentAsync(request, createdBy, cancellationToken);
            return CreatedAtAction(nameof(GetAgentById), new { id = agent.Id }, agent);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Update an existing agent
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(AgentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AgentDto>> UpdateAgent(
        Guid id,
        [FromBody] UpdateAgentRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var updatedBy = HttpContext.Items["ApiKeyId"]?.ToString() ?? "system";
            var agent = await _agentService.UpdateAgentAsync(id, request, updatedBy, cancellationToken);
            if (agent == null)
                return NotFound(new { error = $"Agent with ID '{id}' not found" });

            return Ok(agent);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Delete an agent
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAgent(
        Guid id,
        CancellationToken cancellationToken)
    {
        var deleted = await _agentService.DeleteAgentAsync(id, cancellationToken);
        if (!deleted)
            return NotFound(new { error = $"Agent with ID '{id}' not found" });

        return NoContent();
    }
}
