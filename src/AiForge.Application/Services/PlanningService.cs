using System.Text.Json;
using AutoMapper;
using AiForge.Application.DTOs.Planning;
using AiForge.Domain.Entities;
using AiForge.Domain.Interfaces;
using AiForge.Infrastructure.Repositories;

namespace AiForge.Application.Services;

public interface IPlanningService
{
    // Planning Sessions
    Task<PlanningSessionDto?> GetSessionByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<PlanningSessionDto>> GetSessionsByTicketIdAsync(Guid ticketId, CancellationToken cancellationToken = default);
    Task<PlanningSessionDto> CreateSessionAsync(CreatePlanningSessionRequest request, CancellationToken cancellationToken = default);
    Task<PlanningSessionDto?> UpdateSessionAsync(Guid id, UpdatePlanningSessionRequest request, CancellationToken cancellationToken = default);
    Task<PlanningSessionDto?> CompleteSessionAsync(Guid id, CompletePlanningSessionRequest request, CancellationToken cancellationToken = default);

    // Reasoning Logs
    Task<ReasoningLogDto> CreateReasoningLogAsync(CreateReasoningLogRequest request, CancellationToken cancellationToken = default);
    Task<IEnumerable<ReasoningLogDto>> GetReasoningLogsByTicketIdAsync(Guid ticketId, int? limit = null, CancellationToken cancellationToken = default);

    // Progress Entries
    Task<ProgressEntryDto> CreateProgressEntryAsync(CreateProgressEntryRequest request, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProgressEntryDto>> GetProgressEntriesByTicketIdAsync(Guid ticketId, int? limit = null, CancellationToken cancellationToken = default);

    // Aggregated Planning Data
    Task<PlanningDataDto> GetPlanningDataByTicketIdAsync(Guid ticketId, CancellationToken cancellationToken = default);
}

public class PlanningDataDto
{
    public IEnumerable<PlanningSessionDto> Sessions { get; set; } = new List<PlanningSessionDto>();
    public IEnumerable<ReasoningLogDto> ReasoningLogs { get; set; } = new List<ReasoningLogDto>();
    public IEnumerable<ProgressEntryDto> ProgressEntries { get; set; } = new List<ProgressEntryDto>();
}

public class PlanningService : IPlanningService
{
    private readonly IPlanningSessionRepository _sessionRepository;
    private readonly IReasoningLogRepository _reasoningRepository;
    private readonly IProgressEntryRepository _progressRepository;
    private readonly ITicketRepository _ticketRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ISummaryService _summaryService;

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public PlanningService(
        IPlanningSessionRepository sessionRepository,
        IReasoningLogRepository reasoningRepository,
        IProgressEntryRepository progressRepository,
        ITicketRepository ticketRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ISummaryService summaryService)
    {
        _sessionRepository = sessionRepository;
        _reasoningRepository = reasoningRepository;
        _progressRepository = progressRepository;
        _ticketRepository = ticketRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _summaryService = summaryService;
    }

    #region Planning Sessions

    public async Task<PlanningSessionDto?> GetSessionByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var session = await _sessionRepository.GetByIdAsync(id, cancellationToken);
        return session == null ? null : MapSessionToDto(session);
    }

    public async Task<IEnumerable<PlanningSessionDto>> GetSessionsByTicketIdAsync(Guid ticketId, CancellationToken cancellationToken = default)
    {
        var sessions = await _sessionRepository.GetByTicketIdAsync(ticketId, cancellationToken);
        return sessions.Select(MapSessionToDto);
    }

