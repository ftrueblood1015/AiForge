using AiForge.Api.Authorization;
using AiForge.Application.DTOs.ProjectMember;
using AiForge.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiForge.Api.Controllers;

[ApiController]
[Route("api/projects/{projectId:guid}/members")]
[Authorize]
public class ProjectMembersController : ControllerBase
{
    private readonly IProjectMemberService _projectMemberService;

    public ProjectMembersController(IProjectMemberService projectMemberService)
    {
        _projectMemberService = projectMemberService;
    }

    /// <summary>
    /// Get all members of a project
    /// </summary>
    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.RequireProjectAccess)]
    [ProducesResponseType(typeof(IEnumerable<ProjectMemberDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ProjectMemberDto>>> GetMembers(Guid projectId, CancellationToken cancellationToken)
    {
        var members = await _projectMemberService.GetMembersAsync(projectId, cancellationToken);
        return Ok(members);
    }

    /// <summary>
    /// Get a specific member of a project
    /// </summary>
    [HttpGet("{userId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.RequireProjectAccess)]
    [ProducesResponseType(typeof(ProjectMemberDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProjectMemberDto>> GetMember(Guid projectId, Guid userId, CancellationToken cancellationToken)
    {
        var member = await _projectMemberService.GetMemberAsync(projectId, userId, cancellationToken);
        if (member == null)
            return NotFound(new { error = "Member not found" });

        return Ok(member);
    }

    /// <summary>
    /// Add a member to a project
    /// </summary>
    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.RequireProjectOwner)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddMember(Guid projectId, [FromBody] AddProjectMemberRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return BadRequest(new { error = "Email is required" });

        var added = await _projectMemberService.AddMemberAsync(projectId, request, cancellationToken);
        if (!added)
            return BadRequest(new { error = "User not found or already a member" });

        return Created($"/api/projects/{projectId}/members", null);
    }

    /// <summary>
    /// Update a member's role
    /// </summary>
    [HttpPut("{userId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.RequireProjectOwner)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateMemberRole(Guid projectId, Guid userId, [FromBody] UpdateProjectMemberRoleRequest request, CancellationToken cancellationToken)
    {
        var updated = await _projectMemberService.UpdateMemberRoleAsync(projectId, userId, request.Role, cancellationToken);
        if (!updated)
            return NotFound(new { error = "Member not found" });

        return NoContent();
    }

    /// <summary>
    /// Remove a member from a project
    /// </summary>
    [HttpDelete("{userId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.RequireProjectOwner)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveMember(Guid projectId, Guid userId, CancellationToken cancellationToken)
    {
        var removed = await _projectMemberService.RemoveMemberAsync(projectId, userId, cancellationToken);
        if (!removed)
            return NotFound(new { error = "Member not found" });

        return NoContent();
    }
}
