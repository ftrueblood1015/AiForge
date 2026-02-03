using AiForge.Application.DTOs.Organization;
using AiForge.Domain.Entities;
using AiForge.Domain.Enums;
using AiForge.Domain.Interfaces;
using AiForge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AiForge.Application.Services;

public interface IOrganizationService
{
    Task<OrganizationDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<OrganizationDto>> GetUserOrganizationsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<OrganizationDto> CreateAsync(CreateOrganizationRequest request, Guid createdByUserId, CancellationToken cancellationToken = default);
    Task<OrganizationDto?> UpdateAsync(Guid id, UpdateOrganizationRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<OrganizationMemberDto>> GetMembersAsync(Guid organizationId, CancellationToken cancellationToken = default);
    Task<bool> AddMemberAsync(Guid organizationId, AddOrganizationMemberRequest request, CancellationToken cancellationToken = default);
    Task<bool> RemoveMemberAsync(Guid organizationId, Guid userId, CancellationToken cancellationToken = default);
    Task<bool> UpdateMemberRoleAsync(Guid organizationId, Guid userId, OrganizationRole newRole, CancellationToken cancellationToken = default);
}

public class OrganizationService : IOrganizationService
{
    private readonly AiForgeDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public OrganizationService(AiForgeDbContext context, IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<OrganizationDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var org = await _context.Organizations
            .Include(o => o.Members)
            .Include(o => o.Projects)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

        return org == null ? null : MapToDto(org);
    }

    public async Task<IEnumerable<OrganizationDto>> GetUserOrganizationsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var orgIds = await _context.OrganizationMembers
            .Where(m => m.UserId == userId)
            .Select(m => m.OrganizationId)
            .ToListAsync(cancellationToken);

        var orgs = await _context.Organizations
            .Include(o => o.Members)
            .Include(o => o.Projects)
            .Where(o => orgIds.Contains(o.Id))
            .ToListAsync(cancellationToken);

        return orgs.Select(MapToDto);
    }

    public async Task<OrganizationDto> CreateAsync(CreateOrganizationRequest request, Guid createdByUserId, CancellationToken cancellationToken = default)
    {
        var slug = request.Slug ?? GenerateSlug(request.Name);

        // Ensure slug is unique
        var existingSlug = await _context.Organizations.AnyAsync(o => o.Slug == slug, cancellationToken);
        if (existingSlug)
        {
            slug = $"{slug}-{Guid.NewGuid().ToString()[..8]}";
        }

        var org = new Organization
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Slug = slug,
            CreatedAt = DateTime.UtcNow,
            CreatedByUserId = createdByUserId
        };

        _context.Organizations.Add(org);

        // Add creator as admin member
        var membership = new OrganizationMember
        {
            Id = Guid.NewGuid(),
            OrganizationId = org.Id,
            UserId = createdByUserId,
            Role = OrganizationRole.Admin,
            JoinedAt = DateTime.UtcNow
        };

        _context.OrganizationMembers.Add(membership);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(org);
    }

    public async Task<OrganizationDto?> UpdateAsync(Guid id, UpdateOrganizationRequest request, CancellationToken cancellationToken = default)
    {
        var org = await _context.Organizations
            .Include(o => o.Members)
            .Include(o => o.Projects)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

        if (org == null) return null;

        if (request.Name != null)
            org.Name = request.Name;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return MapToDto(org);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var org = await _context.Organizations.FindAsync([id], cancellationToken);
        if (org == null) return false;

        _context.Organizations.Remove(org);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<IEnumerable<OrganizationMemberDto>> GetMembersAsync(Guid organizationId, CancellationToken cancellationToken = default)
    {
        var members = await _context.OrganizationMembers
            .Where(m => m.OrganizationId == organizationId)
            .ToListAsync(cancellationToken);

        var userIds = members.Select(m => m.UserId).ToList();
        var users = await _context.Users
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, cancellationToken);

        return members.Select(m => new OrganizationMemberDto
        {
            Id = m.Id,
            UserId = m.UserId,
            Email = users.TryGetValue(m.UserId, out var user) ? user.Email ?? "" : "",
            DisplayName = users.TryGetValue(m.UserId, out var u) ? (u as dynamic).DisplayName ?? "" : "",
            Role = m.Role,
            JoinedAt = m.JoinedAt
        });
    }

    public async Task<bool> AddMemberAsync(Guid organizationId, AddOrganizationMemberRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);
        if (user == null) return false;

        var existingMembership = await _context.OrganizationMembers
            .AnyAsync(m => m.OrganizationId == organizationId && m.UserId == user.Id, cancellationToken);
        if (existingMembership) return false;

        var membership = new OrganizationMember
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            UserId = user.Id,
            Role = request.Role,
            JoinedAt = DateTime.UtcNow
        };

        _context.OrganizationMembers.Add(membership);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> RemoveMemberAsync(Guid organizationId, Guid userId, CancellationToken cancellationToken = default)
    {
        var membership = await _context.OrganizationMembers
            .FirstOrDefaultAsync(m => m.OrganizationId == organizationId && m.UserId == userId, cancellationToken);

        if (membership == null) return false;

        _context.OrganizationMembers.Remove(membership);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> UpdateMemberRoleAsync(Guid organizationId, Guid userId, OrganizationRole newRole, CancellationToken cancellationToken = default)
    {
        var membership = await _context.OrganizationMembers
            .FirstOrDefaultAsync(m => m.OrganizationId == organizationId && m.UserId == userId, cancellationToken);

        if (membership == null) return false;

        membership.Role = newRole;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static string GenerateSlug(string name)
    {
        return name.ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("'", "")
            .Replace("\"", "");
    }

    private static OrganizationDto MapToDto(Organization org)
    {
        return new OrganizationDto
        {
            Id = org.Id,
            Name = org.Name,
            Slug = org.Slug,
            CreatedAt = org.CreatedAt,
            CreatedByUserId = org.CreatedByUserId,
            MemberCount = org.Members?.Count ?? 0,
            ProjectCount = org.Projects?.Count ?? 0
        };
    }
}
