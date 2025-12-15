using AiForge.Application.DTOs.Analytics.Confidence;
using AiForge.Application.DTOs.Analytics.Dashboard;
using AiForge.Application.DTOs.Analytics.Patterns;
using AiForge.Application.DTOs.Analytics.Sessions;
using AiForge.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace AiForge.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;

    public AnalyticsController(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    #region Dashboard

    /// <summary>
    /// Get aggregated analytics dashboard
    /// </summary>
    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(AnalyticsDashboardDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<AnalyticsDashboardDto>> GetDashboard(
        [FromQuery] Guid? projectId,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] int recentActivityLimit = 10,
        [FromQuery] int topHotFilesLimit = 5,
        [FromQuery] int lowConfidenceLimit = 5,
        CancellationToken cancellationToken = default)
    {
        var request = new AnalyticsDashboardRequest
        {
            ProjectId = projectId,
            StartDate = startDate,
            EndDate = endDate,
            RecentActivityLimit = recentActivityLimit,
            TopHotFilesLimit = topHotFilesLimit,
            LowConfidenceLimit = lowConfidenceLimit
        };

        var dashboard = await _analyticsService.GetDashboardAsync(request, cancellationToken);
        return Ok(dashboard);
    }

    #endregion

    #region Confidence Tracking

    /// <summary>
    /// Get low confidence decisions that may need review
    /// </summary>
    [HttpGet("confidence/low")]
    [ProducesResponseType(typeof(List<LowConfidenceDecisionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<LowConfidenceDecisionDto>>> GetLowConfidenceDecisions(
        [FromQuery] int confidenceThreshold = 50,
        [FromQuery] Guid? projectId = null,
        [FromQuery] DateTime? since = null,
        [FromQuery] int? limit = 50,
        CancellationToken cancellationToken = default)
    {
        var request = new LowConfidenceDecisionRequest
        {
            ConfidenceThreshold = confidenceThreshold,
            ProjectId = projectId,
            Since = since,
            Limit = limit
        };

        var decisions = await _analyticsService.GetLowConfidenceDecisionsAsync(request, cancellationToken);
        return Ok(decisions);
    }

    /// <summary>
    /// Get confidence trends over time
    /// </summary>
    [HttpGet("confidence/trends")]
    [ProducesResponseType(typeof(ConfidenceTrendDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ConfidenceTrendDto>> GetConfidenceTrends(
        [FromQuery] Guid? projectId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string granularity = "day",
        CancellationToken cancellationToken = default)
    {
        var request = new ConfidenceTrendRequest
        {
            ProjectId = projectId,
            StartDate = startDate,
            EndDate = endDate,
            Granularity = granularity
        };

        var trends = await _analyticsService.GetConfidenceTrendsAsync(request, cancellationToken);
        return Ok(trends);
    }

    /// <summary>
    /// Get tickets with consistently low confidence
    /// </summary>
    [HttpGet("confidence/tickets")]
    [ProducesResponseType(typeof(List<TicketConfidenceSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<TicketConfidenceSummaryDto>>> GetLowConfidenceTickets(
        [FromQuery] Guid? projectId = null,
        [FromQuery] int confidenceThreshold = 50,
        [FromQuery] int? limit = 20,
        CancellationToken cancellationToken = default)
    {
        var request = new TicketConfidenceSummaryRequest
        {
            ProjectId = projectId,
            ConfidenceThreshold = confidenceThreshold,
            Limit = limit
        };

        var tickets = await _analyticsService.GetLowConfidenceTicketsAsync(request, cancellationToken);
        return Ok(tickets);
    }

    #endregion

    #region Pattern Detection

    /// <summary>
    /// Get most frequently modified files (hot files)
    /// </summary>
    [HttpGet("patterns/hot-files")]
    [ProducesResponseType(typeof(List<AnalyticsHotFileDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AnalyticsHotFileDto>>> GetHotFiles(
        [FromQuery] Guid? projectId = null,
        [FromQuery] DateTime? since = null,
        [FromQuery] int topN = 20,
        CancellationToken cancellationToken = default)
    {
        var request = new HotFileRequest
        {
            ProjectId = projectId,
            Since = since,
            TopN = topN
        };

        var hotFiles = await _analyticsService.GetHotFilesAsync(request, cancellationToken);
        return Ok(hotFiles);
    }

    /// <summary>
    /// Get files that frequently change together with a given file
    /// </summary>
    [HttpGet("patterns/correlations")]
    [ProducesResponseType(typeof(FileCorrelationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<FileCorrelationDto>> GetCorrelatedFiles(
        [FromQuery] string filePath,
        [FromQuery] int minCooccurrence = 2,
        [FromQuery] int topN = 10,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return BadRequest(new { error = "filePath is required" });

        var request = new FileCorrelationRequest
        {
            FilePath = filePath,
            MinCooccurrence = minCooccurrence,
            TopN = topN
        };

        var correlations = await _analyticsService.GetCorrelatedFilesAsync(request, cancellationToken);
        return Ok(correlations);
    }

    /// <summary>
    /// Get recurring issue patterns
    /// </summary>
    [HttpGet("patterns/recurring")]
    [ProducesResponseType(typeof(List<RecurringIssueDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<RecurringIssueDto>>> GetRecurringIssues(
        [FromQuery] Guid? projectId = null,
        [FromQuery] string? ticketType = null,
        [FromQuery] int minOccurrences = 2,
        CancellationToken cancellationToken = default)
    {
        var request = new RecurringIssueRequest
        {
            ProjectId = projectId,
            TicketType = ticketType,
            MinOccurrences = minOccurrences
        };

        var issues = await _analyticsService.GetRecurringIssuesAsync(request, cancellationToken);
        return Ok(issues);
    }

    /// <summary>
    /// Get technical debt pattern summary
    /// </summary>
    [HttpGet("patterns/debt")]
    [ProducesResponseType(typeof(DebtPatternSummaryDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<DebtPatternSummaryDto>> GetDebtPatternSummary(
        [FromQuery] Guid? projectId = null,
        [FromQuery] bool includeResolved = false,
        CancellationToken cancellationToken = default)
    {
        var request = new DebtPatternRequest
        {
            ProjectId = projectId,
            IncludeResolved = includeResolved
        };

        var summary = await _analyticsService.GetDebtPatternSummaryAsync(request, cancellationToken);
        return Ok(summary);
    }

    #endregion

    #region Session Analytics

    /// <summary>
    /// Log session metrics
    /// </summary>
    [HttpPost("sessions")]
    [ProducesResponseType(typeof(SessionMetricsDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SessionMetricsDto>> LogSessionMetrics(
        [FromBody] LogSessionMetricsRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.TicketId == Guid.Empty)
            return BadRequest(new { error = "TicketId is required" });

        if (string.IsNullOrWhiteSpace(request.SessionId))
            return BadRequest(new { error = "SessionId is required" });

        try
        {
            var metrics = await _analyticsService.LogSessionMetricsAsync(request, cancellationToken);
            return StatusCode(StatusCodes.Status201Created, metrics);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Update existing session metrics
    /// </summary>
    [HttpPut("sessions")]
    [ProducesResponseType(typeof(SessionMetricsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SessionMetricsDto>> UpdateSessionMetrics(
        [FromBody] UpdateSessionMetricsRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!request.Id.HasValue && string.IsNullOrWhiteSpace(request.SessionId))
            return BadRequest(new { error = "Either Id or SessionId is required" });

        var metrics = await _analyticsService.UpdateSessionMetricsAsync(request, cancellationToken);
        if (metrics == null)
            return NotFound(new { error = "Session metrics not found" });

        return Ok(metrics);
    }

    /// <summary>
    /// Get session analytics for a specific ticket
    /// </summary>
    [HttpGet("sessions/ticket/{ticketId:guid}")]
    [ProducesResponseType(typeof(TicketSessionAnalyticsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TicketSessionAnalyticsDto>> GetTicketSessionAnalytics(
        Guid ticketId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var analytics = await _analyticsService.GetTicketSessionAnalyticsAsync(ticketId, cancellationToken);
            return Ok(analytics);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get session analytics for a project
    /// </summary>
    [HttpGet("sessions/project/{projectId:guid}")]
    [ProducesResponseType(typeof(ProjectSessionAnalyticsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProjectSessionAnalyticsDto>> GetProjectSessionAnalytics(
        Guid projectId,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new ProjectSessionAnalyticsRequest
            {
                ProjectId = projectId,
                StartDate = startDate,
                EndDate = endDate
            };

            var analytics = await _analyticsService.GetProjectSessionAnalyticsAsync(request, cancellationToken);
            return Ok(analytics);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get productivity metrics
    /// </summary>
    [HttpGet("productivity")]
    [ProducesResponseType(typeof(ProductivityMetricsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ProductivityMetricsDto>> GetProductivityMetrics(
        [FromQuery] Guid? projectId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var request = new ProductivityMetricsRequest
        {
            ProjectId = projectId,
            StartDate = startDate,
            EndDate = endDate
        };

        var metrics = await _analyticsService.GetProductivityMetricsAsync(request, cancellationToken);
        return Ok(metrics);
    }

    #endregion
}
