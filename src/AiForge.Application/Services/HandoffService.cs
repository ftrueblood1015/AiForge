using System.Text.Json;
using AutoMapper;
using AiForge.Application.DTOs.Handoffs;
using AiForge.Application.Extensions;
using AiForge.Application.Interfaces;
using AiForge.Domain.Entities;
using AiForge.Domain.Enums;
using AiForge.Domain.Interfaces;
using AiForge.Infrastructure.Repositories;

namespace AiForge.Application.Services;

public interface IHandoffService
{
    Task<HandoffDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<HandoffDetailDto?> GetLatestActiveByTicketIdAsync(Guid ticketId, CancellationToken cancellationToken = default);
    Task<IEnumerable<HandoffDto>> GetByTicketIdAsync(Guid ticketId, CancellationToken cancellationToken = default);
    Task<IEnumerable<HandoffDto>> SearchAsync(Guid? ticketId, HandoffType? type, string? search, CancellationToken cancellationToken = default);
    Task<HandoffDetailDto> CreateAsync(CreateHandoffRequest request, CancellationToken cancellationToken = default);
    Task<HandoffDetailDto?> UpdateAsync(Guid id, UpdateHandoffRequest request, CancellationToken cancellationToken = default);
    Task<FileSnapshotDto> AddFileSnapshotAsync(Guid handoffId, CreateFileSnapshotRequest request, CancellationToken cancellationToken = default);
    Task<IEnumerable<FileSnapshotDto>> GetFileSnapshotsAsync(Guid handoffId, CancellationToken cancellationToken = default);
}

public class HandoffService : IHandoffService
{
    private readonly IHandoffRepository _handoffRepository;
    private readonly ITicketRepository _ticketRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IUserContext _userContext;
    private readonly IProjectMemberService _projectMemberService;

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public HandoffService(
        IHandoffRepository handoffRepository,
        ITicketRepository ticketRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IUserContext userContext,
        IProjectMemberService projectMemberService)
    {
        _handoffRepository = handoffRepository;
        _ticketRepository = ticketRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _userContext = userContext;
        _projectMemberService = projectMemberService;
    }

    public async Task<HandoffDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var handoff = await _handoffRepository.GetByIdWithDetailsAsync(id, cancellationToken);
        if (handoff == null) return null;

        // Check project access
        var accessibleProjects = await _projectMemberService.GetAccessibleProjectIdsOrNullAsync(_userContext, cancellationToken);
        if (accessibleProjects != null && handoff.Ticket != null && !accessibleProjects.Contains(handoff.Ticket.ProjectId))
            return null;

