using AiForge.Application.DTOs.ProjectMember;
using AiForge.Application.Interfaces;
using AiForge.Domain.Entities;
using AiForge.Domain.Enums;
using AiForge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AiForge.Application.Services;

public interface IProjectMemberService
{
    Task<IEnumerable<ProjectMemberDto>> GetMembersAsync(Guid projectId, CancellationToken cancellationToken = default);
    Task<ProjectMemberDto?> GetMemberAsync(Guid projectId, Guid userId, CancellationToken cancellationToken = default);
    Task<bool> AddMemberAsync(Guid projectId, AddProjectMemberRequest request, CancellationToken cancellationToken = default);
    Task<bool> RemoveMemberAsync(Guid projectId, Guid userId, CancellationToken cancellationToken = default);
    Task<bool> UpdateMemberRoleAsync(Guid projectId, Guid userId, ProjectRole newRole, CancellationToken cancellationToken = default);
    Task<bool> HasAccessAsync(Guid projectId, Guid userId, CancellationToken cancellationToken = default);
    Task<bool> HasRoleAsync(Guid projectId, Guid userId, ProjectRole minimumRole, CancellationToken cancellationToken = default);
    Task<IEnumerable<Guid>> GetAccessibleProjectIdsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> AddOwnerAsync(Guid projectId, Guid userId, CancellationToken cancellationToken = default);
}

public class ProjectMemberService : IProjectMemberService
{
    private readonly AiForgeDbContext _context;
    private readonly IUserContext _userContext;

    public ProjectMemberService(AiForgeDbContext context, IUserContext userContext)
    {
        _context = context;
        _userContext = userContext;
    }

    public async Task<IEnumerable<ProjectMemberDto>> GetMembersAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        var members = await _context.ProjectMembers
            .Where(m => m.ProjectId == projectId)
            .ToListAsync(cancellationToken);

        var userIds = members.Select(m => m.UserId).ToList();
        var users = await _context.Users
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, cancellationToken);

        return members.Select(m => new ProjectMemberDto
        {
            Id = m.Id,
            ProjectId = m.ProjectId,
            UserId = m.UserId,
            Email = users.TryGetValue(m.UserId, out var user) ? user.Email ?? "" : "",
            DisplayName = users.TryGetValue(m.UserId, out var u) ? u.DisplayName ?? "" : "",
            Role = m.Role,
            AddedAt = m.AddedAt,
            AddedByUserId = m.AddedByUserId
        });
    }

    public async Task<ProjectMemberDto?> GetMemberAsync(Guid projectId, Guid userId, CancellationToken cancellationToken = default)
    {
        var member = await _context.ProjectMembers
            .FirstOrDefaultAsync(m => m.ProjectId == projectId && m.UserId == userId, cancellationToken);

        if (member == null) return null;

        var user = await _context.Users.FindAsync([userId], cancellationToken);

        return new ProjectMemberDto
        {
            Id = member.Id,
            ProjectId = member.ProjectId,
            UserId = member.UserId,
            Email = user?.Email ?? "",
            DisplayName = user?.DisplayName ?? "",
            Role = member.Role,
            AddedAt = member.AddedAt,
            AddedByUserId = member.AddedByUserId
        };
    }

    public async Task<bool> AddMemberAsync(Guid projectId, AddProjectMemberRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);
        if (user == null) return false;

        var existingMembership = await _context.ProjectMembers
            .AnyAsync(m => m.ProjectId == projectId && m.UserId == user.Id, cancellationToken);
        if (existingMembership) return false;

        var membership = new ProjectMember
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            UserId = user.Id,
            Role = request.Role,
            AddedAt = DateTime.UtcNow,
            AddedByUserId = _userContext.UserId
        };

        _context.ProjectMembers.Add(membership);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> AddOwnerAsync(Guid projectId, Guid userId, CancellationToken cancellationToken = default)
    {
        // Check for existing membership (defensive - shouldn't exist for new project)
        var existingMembership = await _context.ProjectMembers
            .AnyAsync(m => m.ProjectId == projectId && m.UserId == userId, cancellationToken);
        if (existingMembership) return false;

        var membership = new ProjectMember
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            UserId = userId,
            Role = ProjectRole.Owner,
            AddedAt = DateTime.UtcNow,
            AddedByUserId = null  // Self-added as project creator
        };

        _context.ProjectMembers.Add(membership);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> RemoveMemberAsync(Guid projectId, Guid userId, CancellationToken cancellationToken = default)
    {
        var membership = await _context.ProjectMembers
            .FirstOrDefaultAsync(m => m.ProjectId == projectId && m.UserId == userId, cancellationToken);

        if (membership == null) return false;

        _context.ProjectMembers.Remove(membership);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> UpdateMemberRoleAsync(Guid projectId, Guid userId, ProjectRole newRole, CancellationToken cancellationToken = default)
    {
        var membership = await _context.ProjectMembers
            .FirstOrDefaultAsync(m => m.ProjectId == projectId && m.UserId == userId, cancellationToken);

        if (membership == null) return false;

        membership.Role = newRole;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> HasAccessAsync(Guid projectId, Guid userId, CancellationToken cancellationToken = default)
    {
        // Service accounts have access to all projects
        if (_userContext.IsServiceAccount) return true;

        // System admins have access to all projects
        if (_userContext.IsAdmin) return true;

        return await _context.ProjectMembers
            .AnyAsync(m => m.ProjectId == projectId && m.UserId == userId, cancellationToken);
    }

    public async Task<bool> HasRoleAsync(Guid projectId, Guid userId, ProjectRole minimumRole, CancellationToken cancellationToken = default)
    {
        // Service accounts and admins pass all role checks
        if (_userContext.IsServiceAccount || _userContext.IsAdmin) return true;

        var membership = await _context.ProjectMembers
            .FirstOrDefaultAsync(m => m.ProjectId == projectId && m.UserId == userId, cancellationToken);

        if (membership == null) return false;

        // ProjectRole: Viewer = 0, Member = 1, Owner = 2
        return membership.Role >= minimumRole;
    }

    public async Task<IEnumerable<Guid>> GetAccessibleProjectIdsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // Service accounts can access all projects
        if (_userContext.IsServiceAccount)
        {
            return await _context.Projects.Select(p => p.Id).ToListAsync(cancellationToken);
        }

        // System admins can access all projects
        if (_userContext.IsAdmin)
        {
            return await _context.Projects.Select(p => p.Id).ToListAsync(cancellationToken);
        }

        return await _context.ProjectMembers
            .Where(m => m.UserId == userId)
            .Select(m => m.ProjectId)
            .ToListAsync(cancellationToken);
    }
}
