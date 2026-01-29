using System.Text.Json;
using AiForge.Application.DTOs.Planning;
using AiForge.Domain.Entities;
using AiForge.Domain.Enums;
using AiForge.Domain.Interfaces;
using AiForge.Infrastructure.Repositories;

namespace AiForge.Application.Services;

public interface ISummaryService
{
    /// <summary>
    /// Updates the progress summary on a ticket based on recent progress entries.
    /// Called after new progress entries are created.
    /// </summary>
    Task UpdateProgressSummaryAsync(Guid ticketId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the decision summary on a ticket based on recent reasoning logs.
    /// Called after new reasoning logs are created.
    /// </summary>
    Task UpdateDecisionSummaryAsync(Guid ticketId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a summarized view of planning data, optimized for reduced token consumption.
    /// </summary>
    Task<PlanningDataSummaryDto> GetPlanningDataSummaryAsync(Guid ticketId, int recentCount = 5, CancellationToken cancellationToken = default);
}

public class SummaryService : ISummaryService
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IProgressEntryRepository _progressRepository;
    private readonly IReasoningLogRepository _reasoningRepository;
    private readonly IPlanningSessionRepository _sessionRepository;
    private readonly IUnitOfWork _unitOfWork;

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public SummaryService(
        ITicketRepository ticketRepository,
        IProgressEntryRepository progressRepository,
        IReasoningLogRepository reasoningRepository,
        IPlanningSessionRepository sessionRepository,
        IUnitOfWork unitOfWork)
    {
        _ticketRepository = ticketRepository;
        _progressRepository = progressRepository;
        _reasoningRepository = reasoningRepository;
        _sessionRepository = sessionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task UpdateProgressSummaryAsync(Guid ticketId, CancellationToken cancellationToken = default)
    {
        var ticket = await _ticketRepository.GetByIdAsync(ticketId, cancellationToken);
        if (ticket == null) return;

        var entries = await _progressRepository.GetByTicketIdAsync(ticketId, null, cancellationToken);
        var entriesList = entries.ToList();

        // Calculate outcome statistics
        var stats = new OutcomeStatisticsDto
        {
            Success = entriesList.Count(e => e.Outcome == ProgressOutcome.Success),
            Failure = entriesList.Count(e => e.Outcome == ProgressOutcome.Failure),
            Partial = entriesList.Count(e => e.Outcome == ProgressOutcome.Partial),
            Blocked = entriesList.Count(e => e.Outcome == ProgressOutcome.Blocked)
        };

        ticket.OutcomeStatistics = JsonSerializer.Serialize(stats, JsonOptions);

        // Generate progress summary
        var mostRecent = entriesList.OrderByDescending(e => e.CreatedAt).FirstOrDefault();
        var blockedEntries = entriesList.Where(e => e.Outcome == ProgressOutcome.Blocked).ToList();

        var summaryParts = new List<string>();

        // Current phase from most recent entry
        if (mostRecent != null)
        {
            var currentPhase = mostRecent.Content.Length > 50
                ? mostRecent.Content[..50] + "..."
                : mostRecent.Content;
            summaryParts.Add(currentPhase);
        }

        // Outcome summary
        var outcomeSummary = $"{stats.Success} completed";
        if (stats.Failure > 0) outcomeSummary += $", {stats.Failure} failed";
        if (stats.Partial > 0) outcomeSummary += $", {stats.Partial} partial";
        summaryParts.Add(outcomeSummary);

        // Blocker info
        if (blockedEntries.Any())
        {
            var lastBlocked = blockedEntries.OrderByDescending(e => e.CreatedAt).First();
            var blockerContent = lastBlocked.Content.Length > 30
                ? lastBlocked.Content[..30] + "..."
                : lastBlocked.Content;
            summaryParts.Add($"Blocked: {blockerContent}");
        }

        ticket.ProgressSummary = string.Join(". ", summaryParts);
        ticket.SummaryUpdatedAt = DateTime.UtcNow;

        await _ticketRepository.UpdateAsync(ticket, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateDecisionSummaryAsync(Guid ticketId, CancellationToken cancellationToken = default)
    {
        var ticket = await _ticketRepository.GetByIdAsync(ticketId, cancellationToken);
        if (ticket == null) return;

        var logs = await _reasoningRepository.GetByTicketIdAsync(ticketId, null, cancellationToken);
        var logsList = logs.ToList();

        if (!logsList.Any())
        {
            ticket.DecisionSummary = "No decisions logged yet.";
            ticket.SummaryUpdatedAt = DateTime.UtcNow;
            await _ticketRepository.UpdateAsync(ticket, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return;
        }

        // Get high-confidence decisions (>70%)
        var keyDecisions = logsList
            .Where(l => l.ConfidencePercent.HasValue && l.ConfidencePercent.Value > 70)
            .OrderByDescending(l => l.ConfidencePercent)
            .ThenByDescending(l => l.CreatedAt)
            .Take(2)
            .Select(l => $"{l.DecisionPoint}: {l.ChosenOption}")
            .ToList();

        var summaryParts = new List<string>
        {
            $"{logsList.Count} decisions made"
        };

        if (keyDecisions.Any())
        {
            summaryParts.Add("Key: " + string.Join("; ", keyDecisions));
        }

        ticket.DecisionSummary = string.Join(". ", summaryParts);
        ticket.SummaryUpdatedAt = DateTime.UtcNow;

        await _ticketRepository.UpdateAsync(ticket, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<PlanningDataSummaryDto> GetPlanningDataSummaryAsync(Guid ticketId, int recentCount = 5, CancellationToken cancellationToken = default)
    {
        var ticket = await _ticketRepository.GetByIdAsync(ticketId, cancellationToken);

        // Get all data for counts and recent items
        var sessions = (await _sessionRepository.GetByTicketIdAsync(ticketId, cancellationToken)).ToList();
        var logs = (await _reasoningRepository.GetByTicketIdAsync(ticketId, null, cancellationToken)).ToList();
        var entries = (await _progressRepository.GetByTicketIdAsync(ticketId, null, cancellationToken)).ToList();

        // Parse outcome statistics from ticket (if available)
        OutcomeStatisticsDto? outcomeStats = null;
        if (!string.IsNullOrWhiteSpace(ticket?.OutcomeStatistics))
        {
            try
            {
                outcomeStats = JsonSerializer.Deserialize<OutcomeStatisticsDto>(ticket.OutcomeStatistics, JsonOptions);
            }
            catch
            {
                // Calculate from entries if deserialization fails
                outcomeStats = CalculateOutcomeStats(entries);
            }
        }
        else if (entries.Any())
        {
            outcomeStats = CalculateOutcomeStats(entries);
        }

        // Get recent items
        var recentProgress = entries
            .OrderByDescending(e => e.CreatedAt)
            .Take(recentCount)
            .Select(MapProgressEntryToDto)
            .ToList();

        var recentDecisions = logs
            .OrderByDescending(l => l.CreatedAt)
            .Take(recentCount)
            .Select(MapReasoningLogToDto)
            .ToList();

        return new PlanningDataSummaryDto
        {
            ProgressSummary = ticket?.ProgressSummary,
            DecisionSummary = ticket?.DecisionSummary,
            OutcomeStatistics = outcomeStats,
            TotalProgressEntries = entries.Count,
            TotalReasoningLogs = logs.Count,
            TotalSessions = sessions.Count,
            LastProgressEntry = recentProgress.FirstOrDefault(),
            LastDecision = recentDecisions.FirstOrDefault(),
            RecentProgress = recentProgress,
            RecentDecisions = recentDecisions,
            SummaryUpdatedAt = ticket?.SummaryUpdatedAt,
            FullHistoryAvailable = true
        };
    }

    private static OutcomeStatisticsDto CalculateOutcomeStats(List<ProgressEntry> entries)
    {
        return new OutcomeStatisticsDto
        {
            Success = entries.Count(e => e.Outcome == ProgressOutcome.Success),
            Failure = entries.Count(e => e.Outcome == ProgressOutcome.Failure),
            Partial = entries.Count(e => e.Outcome == ProgressOutcome.Partial),
            Blocked = entries.Count(e => e.Outcome == ProgressOutcome.Blocked)
        };
    }

    private static ProgressEntryDto MapProgressEntryToDto(ProgressEntry entry)
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

    private static ReasoningLogDto MapReasoningLogToDto(ReasoningLog log)
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
}