        return MapToDetailDto(handoff);
    }

    public async Task<HandoffDetailDto?> GetLatestActiveByTicketIdAsync(Guid ticketId, CancellationToken cancellationToken = default)
    {
        // Check project access via ticket
        var ticket = await _ticketRepository.GetByIdAsync(ticketId, cancellationToken);
        if (ticket == null) return null;

        var accessibleProjects = await _projectMemberService.GetAccessibleProjectIdsOrNullAsync(_userContext, cancellationToken);
        if (accessibleProjects != null && !accessibleProjects.Contains(ticket.ProjectId))
            return null;

        var handoff = await _handoffRepository.GetLatestActiveByTicketIdAsync(ticketId, cancellationToken);
        return handoff == null ? null : MapToDetailDto(handoff);
    }

    public async Task<IEnumerable<HandoffDto>> GetByTicketIdAsync(Guid ticketId, CancellationToken cancellationToken = default)
    {
        // Check project access via ticket
        var ticket = await _ticketRepository.GetByIdAsync(ticketId, cancellationToken);
        if (ticket == null) return Enumerable.Empty<HandoffDto>();

        var accessibleProjects = await _projectMemberService.GetAccessibleProjectIdsOrNullAsync(_userContext, cancellationToken);
        if (accessibleProjects != null && !accessibleProjects.Contains(ticket.ProjectId))
            return Enumerable.Empty<HandoffDto>();

        var handoffs = await _handoffRepository.GetByTicketIdAsync(ticketId, cancellationToken);
        return handoffs.Select(MapToDto);
    }

    public async Task<IEnumerable<HandoffDto>> SearchAsync(Guid? ticketId, HandoffType? type, string? search, CancellationToken cancellationToken = default)
    {
        var accessibleProjects = await _projectMemberService.GetAccessibleProjectIdsOrNullAsync(_userContext, cancellationToken);
        var handoffs = await _handoffRepository.SearchAsync(ticketId, type, search, cancellationToken);

        // Filter to accessible projects
        if (accessibleProjects != null)
        {
            handoffs = handoffs.Where(h => h.Ticket != null && accessibleProjects.Contains(h.Ticket.ProjectId));
        }

        return handoffs.Select(MapToDto);
    }

    public async Task<HandoffDetailDto> CreateAsync(CreateHandoffRequest request, CancellationToken cancellationToken = default)
    {
        // Verify ticket exists
        var ticket = await _ticketRepository.GetByIdAsync(request.TicketId, cancellationToken)
            ?? throw new InvalidOperationException($"Ticket with ID '{request.TicketId}' not found");

        // Deactivate previous active handoffs for this ticket
        var previousHandoffs = await _handoffRepository.GetByTicketIdAsync(request.TicketId, cancellationToken);
        foreach (var prev in previousHandoffs.Where(h => h.IsActive))
        {
            prev.IsActive = false;
            await _handoffRepository.UpdateAsync(prev, cancellationToken);
        }

        var handoff = new HandoffDocument
        {
            Id = Guid.NewGuid(),
            TicketId = request.TicketId,
            SessionId = request.SessionId,
            Title = request.Title,
            Type = request.Type,
            Summary = request.Summary,
            Content = request.Content,
            StructuredContext = request.Context != null ? JsonSerializer.Serialize(request.Context, JsonOptions) : null,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _handoffRepository.AddAsync(handoff, cancellationToken);

        // Update ticket's handoff summary
        ticket.CurrentHandoffSummary = request.Summary;
        ticket.UpdatedAt = DateTime.UtcNow;
        await _ticketRepository.UpdateAsync(ticket, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDetailDto(handoff);
    }

    public async Task<HandoffDetailDto?> UpdateAsync(Guid id, UpdateHandoffRequest request, CancellationToken cancellationToken = default)
    {
        var handoff = await _handoffRepository.GetByIdWithDetailsAsync(id, cancellationToken);
        if (handoff == null)
            return null;

        // Create a version before updating
        var nextVersion = await _handoffRepository.GetNextVersionNumberAsync(id, cancellationToken);
        var version = new HandoffVersion
        {
            Id = Guid.NewGuid(),
            HandoffId = id,
            Version = nextVersion,
            Content = handoff.Content,
            StructuredContext = handoff.StructuredContext,
            CreatedAt = DateTime.UtcNow
        };
        await _handoffRepository.AddVersionAsync(version, cancellationToken);

        // Update the handoff
        if (request.Title != null)
            handoff.Title = request.Title;

        if (request.Summary != null)
            handoff.Summary = request.Summary;

        if (request.Content != null)
            handoff.Content = request.Content;

        if (request.Context != null)
            handoff.StructuredContext = JsonSerializer.Serialize(request.Context, JsonOptions);

        await _handoffRepository.UpdateAsync(handoff, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<FileSnapshotDto> AddFileSnapshotAsync(Guid handoffId, CreateFileSnapshotRequest request, CancellationToken cancellationToken = default)
    {
        var handoff = await _handoffRepository.GetByIdAsync(handoffId, cancellationToken)
            ?? throw new InvalidOperationException($"Handoff with ID '{handoffId}' not found");

        var snapshot = new FileSnapshot
        {
            Id = Guid.NewGuid(),
            HandoffId = handoffId,
            FilePath = request.FilePath,
            ContentBefore = request.ContentBefore,
            ContentAfter = request.ContentAfter,
            Language = request.Language,
            CreatedAt = DateTime.UtcNow
        };

        await _handoffRepository.AddFileSnapshotAsync(snapshot, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<FileSnapshotDto>(snapshot);
    }

    public async Task<IEnumerable<FileSnapshotDto>> GetFileSnapshotsAsync(Guid handoffId, CancellationToken cancellationToken = default)
    {
        var snapshots = await _handoffRepository.GetFileSnapshotsByHandoffIdAsync(handoffId, cancellationToken);
        return _mapper.Map<IEnumerable<FileSnapshotDto>>(snapshots);
    }

    private HandoffDto MapToDto(HandoffDocument handoff)
    {
        return new HandoffDto
        {
            Id = handoff.Id,
            TicketId = handoff.TicketId,
            SessionId = handoff.SessionId,
            Title = handoff.Title,
            Type = handoff.Type,
            Summary = handoff.Summary,
            IsActive = handoff.IsActive,
            CreatedAt = handoff.CreatedAt
        };
    }

    private HandoffDetailDto MapToDetailDto(HandoffDocument handoff)
    {
        return new HandoffDetailDto
        {
            Id = handoff.Id,
            TicketId = handoff.TicketId,
            SessionId = handoff.SessionId,
            Title = handoff.Title,
            Type = handoff.Type,
            Summary = handoff.Summary,
            Content = handoff.Content,
            StructuredContext = DeserializeContext(handoff.StructuredContext),
            IsActive = handoff.IsActive,
            SupersededById = handoff.SupersededById,
            CreatedAt = handoff.CreatedAt,
            FileSnapshots = handoff.FileSnapshots?.Select(f => new FileSnapshotDto
            {
                Id = f.Id,
                FilePath = f.FilePath,
                ContentBefore = f.ContentBefore,
                ContentAfter = f.ContentAfter,
                Language = f.Language,
                CreatedAt = f.CreatedAt
            }).ToList() ?? new List<FileSnapshotDto>(),
            VersionCount = handoff.Versions?.Count ?? 0
        };
    }

    private static StructuredContextDto? DeserializeContext(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            return JsonSerializer.Deserialize<StructuredContextDto>(json, JsonOptions);
        }
        catch
        {
            return null;
        }
    }
}
