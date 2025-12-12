using AutoMapper;
using AiForge.Application.DTOs.Tickets;
using AiForge.Domain.Entities;
using AiForge.Domain.Enums;
using AiForge.Domain.Interfaces;
using AiForge.Infrastructure.Repositories;

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
}

public class TicketService : ITicketService
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly ITicketHistoryRepository _historyRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public TicketService(
        ITicketRepository ticketRepository,
        IProjectRepository projectRepository,
        ITicketHistoryRepository historyRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _ticketRepository = ticketRepository;
        _projectRepository = projectRepository;
        _historyRepository = historyRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
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
}
