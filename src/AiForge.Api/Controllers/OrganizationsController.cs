using AiForge.Application.DTOs.Organization;
using AiForge.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AiForge.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrganizationsController : ControllerBase
{
    private readonly IOrganizationService _organizationService;

    public OrganizationsController(IOrganizationService organizationService)
    {
        _organizationService = organizationService;
    }

    private Guid? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    /// <summary>
    /// Get organizations for current user
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<OrganizationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<OrganizationDto>>> GetMyOrganizations(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var orgs = await _organizationService.GetUserOrganizationsAsync(userId.Value, cancellationToken);
        return Ok(orgs);
    }

    /// <summary>
    /// Get organization by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrganizationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrganizationDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var org = await _organizationService.GetByIdAsync(id, cancellationToken);
        if (org == null)
            return NotFound(new { error = $"Organization with ID '{id}' not found" });

        return Ok(org);
    }

    /// <summary>
    /// Create a new organization
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(OrganizationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OrganizationDto>> Create([FromBody] CreateOrganizationRequest request, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { error = "Organization name is required" });

        var org = await _organizationService.CreateAsync(request, userId.Value, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = org.Id }, org);
    }

    /// <summary>
    /// Update an organization
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(OrganizationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrganizationDto>> Update(Guid id, [FromBody] UpdateOrganizationRequest request, CancellationToken cancellationToken)
    {
        var org = await _organizationService.UpdateAsync(id, request, cancellationToken);
        if (org == null)
            return NotFound(new { error = $"Organization with ID '{id}' not found" });

        return Ok(org);
    }

    /// <summary>
    /// Delete an organization
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _organizationService.DeleteAsync(id, cancellationToken);
        if (!deleted)
            return NotFound(new { error = $"Organization with ID '{id}' not found" });

        return NoContent();
    }

    /// <summary>
    /// Get members of an organization
    /// </summary>
    [HttpGet("{id:guid}/members")]
    [ProducesResponseType(typeof(IEnumerable<OrganizationMemberDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<OrganizationMemberDto>>> GetMembers(Guid id, CancellationToken cancellationToken)
    {
        var members = await _organizationService.GetMembersAsync(id, cancellationToken);
        return Ok(members);
    }

    /// <summary>
    /// Add a member to an organization
    /// </summary>
    [HttpPost("{id:guid}/members")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddMember(Guid id, [FromBody] AddOrganizationMemberRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return BadRequest(new { error = "Email is required" });

        var added = await _organizationService.AddMemberAsync(id, request, cancellationToken);
        if (!added)
            return BadRequest(new { error = "User not found or already a member" });

        return Created($"/api/organizations/{id}/members", null);
    }

    /// <summary>
    /// Update a member's role
    /// </summary>
    [HttpPut("{id:guid}/members/{userId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateMemberRole(Guid id, Guid userId, [FromBody] UpdateMemberRoleRequest request, CancellationToken cancellationToken)
    {
        var updated = await _organizationService.UpdateMemberRoleAsync(id, userId, request.Role, cancellationToken);
        if (!updated)
            return NotFound(new { error = "Member not found" });

        return NoContent();
    }

    /// <summary>
    /// Remove a member from an organization
    /// </summary>
    [HttpDelete("{id:guid}/members/{userId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveMember(Guid id, Guid userId, CancellationToken cancellationToken)
    {
        var removed = await _organizationService.RemoveMemberAsync(id, userId, cancellationToken);
        if (!removed)
            return NotFound(new { error = "Member not found" });

        return NoContent();
    }
}
