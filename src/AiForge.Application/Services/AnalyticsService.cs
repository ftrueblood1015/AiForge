using AiForge.Application.DTOs.Analytics.Confidence;
using AiForge.Application.DTOs.Analytics.Dashboard;
using AiForge.Application.DTOs.Analytics.Patterns;
using AiForge.Application.DTOs.Analytics.Sessions;
using AiForge.Application.Extensions;
using AiForge.Application.Interfaces;
using AiForge.Domain.Entities;
using AiForge.Domain.Enums;
using AiForge.Domain.Interfaces;
using AiForge.Infrastructure.Data;
using AiForge.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AiForge.Application.Services;

public interface IAnalyticsService
{
    // Confidence Tracking
    Task<List<LowConfidenceDecisionDto>> GetLowConfidenceDecisionsAsync(LowConfidenceDecisionRequest request, CancellationToken cancellationToken = default);
    Task<ConfidenceTrendDto> GetConfidenceTrendsAsync(ConfidenceTrendRequest request, CancellationToken cancellationToken = default);
    Task<List<TicketConfidenceSummaryDto>> GetLowConfidenceTicketsAsync(TicketConfidenceSummaryRequest request, CancellationToken cancellationToken = default);

    // Pattern Detection
    Task<List<AnalyticsHotFileDto>> GetHotFilesAsync(HotFileRequest request, CancellationToken cancellationToken = default);
    Task<FileCorrelationDto> GetCorrelatedFilesAsync(FileCorrelationRequest request, CancellationToken cancellationToken = default);
    Task<List<RecurringIssueDto>> GetRecurringIssuesAsync(RecurringIssueRequest request, CancellationToken cancellationToken = default);
    Task<DebtPatternSummaryDto> GetDebtPatternSummaryAsync(DebtPatternRequest request, CancellationToken cancellationToken = default);

    // Session Analytics
    Task<SessionMetricsDto> LogSessionMetricsAsync(LogSessionMetricsRequest request, CancellationToken cancellationToken = default);
    Task<SessionMetricsDto?> UpdateSessionMetricsAsync(UpdateSessionMetricsRequest request, CancellationToken cancellationToken = default);
    Task<TicketSessionAnalyticsDto> GetTicketSessionAnalyticsAsync(Guid ticketId, CancellationToken cancellationToken = default);
    Task<ProjectSessionAnalyticsDto> GetProjectSessionAnalyticsAsync(ProjectSessionAnalyticsRequest request, CancellationToken cancellationToken = default);
    Task<ProductivityMetricsDto> GetProductivityMetricsAsync(ProductivityMetricsRequest request, CancellationToken cancellationToken = default);

    // Dashboard
    Task<AnalyticsDashboardDto> GetDashboardAsync(AnalyticsDashboardRequest request, CancellationToken cancellationToken = default);
}

public class AnalyticsService : IAnalyticsService
{
    private readonly AiForgeDbContext _context;
    private readonly ISessionMetricsRepository _sessionMetricsRepository;
    private readonly ITicketRepository _ticketRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserContext _userContext;
    private readonly IProjectMemberService _projectMemberService;

    public AnalyticsService(
        AiForgeDbContext context,
        ISessionMetricsRepository sessionMetricsRepository,
        ITicketRepository ticketRepository,
        IUnitOfWork unitOfWork,
        IUserContext userContext,
        IProjectMemberService projectMemberService)
    {
        _context = context;
        _sessionMetricsRepository = sessionMetricsRepository;
        _ticketRepository = ticketRepository;
        _unitOfWork = unitOfWork;
        _userContext = userContext;
        _projectMemberService = projectMemberService;
    }

    #region Confidence Tracking

