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

    /// <summary>
    /// Get the current user's role in a project, or null if not a member.
    /// </summary>
    Task<ProjectRole?> GetUserRoleAsync(Guid projectId, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if user is a project Owner (convenience method).
    /// </summary>
    Task<bool> IsOwnerAsync(Guid projectId, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Search users by email or display name, optionally excluding those already in a project.
    /// </summary>
    Task<IEnumerable<UserSearchResultDto>> SearchUsersAsync(string query, Guid? excludeProjectId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Count the number of Owners in a project (used for last-owner protection).
    /// </summary>
    Task<int> GetOwnerCountAsync(Guid projectId, CancellationToken cancellationToken = default);
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

        // Get all user IDs (members + those who added them)
        var userIds = members.Select(m => m.UserId)
            .Union(members.Where(m => m.AddedByUserId.HasValue).Select(m => m.AddedByUserId!.Value))
            .Distinct()
            .ToList();

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
            AddedByUserId = m.AddedByUserId,
            AddedByUserName = m.AddedByUserId.HasValue && users.TryGetValue(m.AddedByUserId.Value, out var addedByUser)
                ? addedByUser.DisplayName
                : null
        });
    }

    public async Task<ProjectMemberDto?> GetMemberAsync(Guid projectId, Guid userId, CancellationToken cancellationToken = default)
    {
        var member = await _context.ProjectMembers
            .FirstOrDefaultAsync(m => m.ProjectId == projectId && m.UserId == userId, cancellationToken);

        if (member == null) return null;

        var user = await _context.Users.FindAsync([userId], cancellationToken);
        string? addedByUserName = null;
        if (member.AddedByUserId.HasValue)
        {
            var addedByUser = await _context.Users.FindAsync([member.AddedByUserId.Value], cancellationToken);
            addedByUserName = addedByUser?.DisplayName;
        }

        return new ProjectMemberDto
        {
            Id = member.Id,
            ProjectId = member.ProjectId,
            UserId = member.UserId,
            Email = user?.Email ?? "",
            DisplayName = user?.DisplayName ?? "",
            Role = member.Role,
            AddedAt = member.AddedAt,
            AddedByUserId = member.AddedByUserId,
            AddedByUserName = addedByUserName
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

        // Prevent removing the last Owner
        if (membership.Role == ProjectRole.Owner)
        {
            var ownerCount = await GetOwnerCountAsync(projectId, cancellationToken);
            if (ownerCount <= 1) return false;
        }

        _context.ProjectMembers.Remove(membership);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> UpdateMemberRoleAsync(Guid projectId, Guid userId, ProjectRole newRole, CancellationToken cancellationToken = default)
    {
        var membership = await _context.ProjectMembers
            .FirstOrDefaultAsync(m => m.ProjectId == projectId && m.UserId == userId, cancellationToken);

        if (membership == null) return false;

        // Prevent demoting the last Owner
        if (membership.Role == ProjectRole.Owner && newRole != ProjectRole.Owner)
        {
            var ownerCount = await GetOwnerCountAsync(projectId, cancellationToken);
            if (ownerCount <= 1) return false;
        }

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

    public async Task<ProjectRole?> GetUserRoleAsync(Guid projectId, Guid userId, CancellationToken cancellationToken = default)
    {
        var member = await _context.ProjectMembers
            .FirstOrDefaultAsync(m => m.ProjectId == projectId && m.UserId == userId, cancellationToken);

        return member?.Role;
    }

    public async Task<bool> IsOwnerAsync(Guid projectId, Guid userId, CancellationToken cancellationToken = default)
    {
        // Service accounts and admins are treated as owners for authorization purposes
        if (_userContext.IsServiceAccount || _userContext.IsAdmin) return true;

        var role = await GetUserRoleAsync(projectId, userId, cancellationToken);
        return role == ProjectRole.Owner;
    }

    public async Task<IEnumerable<UserSearchResultDto>> SearchUsersAsync(string query, Guid? excludeProjectId = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            return [];

        var normalizedQuery = query.ToLower();

        var usersQuery = _context.Users
            .Where(u => u.IsActive)
            .Where(u => (u.Email != null && u.Email.ToLower().Contains(normalizedQuery)) ||
                        u.DisplayName.ToLower().Contains(normalizedQuery));

        // Exclude users already in the specified project
        if (excludeProjectId.HasValue)
        {
            var existingMemberIds = _context.ProjectMembers
                .Where(m => m.ProjectId == excludeProjectId.Value)
                .Select(m => m.UserId);

            usersQuery = usersQuery.Where(u => !existingMemberIds.Contains(u.Id));
        }

        var users = await usersQuery
            .Take(20)
            .Select(u => new UserSearchResultDto
            {
                Id = u.Id,
                Email = u.Email ?? "",
                DisplayName = u.DisplayName
            })
            .ToListAsync(cancellationToken);

        return users;
    }

    public async Task<int> GetOwnerCountAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        return await _context.ProjectMembers
            .CountAsync(m => m.ProjectId == projectId && m.Role == ProjectRole.Owner, cancellationToken);
    }
}
