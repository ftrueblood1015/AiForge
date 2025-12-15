using AiForge.Application.DTOs.Estimation;
using AiForge.Domain.Entities;
using AiForge.Domain.Enums;
using AiForge.Domain.Interfaces;
using AiForge.Infrastructure.Repositories;

namespace AiForge.Application.Services;

public interface IEffortEstimationService
{
    Task<EffortEstimationDto> CreateEstimationAsync(Guid ticketId, CreateEstimationRequest request, CancellationToken cancellationToken = default);
    Task<EffortEstimationDto> ReviseEstimationAsync(Guid ticketId, ReviseEstimationRequest request, CancellationToken cancellationToken = default);
    Task<EffortEstimationDto?> RecordActualEffortAsync(Guid ticketId, RecordActualEffortRequest request, CancellationToken cancellationToken = default);
    Task<EffortEstimationDto?> GetLatestEstimationAsync(Guid ticketId, CancellationToken cancellationToken = default);
    Task<EstimationHistoryResponse> GetEstimationHistoryAsync(Guid ticketId, CancellationToken cancellationToken = default);
}

public class EffortEstimationService : IEffortEstimationService
{
    private readonly IEffortEstimationRepository _estimationRepository;
    private readonly ITicketRepository _ticketRepository;
    private readonly IUnitOfWork _unitOfWork;

    public EffortEstimationService(
        IEffortEstimationRepository estimationRepository,
        ITicketRepository ticketRepository,
        IUnitOfWork unitOfWork)
    {
        _estimationRepository = estimationRepository;
        _ticketRepository = ticketRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<EffortEstimationDto> CreateEstimationAsync(Guid ticketId, CreateEstimationRequest request, CancellationToken cancellationToken = default)
    {
        // Verify ticket exists
        var ticket = await _ticketRepository.GetByIdAsync(ticketId, cancellationToken)
            ?? throw new InvalidOperationException($"Ticket with ID '{ticketId}' not found");

        // Check if estimation already exists
        var existing = await _estimationRepository.GetLatestByTicketIdAsync(ticketId, cancellationToken);
        if (existing != null)
        {
            throw new InvalidOperationException($"Estimation already exists for ticket '{ticketId}'. Use revise endpoint to update.");
        }

        var estimation = new EffortEstimation
        {
            Id = Guid.NewGuid(),
            TicketId = ticketId,
            Complexity = Enum.Parse<ComplexityLevel>(request.Complexity, ignoreCase: true),
            EstimatedEffort = Enum.Parse<EffortSize>(request.EstimatedEffort, ignoreCase: true),
            ConfidencePercent = request.ConfidencePercent,
            EstimationReasoning = request.EstimationReasoning,
            Assumptions = request.Assumptions,
            SessionId = request.SessionId,
            Version = 1,
            IsLatest = true,
            CreatedAt = DateTime.UtcNow
        };

        await _estimationRepository.AddAsync(estimation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(estimation);
    }

    public async Task<EffortEstimationDto> ReviseEstimationAsync(Guid ticketId, ReviseEstimationRequest request, CancellationToken cancellationToken = default)
    {
        // Verify ticket exists
        var ticket = await _ticketRepository.GetByIdAsync(ticketId, cancellationToken)
            ?? throw new InvalidOperationException($"Ticket with ID '{ticketId}' not found");

        // Mark existing estimation as not latest
        var existing = await _estimationRepository.GetLatestByTicketIdAsync(ticketId, cancellationToken);
        if (existing != null)
        {
            existing.IsLatest = false;
            await _estimationRepository.UpdateAsync(existing, cancellationToken);
        }

        // Get next version number
        var nextVersion = await _estimationRepository.GetNextVersionAsync(ticketId, cancellationToken);

        var estimation = new EffortEstimation
        {
            Id = Guid.NewGuid(),
            TicketId = ticketId,
            Complexity = Enum.Parse<ComplexityLevel>(request.Complexity, ignoreCase: true),
            EstimatedEffort = Enum.Parse<EffortSize>(request.EstimatedEffort, ignoreCase: true),
            ConfidencePercent = request.ConfidencePercent,
            EstimationReasoning = request.EstimationReasoning,
            Assumptions = request.Assumptions,
            RevisionReason = request.RevisionReason,
            SessionId = request.SessionId,
            Version = nextVersion,
            IsLatest = true,
            CreatedAt = DateTime.UtcNow
        };

        await _estimationRepository.AddAsync(estimation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(estimation);
    }

    public async Task<EffortEstimationDto?> RecordActualEffortAsync(Guid ticketId, RecordActualEffortRequest request, CancellationToken cancellationToken = default)
    {
        var estimation = await _estimationRepository.GetLatestByTicketIdAsync(ticketId, cancellationToken);
        if (estimation == null)
        {
            return null;
        }

        estimation.ActualEffort = Enum.Parse<EffortSize>(request.ActualEffort, ignoreCase: true);
        estimation.VarianceNotes = request.VarianceNotes;

        await _estimationRepository.UpdateAsync(estimation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(estimation);
    }

    public async Task<EffortEstimationDto?> GetLatestEstimationAsync(Guid ticketId, CancellationToken cancellationToken = default)
    {
        var estimation = await _estimationRepository.GetLatestByTicketIdAsync(ticketId, cancellationToken);
        return estimation == null ? null : MapToDto(estimation);
    }

    public async Task<EstimationHistoryResponse> GetEstimationHistoryAsync(Guid ticketId, CancellationToken cancellationToken = default)
    {
        var estimations = await _estimationRepository.GetAllByTicketIdAsync(ticketId, cancellationToken);
        var estimationList = estimations.ToList();

        return new EstimationHistoryResponse
        {
            TicketId = ticketId,
            Estimations = estimationList.Select(MapToDto).ToList(),
            TotalVersions = estimationList.Count
        };
    }

    private static EffortEstimationDto MapToDto(EffortEstimation estimation)
    {
        return new EffortEstimationDto
        {
            Id = estimation.Id,
            TicketId = estimation.TicketId,
            Complexity = estimation.Complexity.ToString(),
            EstimatedEffort = estimation.EstimatedEffort.ToString(),
            ConfidencePercent = estimation.ConfidencePercent,
            EstimationReasoning = estimation.EstimationReasoning,
            Assumptions = estimation.Assumptions,
            ActualEffort = estimation.ActualEffort?.ToString(),
            VarianceNotes = estimation.VarianceNotes,
            Version = estimation.Version,
            RevisionReason = estimation.RevisionReason,
            SessionId = estimation.SessionId,
            IsLatest = estimation.IsLatest,
            CreatedAt = estimation.CreatedAt
        };
    }
}