    public async Task<PlanningSessionDto> CreateSessionAsync(CreatePlanningSessionRequest request, CancellationToken cancellationToken = default)
    {
        // Verify ticket exists
        var ticket = await _ticketRepository.GetByIdAsync(request.TicketId, cancellationToken)
            ?? throw new InvalidOperationException($"Ticket with ID '{request.TicketId}' not found");

        var session = new PlanningSession
        {
            Id = Guid.NewGuid(),
            TicketId = request.TicketId,
            SessionId = request.SessionId,
            InitialUnderstanding = request.InitialUnderstanding,
            Assumptions = request.Assumptions != null ? JsonSerializer.Serialize(request.Assumptions, JsonOptions) : null,
            CreatedAt = DateTime.UtcNow
        };

        await _sessionRepository.AddAsync(session, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapSessionToDto(session);
    }

    public async Task<PlanningSessionDto?> UpdateSessionAsync(Guid id, UpdatePlanningSessionRequest request, CancellationToken cancellationToken = default)
    {
        var session = await _sessionRepository.GetByIdAsync(id, cancellationToken);
        if (session == null)
            return null;

        if (request.Assumptions != null)
            session.Assumptions = JsonSerializer.Serialize(request.Assumptions, JsonOptions);

        if (request.AlternativesConsidered != null)
            session.AlternativesConsidered = JsonSerializer.Serialize(request.AlternativesConsidered, JsonOptions);

        if (request.ChosenApproach != null)
            session.ChosenApproach = request.ChosenApproach;

        if (request.Rationale != null)
            session.Rationale = request.Rationale;

        await _sessionRepository.UpdateAsync(session, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapSessionToDto(session);
    }

    public async Task<PlanningSessionDto?> CompleteSessionAsync(Guid id, CompletePlanningSessionRequest request, CancellationToken cancellationToken = default)
    {
        var session = await _sessionRepository.GetByIdAsync(id, cancellationToken);
        if (session == null)
            return null;

        if (request.ChosenApproach != null)
            session.ChosenApproach = request.ChosenApproach;

        if (request.Rationale != null)
            session.Rationale = request.Rationale;

        session.CompletedAt = DateTime.UtcNow;

        await _sessionRepository.UpdateAsync(session, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapSessionToDto(session);
    }

    private PlanningSessionDto MapSessionToDto(PlanningSession session)
    {
        return new PlanningSessionDto
        {
            Id = session.Id,
            TicketId = session.TicketId,
            SessionId = session.SessionId,
            InitialUnderstanding = session.InitialUnderstanding,
            Assumptions = DeserializeList(session.Assumptions),
            AlternativesConsidered = DeserializeList(session.AlternativesConsidered),
            ChosenApproach = session.ChosenApproach,
            Rationale = session.Rationale,
            CreatedAt = session.CreatedAt,
            CompletedAt = session.CompletedAt
        };
    }

    #endregion

    #region Reasoning Logs

    public async Task<ReasoningLogDto> CreateReasoningLogAsync(CreateReasoningLogRequest request, CancellationToken cancellationToken = default)
    {
        // Verify ticket exists
        var ticket = await _ticketRepository.GetByIdAsync(request.TicketId, cancellationToken)
            ?? throw new InvalidOperationException($"Ticket with ID '{request.TicketId}' not found");

        var log = new ReasoningLog
        {
            Id = Guid.NewGuid(),
            TicketId = request.TicketId,
            SessionId = request.SessionId,
            DecisionPoint = request.DecisionPoint,
            OptionsConsidered = request.OptionsConsidered != null ? JsonSerializer.Serialize(request.OptionsConsidered, JsonOptions) : null,
            ChosenOption = request.ChosenOption,
            Rationale = request.Rationale,
            ConfidencePercent = request.ConfidencePercent,
            CreatedAt = DateTime.UtcNow
        };

        await _reasoningRepository.AddAsync(log, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Update ticket decision summary
        await _summaryService.UpdateDecisionSummaryAsync(request.TicketId, cancellationToken);

        return MapReasoningLogToDto(log);
    }

    public async Task<IEnumerable<ReasoningLogDto>> GetReasoningLogsByTicketIdAsync(Guid ticketId, int? limit = null, CancellationToken cancellationToken = default)
    {
        var logs = await _reasoningRepository.GetByTicketIdAsync(ticketId, limit, cancellationToken);
        return logs.Select(MapReasoningLogToDto);
    }

    private ReasoningLogDto MapReasoningLogToDto(ReasoningLog log)
    {
        return new ReasoningLogDto
        {
            Id = log.Id,
            TicketId = log.TicketId,
            SessionId = log.SessionId,
            DecisionPoint = log.DecisionPoint,
            OptionsConsidered = DeserializeList(log.OptionsConsidered),
            ChosenOption = log.ChosenOption,
            Rationale = log.Rationale,
            ConfidencePercent = log.ConfidencePercent,
            CreatedAt = log.CreatedAt
        };
    }

    #endregion

    #region Progress Entries

    public async Task<ProgressEntryDto> CreateProgressEntryAsync(CreateProgressEntryRequest request, CancellationToken cancellationToken = default)
    {
        // Verify ticket exists
        var ticket = await _ticketRepository.GetByIdAsync(request.TicketId, cancellationToken)
            ?? throw new InvalidOperationException($"Ticket with ID '{request.TicketId}' not found");

        var entry = new ProgressEntry
        {
            Id = Guid.NewGuid(),
            TicketId = request.TicketId,
            SessionId = request.SessionId,
            Content = request.Content,
            Outcome = request.Outcome,
            FilesAffected = request.FilesAffected != null ? JsonSerializer.Serialize(request.FilesAffected, JsonOptions) : null,
            ErrorDetails = request.ErrorDetails,
            CreatedAt = DateTime.UtcNow
        };

        await _progressRepository.AddAsync(entry, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Update ticket progress summary
        await _summaryService.UpdateProgressSummaryAsync(request.TicketId, cancellationToken);

        return MapProgressEntryToDto(entry);
    }

    public async Task<IEnumerable<ProgressEntryDto>> GetProgressEntriesByTicketIdAsync(Guid ticketId, int? limit = null, CancellationToken cancellationToken = default)
    {
        var entries = await _progressRepository.GetByTicketIdAsync(ticketId, limit, cancellationToken);
        return entries.Select(MapProgressEntryToDto);
    }

    private ProgressEntryDto MapProgressEntryToDto(ProgressEntry entry)
    {
        return new ProgressEntryDto
        {
            Id = entry.Id,
            TicketId = entry.TicketId,
            SessionId = entry.SessionId,
            Content = entry.Content,
            Outcome = entry.Outcome,
            FilesAffected = DeserializeList(entry.FilesAffected),
            ErrorDetails = entry.ErrorDetails,
            CreatedAt = entry.CreatedAt
        };
    }

    #endregion

    #region Aggregated Data

    public async Task<PlanningDataDto> GetPlanningDataByTicketIdAsync(Guid ticketId, CancellationToken cancellationToken = default)
    {
        var sessions = await GetSessionsByTicketIdAsync(ticketId, cancellationToken);
        var reasoningLogs = await GetReasoningLogsByTicketIdAsync(ticketId, null, cancellationToken);
        var progressEntries = await GetProgressEntriesByTicketIdAsync(ticketId, null, cancellationToken);

        return new PlanningDataDto
        {
            Sessions = sessions,
            ReasoningLogs = reasoningLogs,
            ProgressEntries = progressEntries
        };
    }

    #endregion

    #region Helpers

    private static List<string> DeserializeList(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return new List<string>();

        try
        {
            return JsonSerializer.Deserialize<List<string>>(json, JsonOptions) ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }

    #endregion
}
