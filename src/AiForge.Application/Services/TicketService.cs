using AutoMapper;
using AiForge.Application.DTOs.Tickets;
using AiForge.Application.Interfaces;
using AiForge.Domain.Entities;
using AiForge.Domain.Enums;
using AiForge.Domain.Interfaces;
using AiForge.Infrastructure.Repositories;
using AiForge.Application.Services;

namespace AiForge.Application.Services;

public interface ITicketService
{
    Task<IEnumerable<TicketDto>> SearchAsync(TicketSearchRequest request, CancellationToken cancellationToken = default);
    Task<TicketDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<TicketDetailDto?> GetByKeyAsync(string key, CancellationToken cancellationToken = default);
    Task<TicketDto> CreateAsync(CreateTicketRequest request, CancellationToken cancellationToken = default);
    Task<TicketDto?> UpdateAsync(Guid id, UpdateTicketRequest request, string? changedBy = null, CancellationToken cancellationToken = default);
    Task<TicketDto?> TransitionAsync(Guid id, TransitionTicketRequest request, string? changedBy = null, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    // Sub-ticket operations
    Task<IEnumerable<SubTicketSummaryDto>> GetSubTicketsAsync(Guid parentTicketId, CancellationToken cancellationToken = default);
    Task<TicketDto> CreateSubTicketAsync(Guid parentTicketId, CreateSubTicketRequest request, CancellationToken cancellationToken = default);
    Task<TicketDto?> MoveTicketAsync(Guid ticketId, MoveSubTicketRequest request, string? changedBy = null, CancellationToken cancellationToken = default);
    Task<bool> CanBeParentAsync(Guid ticketId, Guid proposedParentId, CancellationToken cancellationToken = default);
}

public class TicketService : ITicketService
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly ITicketHistoryRepository _historyRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IUserContext _userContext;
    private readonly IProjectMemberService _projectMemberService;

    public TicketService(
        ITicketRepository ticketRepository,
        IProjectRepository projectRepository,
        ITicketHistoryRepository historyRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IUserContext userContext,
        IProjectMemberService projectMemberService)
    {
        _ticketRepository = ticketRepository;
        _projectRepository = projectRepository;
        _historyRepository = historyRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _userContext = userContext;
        _projectMemberService = projectMemberService;
    }

    public async Task<IEnumerable<TicketDto>> SearchAsync(TicketSearchRequest request, CancellationToken cancellationToken = default)
    {
        var tickets = await _ticketRepository.SearchAsync(
            request.ProjectId,
            request.Status,
            request.Type,
            request.Priority,
            request.Search,
            cancellationToken);

        // Service accounts and admins can see all tickets
        if (!_userContext.IsServiceAccount && !_userContext.IsAdmin && _userContext.UserId.HasValue)
        {
            var accessibleIds = await _projectMemberService.GetAccessibleProjectIdsAsync(_userContext.UserId.Value, cancellationToken);
            var accessibleSet = accessibleIds.ToHashSet();
            tickets = tickets.Where(t => accessibleSet.Contains(t.ProjectId));
        }

        return _mapper.Map<IEnumerable<TicketDto>>(tickets);
    }

