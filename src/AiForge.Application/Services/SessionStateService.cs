using System.Text.Json;
using AiForge.Application.DTOs.SessionState;
using AiForge.Domain.Entities;
using AiForge.Domain.Enums;
using AiForge.Domain.Interfaces;
using AiForge.Infrastructure.Repositories;

namespace AiForge.Application.Services;

public interface ISessionStateService
{
    Task<SessionStateDto> SaveAsync(SaveSessionStateRequest request, CancellationToken cancellationToken = default);
    Task<SessionStateDto?> LoadAsync(string sessionId, CancellationToken cancellationToken = default);
    Task<bool> ClearAsync(string sessionId, CancellationToken cancellationToken = default);
    Task<int> CleanupExpiredAsync(CancellationToken cancellationToken = default);
}

public class SessionStateService : ISessionStateService
{
    private readonly ISessionStateRepository _repository;
    private readonly ITicketRepository _ticketRepository;
    private readonly IUnitOfWork _unitOfWork;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public SessionStateService(
        ISessionStateRepository repository,
        ITicketRepository ticketRepository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _ticketRepository = ticketRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<SessionStateDto> SaveAsync(SaveSessionStateRequest request, CancellationToken cancellationToken = default)
    {
        // Check if session already exists (upsert logic)
        var existing = await _repository.GetBySessionIdAsync(request.SessionId, cancellationToken);

        if (existing != null)
        {
            // Update existing session
            existing.TicketId = request.TicketId;
            existing.QueueId = request.QueueId;
            existing.CurrentPhase = Enum.TryParse<SessionPhase>(request.CurrentPhase, true, out var phase)
                ? phase
                : SessionPhase.Researching;
            existing.WorkingSummary = request.WorkingSummary;
            existing.LastCheckpoint = request.Checkpoint != null
                ? JsonSerializer.Serialize(request.Checkpoint, JsonOptions)
                : null;
            existing.UpdatedAt = DateTime.UtcNow;
            existing.ExpiresAt = DateTime.UtcNow.AddHours(request.ExpiresInHours);

            await _repository.UpdateAsync(existing, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return await MapToDtoAsync(existing, cancellationToken);
        }

        // Create new session state
        var sessionState = new SessionState
        {
            Id = Guid.NewGuid(),
            SessionId = request.SessionId,
            TicketId = request.TicketId,
            QueueId = request.QueueId,
            CurrentPhase = Enum.TryParse<SessionPhase>(request.CurrentPhase, true, out var newPhase)
                ? newPhase
                : SessionPhase.Researching,
            WorkingSummary = request.WorkingSummary,
            LastCheckpoint = request.Checkpoint != null
                ? JsonSerializer.Serialize(request.Checkpoint, JsonOptions)
                : null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(request.ExpiresInHours)
        };

        await _repository.AddAsync(sessionState, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await MapToDtoAsync(sessionState, cancellationToken);
    }

    public async Task<SessionStateDto?> LoadAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        var sessionState = await _repository.GetBySessionIdAsync(sessionId, cancellationToken);

        if (sessionState == null)
            return null;

        return await MapToDtoAsync(sessionState, cancellationToken);
    }

    public async Task<bool> ClearAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        var sessionState = await _repository.GetBySessionIdAsync(sessionId, cancellationToken);

        if (sessionState == null)
            return false;

        await _repository.DeleteAsync(sessionState, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<int> CleanupExpiredAsync(CancellationToken cancellationToken = default)
    {
        var expiredStates = await _repository.GetExpiredAsync(DateTime.UtcNow, cancellationToken);
        var expiredList = expiredStates.ToList();

        if (expiredList.Count == 0)
            return 0;

        await _repository.DeleteRangeAsync(expiredList, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return expiredList.Count;
    }

    private async Task<SessionStateDto> MapToDtoAsync(SessionState state, CancellationToken cancellationToken)
    {
        string? ticketKey = null;
        if (state.TicketId.HasValue)
        {
            var ticket = state.Ticket ?? await _ticketRepository.GetByIdAsync(state.TicketId.Value, cancellationToken);
            ticketKey = ticket?.Key;
        }

        return new SessionStateDto
        {
            Id = state.Id,
            SessionId = state.SessionId,
            TicketId = state.TicketId,
            TicketKey = ticketKey,
            QueueId = state.QueueId,
            QueueName = state.Queue?.Name,
            CurrentPhase = state.CurrentPhase.ToString(),
            WorkingSummary = state.WorkingSummary,
            Checkpoint = string.IsNullOrEmpty(state.LastCheckpoint)
                ? null
                : JsonSerializer.Deserialize<Dictionary<string, object>>(state.LastCheckpoint, JsonOptions),
            CreatedAt = state.CreatedAt,
            UpdatedAt = state.UpdatedAt,
            ExpiresAt = state.ExpiresAt
        };
    }
}