    public async Task<List<LowConfidenceDecisionDto>> GetLowConfidenceDecisionsAsync(
        LowConfidenceDecisionRequest request,
        CancellationToken cancellationToken = default)
    {
        var accessibleProjects = await _projectMemberService.GetAccessibleProjectIdsOrNullAsync(_userContext, cancellationToken);

        var query = _context.ReasoningLogs
            .Include(r => r.Ticket)
            .ThenInclude(t => t.Project)
            .Where(r => r.ConfidencePercent.HasValue && r.ConfidencePercent < request.ConfidenceThreshold)
            .AsQueryable();

        // Apply project access filtering
        if (accessibleProjects != null)
        {
            if (request.ProjectId.HasValue)
            {
                if (!accessibleProjects.Contains(request.ProjectId.Value))
                    return new List<LowConfidenceDecisionDto>();
                query = query.Where(r => r.Ticket.ProjectId == request.ProjectId.Value);
            }
            else
            {
                query = query.Where(r => accessibleProjects.Contains(r.Ticket.ProjectId));
            }
        }
        else if (request.ProjectId.HasValue)
        {
            query = query.Where(r => r.Ticket.ProjectId == request.ProjectId.Value);
        }

        if (request.Since.HasValue)
        {
            query = query.Where(r => r.CreatedAt >= request.Since.Value);
        }

        var results = await query
            .OrderByDescending(r => r.CreatedAt)
            .Take(request.Limit ?? 50)
            .Select(r => new LowConfidenceDecisionDto
            {
                ReasoningLogId = r.Id,
                TicketId = r.TicketId,
                TicketKey = r.Ticket.Key,
                TicketTitle = r.Ticket.Title,
                DecisionPoint = r.DecisionPoint,
                ChosenOption = r.ChosenOption,
                Rationale = r.Rationale,
                ConfidencePercent = r.ConfidencePercent!.Value,
                SessionId = r.SessionId,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return results;
    }

    public async Task<ConfidenceTrendDto> GetConfidenceTrendsAsync(
        ConfidenceTrendRequest request,
        CancellationToken cancellationToken = default)
    {
        var accessibleProjects = await _projectMemberService.GetAccessibleProjectIdsOrNullAsync(_userContext, cancellationToken);

        var query = _context.ReasoningLogs
            .Include(r => r.Ticket)
            .Where(r => r.ConfidencePercent.HasValue)
            .AsQueryable();

        // Apply project access filtering
        if (accessibleProjects != null)
        {
            if (request.ProjectId.HasValue)
            {
                if (!accessibleProjects.Contains(request.ProjectId.Value))
                    return new ConfidenceTrendDto { DataPoints = new List<ConfidenceTrendPoint>() };
                query = query.Where(r => r.Ticket.ProjectId == request.ProjectId.Value);
            }
            else
            {
                query = query.Where(r => accessibleProjects.Contains(r.Ticket.ProjectId));
            }
        }
        else if (request.ProjectId.HasValue)
        {
            query = query.Where(r => r.Ticket.ProjectId == request.ProjectId.Value);
        }

        if (request.StartDate.HasValue)
        {
            query = query.Where(r => r.CreatedAt >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            query = query.Where(r => r.CreatedAt <= request.EndDate.Value);
        }

        var decisions = await query
            .Select(r => new { r.CreatedAt, Confidence = r.ConfidencePercent!.Value })
            .ToListAsync(cancellationToken);

        var dataPoints = decisions
            .GroupBy(d => d.CreatedAt.Date)
            .OrderBy(g => g.Key)
            .Select(g => new ConfidenceTrendPoint
            {
                Date = g.Key,
                AverageConfidence = g.Average(x => x.Confidence),
                DecisionCount = g.Count(),
                LowConfidenceCount = g.Count(x => x.Confidence < 50)
            })
            .ToList();

        return new ConfidenceTrendDto
        {
            DataPoints = dataPoints,
            OverallAverageConfidence = decisions.Any() ? decisions.Average(d => d.Confidence) : 0,
            TotalDecisions = decisions.Count,
            LowConfidenceCount = decisions.Count(d => d.Confidence < 50),
            StartDate = request.StartDate,
            EndDate = request.EndDate
        };
    }

    public async Task<List<TicketConfidenceSummaryDto>> GetLowConfidenceTicketsAsync(
        TicketConfidenceSummaryRequest request,
        CancellationToken cancellationToken = default)
    {
        var accessibleProjects = await _projectMemberService.GetAccessibleProjectIdsOrNullAsync(_userContext, cancellationToken);

        var query = _context.ReasoningLogs
            .Include(r => r.Ticket)
            .ThenInclude(t => t.Project)
            .Where(r => r.ConfidencePercent.HasValue)
            .AsQueryable();

        // Apply project access filtering
        if (accessibleProjects != null)
        {
            if (request.ProjectId.HasValue)
            {
                if (!accessibleProjects.Contains(request.ProjectId.Value))
                    return new List<TicketConfidenceSummaryDto>();
                query = query.Where(r => r.Ticket.ProjectId == request.ProjectId.Value);
            }
            else
            {
                query = query.Where(r => accessibleProjects.Contains(r.Ticket.ProjectId));
            }
        }
        else if (request.ProjectId.HasValue)
        {
            query = query.Where(r => r.Ticket.ProjectId == request.ProjectId.Value);
        }

        var ticketGroups = await query
            .GroupBy(r => new { r.TicketId, r.Ticket.Title, r.Ticket.Status, r.Ticket.Key })
            .Select(g => new
            {
                g.Key.TicketId,
                TicketKey = g.Key.Key,
                g.Key.Title,
                g.Key.Status,
                AvgConfidence = g.Average(r => r.ConfidencePercent!.Value),
                TotalDecisions = g.Count(),
                LowConfidenceCount = g.Count(r => r.ConfidencePercent < request.ConfidenceThreshold),
                LowestConfidence = g.Min(r => r.ConfidencePercent),
                LastDecisionAt = g.Max(r => r.CreatedAt)
            })
            .Where(x => x.AvgConfidence < request.ConfidenceThreshold)
            .OrderBy(x => x.AvgConfidence)
            .Take(request.Limit ?? 20)
            .ToListAsync(cancellationToken);

        return ticketGroups.Select(g => new TicketConfidenceSummaryDto
        {
            TicketId = g.TicketId,
            TicketKey = g.TicketKey,
            TicketTitle = g.Title,
            TicketStatus = g.Status.ToString(),
            AverageConfidence = g.AvgConfidence,
            TotalDecisions = g.TotalDecisions,
            LowConfidenceDecisions = g.LowConfidenceCount,
            LowestConfidence = g.LowestConfidence,
            LastDecisionAt = g.LastDecisionAt
        }).ToList();
    }

    #endregion

    #region Pattern Detection

    public async Task<List<AnalyticsHotFileDto>> GetHotFilesAsync(
        HotFileRequest request,
        CancellationToken cancellationToken = default)
    {
        var accessibleProjects = await _projectMemberService.GetAccessibleProjectIdsOrNullAsync(_userContext, cancellationToken);

        var query = _context.FileChanges
            .Include(f => f.Ticket)
            .ThenInclude(t => t.Project)
            .AsQueryable();

        // Apply project access filtering
        if (accessibleProjects != null)
        {
            if (request.ProjectId.HasValue)
            {
                if (!accessibleProjects.Contains(request.ProjectId.Value))
                    return new List<AnalyticsHotFileDto>();
                query = query.Where(f => f.Ticket.ProjectId == request.ProjectId.Value);
            }
            else
            {
                query = query.Where(f => accessibleProjects.Contains(f.Ticket.ProjectId));
            }
        }
        else if (request.ProjectId.HasValue)
        {
            query = query.Where(f => f.Ticket.ProjectId == request.ProjectId.Value);
        }

        if (request.Since.HasValue)
        {
            query = query.Where(f => f.CreatedAt >= request.Since.Value);
        }

        var fileGroups = await query
            .GroupBy(f => f.FilePath)
            .Select(g => new
            {
                FilePath = g.Key,
                ModificationCount = g.Count(),
                TicketCount = g.Select(f => f.TicketId).Distinct().Count(),
                TotalLinesAdded = g.Sum(f => f.LinesAdded ?? 0),
                TotalLinesRemoved = g.Sum(f => f.LinesRemoved ?? 0),
                FirstModified = g.Min(f => f.CreatedAt),
                LastModified = g.Max(f => f.CreatedAt),
                RecentTicketKeys = g.OrderByDescending(f => f.CreatedAt)
                    .Select(f => f.Ticket.Key)
                    .Take(5)
                    .ToList()
            })
            .OrderByDescending(x => x.ModificationCount)
            .Take(request.TopN)
            .ToListAsync(cancellationToken);

        return fileGroups.Select(g => new AnalyticsHotFileDto
        {
            FilePath = g.FilePath,
            ModificationCount = g.ModificationCount,
            TicketCount = g.TicketCount,
            TotalLinesAdded = g.TotalLinesAdded,
            TotalLinesRemoved = g.TotalLinesRemoved,
            FirstModified = g.FirstModified,
            LastModified = g.LastModified,
            RecentTicketKeys = g.RecentTicketKeys
        }).ToList();
    }

    public async Task<FileCorrelationDto> GetCorrelatedFilesAsync(
        FileCorrelationRequest request,
        CancellationToken cancellationToken = default)
    {
        // Get all tickets that modified the target file
        var ticketIds = await _context.FileChanges
            .Where(f => f.FilePath == request.FilePath)
            .Select(f => f.TicketId)
            .Distinct()
            .ToListAsync(cancellationToken);

        // Find other files modified in those tickets
        var correlatedFiles = await _context.FileChanges
            .Where(f => ticketIds.Contains(f.TicketId) && f.FilePath != request.FilePath)
            .GroupBy(f => f.FilePath)
            .Select(g => new
            {
                FilePath = g.Key,
                CooccurrenceCount = g.Select(f => f.TicketId).Distinct().Count()
            })
            .Where(x => x.CooccurrenceCount >= request.MinCooccurrence)
            .OrderByDescending(x => x.CooccurrenceCount)
            .Take(request.TopN)
            .ToListAsync(cancellationToken);

        var totalTickets = ticketIds.Count;

        return new FileCorrelationDto
        {
            FilePath = request.FilePath,
            CorrelatedFiles = correlatedFiles.Select(cf => new CorrelatedFile
            {
                FilePath = cf.FilePath,
                CooccurrenceCount = cf.CooccurrenceCount,
                CorrelationStrength = totalTickets > 0 ? (double)cf.CooccurrenceCount / totalTickets : 0
            }).ToList()
        };
    }

    public async Task<List<RecurringIssueDto>> GetRecurringIssuesAsync(
        RecurringIssueRequest request,
        CancellationToken cancellationToken = default)
    {
        var accessibleProjects = await _projectMemberService.GetAccessibleProjectIdsOrNullAsync(_userContext, cancellationToken);

        var query = _context.Tickets
            .Include(t => t.Project)
            .Where(t => t.Type == TicketType.Bug)
            .AsQueryable();

        // Apply project access filtering
        if (accessibleProjects != null)
        {
            if (request.ProjectId.HasValue)
            {
                if (!accessibleProjects.Contains(request.ProjectId.Value))
                    return new List<RecurringIssueDto>();
                query = query.Where(t => t.ProjectId == request.ProjectId.Value);
            }
            else
            {
                query = query.Where(t => accessibleProjects.Contains(t.ProjectId));
            }
        }
        else if (request.ProjectId.HasValue)
        {
            query = query.Where(t => t.ProjectId == request.ProjectId.Value);
        }

        // Get all bug tickets to find patterns
        var tickets = await query
            .Select(t => new
            {
                t.Id,
                TicketKey = t.Key,
                t.Title,
                Status = t.Status.ToString(),
                t.CreatedAt
            })
            .ToListAsync(cancellationToken);

        // Simple pattern detection: group by common words in titles
        var patterns = new Dictionary<string, List<RelatedTicket>>();

        foreach (var ticket in tickets)
        {
            var words = ticket.Title.ToLower()
                .Split(new[] { ' ', '-', '_', '.', ':', '(', ')', '[', ']' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(w => w.Length > 3)
                .Distinct();

            foreach (var word in words)
            {
                if (!patterns.ContainsKey(word))
                    patterns[word] = new List<RelatedTicket>();

                patterns[word].Add(new RelatedTicket
                {
                    TicketId = ticket.Id,
                    TicketKey = ticket.TicketKey,
                    Title = ticket.Title,
                    Status = ticket.Status,
                    CreatedAt = ticket.CreatedAt
                });
            }
        }

        return patterns
            .Where(p => p.Value.Count >= request.MinOccurrences)
            .OrderByDescending(p => p.Value.Count)
            .Take(20)
            .Select(p => new RecurringIssueDto
            {
                Pattern = p.Key,
                OccurrenceCount = p.Value.Count,
                RelatedTickets = p.Value.OrderByDescending(t => t.CreatedAt).ToList()
            })
            .ToList();
    }

    public async Task<DebtPatternSummaryDto> GetDebtPatternSummaryAsync(
        DebtPatternRequest request,
        CancellationToken cancellationToken = default)
    {
        var accessibleProjects = await _projectMemberService.GetAccessibleProjectIdsOrNullAsync(_userContext, cancellationToken);

        var query = _context.TechnicalDebts
            .Include(d => d.OriginatingTicket)
            .ThenInclude(t => t.Project)
            .AsQueryable();

        // Apply project access filtering
        if (accessibleProjects != null)
        {
            if (request.ProjectId.HasValue)
            {
                if (!accessibleProjects.Contains(request.ProjectId.Value))
                    return new DebtPatternSummaryDto { ByCategory = new(), BySeverity = new(), TopHotspots = new() };
                query = query.Where(d => d.OriginatingTicket.ProjectId == request.ProjectId.Value);
            }
            else
            {
                query = query.Where(d => accessibleProjects.Contains(d.OriginatingTicket.ProjectId));
            }
        }
        else if (request.ProjectId.HasValue)
        {
            query = query.Where(d => d.OriginatingTicket.ProjectId == request.ProjectId.Value);
        }

        if (!request.IncludeResolved)
        {
            query = query.Where(d => d.Status != DebtStatus.Resolved);
        }

        var debts = await query.ToListAsync(cancellationToken);

        var byCategory = debts
            .GroupBy(d => d.Category)
            .Select(g => new DebtByCategoryDto
            {
                Category = g.Key.ToString(),
                Count = g.Count(),
                OpenCount = g.Count(d => d.Status == DebtStatus.Open)
            })
            .OrderByDescending(x => x.Count)
            .ToList();

        var bySeverity = debts
            .GroupBy(d => d.Severity)
            .Select(g => new DebtBySeverityDto
            {
                Severity = g.Key.ToString(),
                Count = g.Count(),
                OpenCount = g.Count(d => d.Status == DebtStatus.Open)
            })
            .OrderByDescending(x => x.Count)
            .ToList();

        // Hotspots based on affected files
        var fileDebts = debts
            .Where(d => !string.IsNullOrEmpty(d.AffectedFiles))
            .SelectMany(d => d.AffectedFiles!.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(f => new { FilePath = f.Trim(), d.Category }))
            .GroupBy(x => x.FilePath)
            .Select(g => new DebtHotspotDto
            {
                FilePath = g.Key,
                DebtItemCount = g.Count(),
                Categories = g.Select(x => x.Category.ToString()).Distinct().ToList()
            })
            .OrderByDescending(x => x.DebtItemCount)
            .Take(10)
            .ToList();

        return new DebtPatternSummaryDto
        {
            TotalDebtItems = debts.Count,
            OpenDebtItems = debts.Count(d => d.Status == DebtStatus.Open),
            ResolvedDebtItems = debts.Count(d => d.Status == DebtStatus.Resolved),
            ByCategory = byCategory,
            BySeverity = bySeverity,
            TopHotspots = fileDebts
        };
    }

    #endregion

    #region Session Analytics

    public async Task<SessionMetricsDto> LogSessionMetricsAsync(
        LogSessionMetricsRequest request,
        CancellationToken cancellationToken = default)
    {
        var ticket = await _ticketRepository.GetByIdAsync(request.TicketId, cancellationToken)
            ?? throw new InvalidOperationException($"Ticket with ID '{request.TicketId}' not found");

        var metrics = new SessionMetrics
        {
            Id = Guid.NewGuid(),
            TicketId = request.TicketId,
            SessionId = request.SessionId,
            SessionStartedAt = request.SessionStartedAt ?? DateTime.UtcNow,
            SessionEndedAt = request.SessionEndedAt,
            DurationMinutes = request.DurationMinutes,
            InputTokens = request.InputTokens,
            OutputTokens = request.OutputTokens,
            TotalTokens = request.TotalTokens,
            DecisionsLogged = request.DecisionsLogged ?? 0,
            ProgressEntriesLogged = request.ProgressEntriesLogged ?? 0,
            FilesModified = request.FilesModified ?? 0,
            HandoffCreated = request.HandoffCreated ?? false,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow
        };

        await _sessionMetricsRepository.AddAsync(metrics, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToSessionMetricsDto(metrics, ticket);
    }

    public async Task<SessionMetricsDto?> UpdateSessionMetricsAsync(
        UpdateSessionMetricsRequest request,
        CancellationToken cancellationToken = default)
    {
        SessionMetrics? metrics = null;

        if (request.Id.HasValue)
        {
            metrics = await _sessionMetricsRepository.GetByIdAsync(request.Id.Value, cancellationToken);
        }
        else if (!string.IsNullOrEmpty(request.SessionId))
        {
            metrics = await _sessionMetricsRepository.GetBySessionIdAsync(request.SessionId, cancellationToken);
        }

        if (metrics == null)
            return null;

        if (request.SessionEndedAt.HasValue) metrics.SessionEndedAt = request.SessionEndedAt;
        if (request.DurationMinutes.HasValue) metrics.DurationMinutes = request.DurationMinutes;
        if (request.InputTokens.HasValue) metrics.InputTokens = request.InputTokens;
        if (request.OutputTokens.HasValue) metrics.OutputTokens = request.OutputTokens;
        if (request.TotalTokens.HasValue) metrics.TotalTokens = request.TotalTokens;
        if (request.DecisionsLogged.HasValue) metrics.DecisionsLogged = request.DecisionsLogged.Value;
        if (request.ProgressEntriesLogged.HasValue) metrics.ProgressEntriesLogged = request.ProgressEntriesLogged.Value;
        if (request.FilesModified.HasValue) metrics.FilesModified = request.FilesModified.Value;
        if (request.HandoffCreated.HasValue) metrics.HandoffCreated = request.HandoffCreated.Value;
        if (request.Notes != null) metrics.Notes = request.Notes;

        await _sessionMetricsRepository.UpdateAsync(metrics, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var ticket = await _ticketRepository.GetByIdAsync(metrics.TicketId, cancellationToken);
        return MapToSessionMetricsDto(metrics, ticket!);
    }

    public async Task<TicketSessionAnalyticsDto> GetTicketSessionAnalyticsAsync(
        Guid ticketId,
        CancellationToken cancellationToken = default)
    {
        var ticket = await _context.Tickets
            .Include(t => t.Project)
            .FirstOrDefaultAsync(t => t.Id == ticketId, cancellationToken)
            ?? throw new InvalidOperationException($"Ticket with ID '{ticketId}' not found");

        var sessions = await _context.SessionMetrics
            .Where(s => s.TicketId == ticketId)
            .OrderByDescending(s => s.SessionStartedAt)
            .ToListAsync(cancellationToken);

        return new TicketSessionAnalyticsDto
        {
            TicketId = ticketId,
            TicketKey = ticket.Key,
            TicketTitle = ticket.Title,
            TotalSessions = sessions.Count,
            TotalDurationMinutes = sessions.Sum(s => s.DurationMinutes ?? 0),
            AverageDurationMinutes = sessions.Any(s => s.DurationMinutes.HasValue)
                ? (int?)sessions.Where(s => s.DurationMinutes.HasValue).Average(s => s.DurationMinutes!.Value)
                : null,
            TotalInputTokens = sessions.Sum(s => s.InputTokens ?? 0),
            TotalOutputTokens = sessions.Sum(s => s.OutputTokens ?? 0),
            TotalTokens = sessions.Sum(s => s.TotalTokens ?? 0),
            TotalDecisions = sessions.Sum(s => s.DecisionsLogged),
            TotalProgressEntries = sessions.Sum(s => s.ProgressEntriesLogged),
            TotalFilesModified = sessions.Sum(s => s.FilesModified),
            HandoffsCreated = sessions.Count(s => s.HandoffCreated),
            Sessions = sessions.Select(s => MapToSessionMetricsDto(s, ticket)).ToList()
        };
    }

    public async Task<ProjectSessionAnalyticsDto> GetProjectSessionAnalyticsAsync(
        ProjectSessionAnalyticsRequest request,
        CancellationToken cancellationToken = default)
    {
        var project = await _context.Projects
            .FirstOrDefaultAsync(p => p.Id == request.ProjectId, cancellationToken)
            ?? throw new InvalidOperationException($"Project with ID '{request.ProjectId}' not found");

        var query = _context.SessionMetrics
            .Include(s => s.Ticket)
            .Where(s => s.Ticket.ProjectId == request.ProjectId)
            .AsQueryable();

        if (request.StartDate.HasValue)
            query = query.Where(s => s.SessionStartedAt >= request.StartDate.Value);

        if (request.EndDate.HasValue)
            query = query.Where(s => s.SessionStartedAt <= request.EndDate.Value);

        var sessions = await query.ToListAsync(cancellationToken);

        return new ProjectSessionAnalyticsDto
        {
            ProjectId = project.Id,
            ProjectKey = project.Key,
            ProjectName = project.Name,
            TotalSessions = sessions.Count,
            TotalDurationMinutes = sessions.Sum(s => s.DurationMinutes ?? 0),
            AverageDurationMinutes = sessions.Any(s => s.DurationMinutes.HasValue)
                ? (int?)sessions.Where(s => s.DurationMinutes.HasValue).Average(s => s.DurationMinutes!.Value)
                : null,
            TotalInputTokens = sessions.Sum(s => s.InputTokens ?? 0),
            TotalOutputTokens = sessions.Sum(s => s.OutputTokens ?? 0),
            TotalTokens = sessions.Sum(s => s.TotalTokens ?? 0),
            TotalDecisions = sessions.Sum(s => s.DecisionsLogged),
            TotalProgressEntries = sessions.Sum(s => s.ProgressEntriesLogged),
            TotalFilesModified = sessions.Sum(s => s.FilesModified),
            HandoffsCreated = sessions.Count(s => s.HandoffCreated),
            TicketsWorkedOn = sessions.Select(s => s.TicketId).Distinct().Count(),
            StartDate = request.StartDate,
            EndDate = request.EndDate
        };
    }

    public async Task<ProductivityMetricsDto> GetProductivityMetricsAsync(
        ProductivityMetricsRequest request,
        CancellationToken cancellationToken = default)
    {
        var accessibleProjects = await _projectMemberService.GetAccessibleProjectIdsOrNullAsync(_userContext, cancellationToken);

        var ticketQuery = _context.Tickets
            .Include(t => t.Project)
            .Where(t => t.Status == TicketStatus.Done)
            .AsQueryable();

        var sessionQuery = _context.SessionMetrics
            .Include(s => s.Ticket)
            .AsQueryable();

        // Apply project access filtering
        if (accessibleProjects != null)
        {
            if (request.ProjectId.HasValue)
            {
                if (!accessibleProjects.Contains(request.ProjectId.Value))
                    return new ProductivityMetricsDto { ByTicketType = new(), DailyTrend = new() };
                ticketQuery = ticketQuery.Where(t => t.ProjectId == request.ProjectId.Value);
                sessionQuery = sessionQuery.Where(s => s.Ticket.ProjectId == request.ProjectId.Value);
            }
            else
            {
                ticketQuery = ticketQuery.Where(t => accessibleProjects.Contains(t.ProjectId));
                sessionQuery = sessionQuery.Where(s => accessibleProjects.Contains(s.Ticket.ProjectId));
            }
        }
        else if (request.ProjectId.HasValue)
        {
            ticketQuery = ticketQuery.Where(t => t.ProjectId == request.ProjectId.Value);
            sessionQuery = sessionQuery.Where(s => s.Ticket.ProjectId == request.ProjectId.Value);
        }

        if (request.StartDate.HasValue)
        {
            ticketQuery = ticketQuery.Where(t => t.UpdatedAt >= request.StartDate.Value);
            sessionQuery = sessionQuery.Where(s => s.SessionStartedAt >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            ticketQuery = ticketQuery.Where(t => t.UpdatedAt <= request.EndDate.Value);
            sessionQuery = sessionQuery.Where(s => s.SessionStartedAt <= request.EndDate.Value);
        }

        var completedTickets = await ticketQuery.ToListAsync(cancellationToken);
        var sessions = await sessionQuery.ToListAsync(cancellationToken);

        var byType = completedTickets
            .GroupBy(t => t.Type)
            .Select(g =>
            {
                var ticketIds = g.Select(t => t.Id).ToHashSet();
                var typeSessions = sessions.Where(s => ticketIds.Contains(s.TicketId)).ToList();
                return new ProductivityByType
                {
                    TicketType = g.Key.ToString(),
                    TicketCount = g.Count(),
                    AverageMinutes = typeSessions.Any() ? (int)typeSessions.Average(s => s.DurationMinutes ?? 0) : 0,
                    AverageTokens = typeSessions.Any() ? (int)typeSessions.Average(s => s.TotalTokens ?? 0) : 0
                };
            })
            .OrderByDescending(x => x.TicketCount)
            .ToList();

        var dailyTrend = sessions
            .GroupBy(s => s.SessionStartedAt.Date)
            .OrderBy(g => g.Key)
            .Select(g => new ProductivityByDay
            {
                Date = g.Key,
                SessionCount = g.Count(),
                TicketsWorkedOn = g.Select(s => s.TicketId).Distinct().Count(),
                TotalMinutes = g.Sum(s => s.DurationMinutes ?? 0),
                TotalTokens = g.Sum(s => s.TotalTokens ?? 0)
            })
            .ToList();

        var totalDecisions = sessions.Sum(s => s.DecisionsLogged);

        return new ProductivityMetricsDto
        {
            TotalTicketsCompleted = completedTickets.Count,
            TotalSessions = sessions.Count,
            TotalDurationMinutes = sessions.Sum(s => s.DurationMinutes ?? 0),
            TotalTokensUsed = sessions.Sum(s => s.TotalTokens ?? 0),
            AverageSessionsPerTicket = completedTickets.Any()
                ? (double)sessions.Count / completedTickets.Count
                : 0,
            AverageMinutesPerTicket = completedTickets.Any()
                ? (double)sessions.Sum(s => s.DurationMinutes ?? 0) / completedTickets.Count
                : 0,
            AverageTokensPerTicket = completedTickets.Any()
                ? (double)sessions.Sum(s => s.TotalTokens ?? 0) / completedTickets.Count
                : 0,
            AverageDecisionsPerTicket = completedTickets.Any()
                ? (double)totalDecisions / completedTickets.Count
                : 0,
            ByTicketType = byType,
            DailyTrend = dailyTrend,
            StartDate = request.StartDate,
            EndDate = request.EndDate
        };
    }

    #endregion

    #region Dashboard

    public async Task<AnalyticsDashboardDto> GetDashboardAsync(
        AnalyticsDashboardRequest request,
        CancellationToken cancellationToken = default)
    {
        var accessibleProjects = await _projectMemberService.GetAccessibleProjectIdsOrNullAsync(_userContext, cancellationToken);

        var ticketQuery = _context.Tickets.AsQueryable();
        var sessionQuery = _context.SessionMetrics.Include(s => s.Ticket).AsQueryable();
        var decisionQuery = _context.ReasoningLogs.Include(r => r.Ticket).AsQueryable();
        var debtQuery = _context.TechnicalDebts.Include(d => d.OriginatingTicket).AsQueryable();

        // Apply project access filtering
        if (accessibleProjects != null)
        {
            if (request.ProjectId.HasValue)
            {
                if (!accessibleProjects.Contains(request.ProjectId.Value))
                    return new AnalyticsDashboardDto { RecentLowConfidenceDecisions = new(), TopHotFiles = new(), RecentActivity = new(), GeneratedAt = DateTime.UtcNow };
                ticketQuery = ticketQuery.Where(t => t.ProjectId == request.ProjectId.Value);
                sessionQuery = sessionQuery.Where(s => s.Ticket.ProjectId == request.ProjectId.Value);
                decisionQuery = decisionQuery.Where(r => r.Ticket.ProjectId == request.ProjectId.Value);
                debtQuery = debtQuery.Where(d => d.OriginatingTicket.ProjectId == request.ProjectId.Value);
            }
            else
            {
                ticketQuery = ticketQuery.Where(t => accessibleProjects.Contains(t.ProjectId));
                sessionQuery = sessionQuery.Where(s => accessibleProjects.Contains(s.Ticket.ProjectId));
                decisionQuery = decisionQuery.Where(r => accessibleProjects.Contains(r.Ticket.ProjectId));
                debtQuery = debtQuery.Where(d => accessibleProjects.Contains(d.OriginatingTicket.ProjectId));
            }
        }
        else if (request.ProjectId.HasValue)
        {
            ticketQuery = ticketQuery.Where(t => t.ProjectId == request.ProjectId.Value);
            sessionQuery = sessionQuery.Where(s => s.Ticket.ProjectId == request.ProjectId.Value);
            decisionQuery = decisionQuery.Where(r => r.Ticket.ProjectId == request.ProjectId.Value);
            debtQuery = debtQuery.Where(d => d.OriginatingTicket.ProjectId == request.ProjectId.Value);
        }

        if (request.StartDate.HasValue)
        {
            ticketQuery = ticketQuery.Where(t => t.CreatedAt >= request.StartDate.Value);
            sessionQuery = sessionQuery.Where(s => s.SessionStartedAt >= request.StartDate.Value);
            decisionQuery = decisionQuery.Where(r => r.CreatedAt >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            ticketQuery = ticketQuery.Where(t => t.CreatedAt <= request.EndDate.Value);
            sessionQuery = sessionQuery.Where(s => s.SessionStartedAt <= request.EndDate.Value);
            decisionQuery = decisionQuery.Where(r => r.CreatedAt <= request.EndDate.Value);
        }

        var tickets = await ticketQuery.ToListAsync(cancellationToken);
        var sessions = await sessionQuery.ToListAsync(cancellationToken);
        var decisions = await decisionQuery.Where(r => r.ConfidencePercent.HasValue).ToListAsync(cancellationToken);
        var openDebtCount = await debtQuery.CountAsync(d => d.Status == DebtStatus.Open, cancellationToken);

        // Get hot files
        var hotFilesRequest = new HotFileRequest { ProjectId = request.ProjectId, TopN = request.TopHotFilesLimit };
        var hotFiles = await GetHotFilesAsync(hotFilesRequest, cancellationToken);

        // Get low confidence decisions
        var lowConfidenceRequest = new LowConfidenceDecisionRequest
        {
            ProjectId = request.ProjectId,
            Limit = request.LowConfidenceLimit,
            Since = request.StartDate
        };
        var lowConfidenceDecisions = await GetLowConfidenceDecisionsAsync(lowConfidenceRequest, cancellationToken);

        // Build recent activity
        var recentActivity = new List<RecentActivityDto>();

        var recentSessions = sessions.OrderByDescending(s => s.SessionStartedAt).Take(5)
            .Select(s => new RecentActivityDto
            {
                ActivityType = "Session",
                Description = $"Session started for ticket",
                TicketKey = s.Ticket?.Key,
                Timestamp = s.SessionStartedAt
            });

        var recentDecisions = decisions.OrderByDescending(d => d.CreatedAt).Take(5)
            .Select(d => new RecentActivityDto
            {
                ActivityType = "Decision",
                Description = d.DecisionPoint.Length > 50 ? d.DecisionPoint[..50] + "..." : d.DecisionPoint,
                TicketKey = d.Ticket?.Key,
                Timestamp = d.CreatedAt
            });

        recentActivity.AddRange(recentSessions);
        recentActivity.AddRange(recentDecisions);
        recentActivity = recentActivity.OrderByDescending(a => a.Timestamp).Take(request.RecentActivityLimit).ToList();

        return new AnalyticsDashboardDto
        {
            TotalTickets = tickets.Count,
            TicketsInProgress = tickets.Count(t => t.Status == TicketStatus.InProgress),
            TicketsCompleted = tickets.Count(t => t.Status == TicketStatus.Done),
            TotalSessions = sessions.Count,
            TotalDecisions = decisions.Count,
            OverallAverageConfidence = decisions.Any() ? decisions.Average(d => d.ConfidencePercent!.Value) : 0,
            LowConfidenceDecisionCount = decisions.Count(d => d.ConfidencePercent < 50),
            RecentLowConfidenceDecisions = lowConfidenceDecisions,
            TopHotFiles = hotFiles,
            OpenTechnicalDebtCount = openDebtCount,
            TotalTokensUsed = sessions.Sum(s => s.TotalTokens ?? 0),
            TotalMinutesWorked = sessions.Sum(s => s.DurationMinutes ?? 0),
            HandoffsCreated = sessions.Count(s => s.HandoffCreated),
            RecentActivity = recentActivity,
            GeneratedAt = DateTime.UtcNow
        };
    }

    #endregion

    #region Helpers

    private static SessionMetricsDto MapToSessionMetricsDto(SessionMetrics metrics, Ticket ticket)
    {
        return new SessionMetricsDto
        {
            Id = metrics.Id,
            TicketId = metrics.TicketId,
            TicketKey = ticket.Key,
            SessionId = metrics.SessionId,
            SessionStartedAt = metrics.SessionStartedAt,
            SessionEndedAt = metrics.SessionEndedAt,
            DurationMinutes = metrics.DurationMinutes,
            InputTokens = metrics.InputTokens,
            OutputTokens = metrics.OutputTokens,
            TotalTokens = metrics.TotalTokens,
            DecisionsLogged = metrics.DecisionsLogged,
            ProgressEntriesLogged = metrics.ProgressEntriesLogged,
            FilesModified = metrics.FilesModified,
            HandoffCreated = metrics.HandoffCreated,
            Notes = metrics.Notes,
            CreatedAt = metrics.CreatedAt
        };
    }

    #endregion
}