    public async Task<TicketDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var ticket = await _ticketRepository.GetByIdWithDetailsAsync(id, cancellationToken);
        return ticket == null ? null : _mapper.Map<TicketDetailDto>(ticket);
    }

    public async Task<TicketDetailDto?> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        var ticket = await _ticketRepository.GetByKeyAsync(key, cancellationToken);
        if (ticket == null)
            return null;

        // Get full details
        return await GetByIdAsync(ticket.Id, cancellationToken);
    }

    public async Task<TicketDto> CreateAsync(CreateTicketRequest request, CancellationToken cancellationToken = default)
    {
        var project = await _projectRepository.GetByKeyAsync(request.ProjectKey, cancellationToken)
            ?? throw new InvalidOperationException($"Project with key '{request.ProjectKey}' not found");

        var ticket = _mapper.Map<Ticket>(request);
        ticket.ProjectId = project.Id;

        // Set CreatedByUserId: use request value if provided (MCP), otherwise use current user
        ticket.CreatedByUserId = request.CreatedByUserId ?? _userContext.UserId;
        ticket.AssignedToUserId = request.AssignedToUserId;

        // Get and increment the ticket number
        var ticketNumber = await _projectRepository.GetAndIncrementNextTicketNumberAsync(project.Id, cancellationToken);
        ticket.Number = ticketNumber;
        ticket.Key = $"{project.Key}-{ticketNumber}";

        await _ticketRepository.AddAsync(ticket, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Reload with project info
        var createdTicket = await _ticketRepository.GetByIdAsync(ticket.Id, cancellationToken);
        createdTicket!.Project = project;

        return _mapper.Map<TicketDto>(createdTicket);
    }

    public async Task<TicketDto?> UpdateAsync(Guid id, UpdateTicketRequest request, string? changedBy = null, CancellationToken cancellationToken = default)
    {
        var ticket = await _ticketRepository.GetByIdAsync(id, cancellationToken);
        if (ticket == null)
            return null;

        var changes = new List<(string Field, string? OldValue, string? NewValue)>();

        if (request.Title != null && request.Title != ticket.Title)
        {
            changes.Add(("Title", ticket.Title, request.Title));
            ticket.Title = request.Title;
        }

        if (request.Description != null && request.Description != ticket.Description)
        {
            changes.Add(("Description", ticket.Description, request.Description));
            ticket.Description = request.Description;
        }

        if (request.Type.HasValue && request.Type.Value != ticket.Type)
        {
            changes.Add(("Type", ticket.Type.ToString(), request.Type.Value.ToString()));
            ticket.Type = request.Type.Value;
        }

        if (request.Priority.HasValue && request.Priority.Value != ticket.Priority)
        {
            changes.Add(("Priority", ticket.Priority.ToString(), request.Priority.Value.ToString()));
            ticket.Priority = request.Priority.Value;
        }

        if (request.ParentTicketId != ticket.ParentTicketId)
        {
            changes.Add(("ParentTicketId", ticket.ParentTicketId?.ToString(), request.ParentTicketId?.ToString()));
            ticket.ParentTicketId = request.ParentTicketId;
        }

        // Handle AssignedToUserId changes
        if (request.AssignedToUserId.HasValue && request.AssignedToUserId != ticket.AssignedToUserId)
        {
            changes.Add(("AssignedToUserId", ticket.AssignedToUserId?.ToString(), request.AssignedToUserId.Value.ToString()));
            ticket.AssignedToUserId = request.AssignedToUserId.Value;
        }
        else if (request.ClearAssignee && ticket.AssignedToUserId.HasValue)
        {
            changes.Add(("AssignedToUserId", ticket.AssignedToUserId?.ToString(), null));
            ticket.AssignedToUserId = null;
        }

        ticket.UpdatedAt = DateTime.UtcNow;

        // Record history
        foreach (var (field, oldValue, newValue) in changes)
        {
            await _historyRepository.AddAsync(new TicketHistory
            {
                Id = Guid.NewGuid(),
                TicketId = ticket.Id,
                Field = field,
                OldValue = oldValue,
                NewValue = newValue,
                ChangedBy = changedBy,
                ChangedAt = DateTime.UtcNow
            }, cancellationToken);
        }

        await _ticketRepository.UpdateAsync(ticket, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var updatedTicket = await _ticketRepository.GetByKeyAsync(ticket.Key, cancellationToken);
        return _mapper.Map<TicketDto>(updatedTicket);
    }

    public async Task<TicketDto?> TransitionAsync(Guid id, TransitionTicketRequest request, string? changedBy = null, CancellationToken cancellationToken = default)
    {
        var ticket = await _ticketRepository.GetByIdAsync(id, cancellationToken);
        if (ticket == null)
            return null;

        var oldStatus = ticket.Status;
        if (oldStatus == request.Status)
        {
            var currentTicket = await _ticketRepository.GetByKeyAsync(ticket.Key, cancellationToken);
            return _mapper.Map<TicketDto>(currentTicket);
        }

        ticket.Status = request.Status;
        ticket.UpdatedAt = DateTime.UtcNow;

        // Record history
        await _historyRepository.AddAsync(new TicketHistory
        {
            Id = Guid.NewGuid(),
            TicketId = ticket.Id,
            Field = "Status",
            OldValue = oldStatus.ToString(),
            NewValue = request.Status.ToString(),
            ChangedBy = changedBy,
            ChangedAt = DateTime.UtcNow
        }, cancellationToken);

        await _ticketRepository.UpdateAsync(ticket, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var updatedTicket = await _ticketRepository.GetByKeyAsync(ticket.Key, cancellationToken);
        return _mapper.Map<TicketDto>(updatedTicket);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var ticket = await _ticketRepository.GetByIdAsync(id, cancellationToken);
        if (ticket == null)
            return false;

        await _ticketRepository.DeleteAsync(ticket, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // Sub-ticket operations

    public async Task<IEnumerable<SubTicketSummaryDto>> GetSubTicketsAsync(Guid parentTicketId, CancellationToken cancellationToken = default)
    {
        var subTickets = await _ticketRepository.GetSubTicketsAsync(parentTicketId, cancellationToken);
        return _mapper.Map<IEnumerable<SubTicketSummaryDto>>(subTickets);
    }

    public async Task<TicketDto> CreateSubTicketAsync(Guid parentTicketId, CreateSubTicketRequest request, CancellationToken cancellationToken = default)
    {
        var parent = await _ticketRepository.GetByIdAsync(parentTicketId, cancellationToken)
            ?? throw new InvalidOperationException($"Parent ticket {parentTicketId} not found");

        // Check nesting depth (max 2 levels - parent -> child, no grandchildren)
        if (parent.ParentTicketId.HasValue)
            throw new InvalidOperationException("Cannot create sub-ticket of a sub-ticket (max depth is 2)");

        // Get project for ticket key
        var project = await _projectRepository.GetByIdAsync(parent.ProjectId, cancellationToken)
            ?? throw new InvalidOperationException($"Project {parent.ProjectId} not found");

        var ticketNumber = await _ticketRepository.GetNextTicketNumberAsync(parent.ProjectId, cancellationToken);

        var ticket = new Ticket
        {
            Id = Guid.NewGuid(),
            ProjectId = parent.ProjectId,
            ParentTicketId = parentTicketId,
            Number = ticketNumber,
            Key = $"{project.Key}-{ticketNumber}",
            Title = request.Title,
            Description = request.Description,
            Type = request.Type,
            Priority = request.Priority,
            Status = TicketStatus.ToDo,
            CreatedByUserId = _userContext.UserId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Update project's next ticket number
        await _projectRepository.GetAndIncrementNextTicketNumberAsync(parent.ProjectId, cancellationToken);

        await _ticketRepository.AddAsync(ticket, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Reload with project info
        ticket.Project = project;
        return _mapper.Map<TicketDto>(ticket);
    }

    public async Task<TicketDto?> MoveTicketAsync(Guid ticketId, MoveSubTicketRequest request, string? changedBy = null, CancellationToken cancellationToken = default)
    {
        var ticket = await _ticketRepository.GetByIdAsync(ticketId, cancellationToken);
        if (ticket == null)
            return null;

        if (request.NewParentTicketId.HasValue)
        {
            // Validate no circular reference and depth limit
            if (!await CanBeParentAsync(ticketId, request.NewParentTicketId.Value, cancellationToken))
                throw new InvalidOperationException("Invalid parent: would create circular reference or exceed depth limit");
        }

        var oldParentId = ticket.ParentTicketId;
        ticket.ParentTicketId = request.NewParentTicketId;
        ticket.UpdatedAt = DateTime.UtcNow;

        // Record history
        await _historyRepository.AddAsync(new TicketHistory
        {
            Id = Guid.NewGuid(),
            TicketId = ticket.Id,
            Field = "ParentTicketId",
            OldValue = oldParentId?.ToString(),
            NewValue = request.NewParentTicketId?.ToString(),
            ChangedBy = changedBy,
            ChangedAt = DateTime.UtcNow
        }, cancellationToken);

        await _ticketRepository.UpdateAsync(ticket, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var updatedTicket = await _ticketRepository.GetByKeyAsync(ticket.Key, cancellationToken);
        return _mapper.Map<TicketDto>(updatedTicket);
    }

    public async Task<bool> CanBeParentAsync(Guid ticketId, Guid proposedParentId, CancellationToken cancellationToken = default)
    {
        // Cannot be own parent
        if (ticketId == proposedParentId)
            return false;

        // Check proposed parent isn't already a sub-ticket (max depth = 2)
        var proposedParent = await _ticketRepository.GetByIdAsync(proposedParentId, cancellationToken);
        if (proposedParent?.ParentTicketId.HasValue == true)
            return false;

        // Check for circular reference
        return !await _ticketRepository.HasCircularReferenceAsync(ticketId, proposedParentId, cancellationToken);
    }
}
