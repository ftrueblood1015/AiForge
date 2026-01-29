using System.ComponentModel;
using System.Text.Json;
using AiForge.Application.DTOs.FileChange;
using AiForge.Application.DTOs.TestLink;
using AiForge.Application.DTOs.TechnicalDebt;
using AiForge.Application.Services;
using ModelContextProtocol.Server;

namespace AiForge.Mcp.Tools;

[McpServerToolType]
public class CodeIntelligenceTools
{
    private readonly IFileChangeService _fileChangeService;
    private readonly ITestLinkService _testLinkService;
    private readonly ITechnicalDebtService _debtService;
    private readonly ITicketService _ticketService;
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public CodeIntelligenceTools(
        IFileChangeService fileChangeService,
        ITestLinkService testLinkService,
        ITechnicalDebtService debtService,
        ITicketService ticketService)
    {
        _fileChangeService = fileChangeService;
        _testLinkService = testLinkService;
        _debtService = debtService;
        _ticketService = ticketService;
    }

    #region File Change Tools

    [McpServerTool(Name = "log_file_change"), Description("Log a file change for a ticket")]
    public async Task<string> LogFileChange(
        [Description("Ticket key (e.g., DEMO-1) or ID")] string ticketKeyOrId,
        [Description("File path that was changed")] string filePath,
        [Description("Change type: Created, Modified, Deleted, Renamed")] string changeType,
        [Description("Reason for the change")] string? changeReason = null,
        [Description("Old file path (for renames)")] string? oldFilePath = null,
        [Description("Number of lines added")] int? linesAdded = null,
        [Description("Number of lines removed")] int? linesRemoved = null,
        [Description("Claude session identifier")] string? sessionId = null,
        [Description("Return full entity response instead of minimal confirmation")] bool returnFull = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var (ticketId, ticketKey) = await ResolveTicketAsync(ticketKeyOrId, cancellationToken);
            if (ticketId == null)
                return JsonSerializer.Serialize(new { error = $"Ticket '{ticketKeyOrId}' not found" });

            var request = new LogFileChangeRequest
            {
                FilePath = filePath,
                ChangeType = changeType,
                ChangeReason = changeReason,
                OldFilePath = oldFilePath,
                LinesAdded = linesAdded,
                LinesRemoved = linesRemoved,
                SessionId = sessionId
            };

            var fileChange = await _fileChangeService.LogFileChangeAsync(ticketId.Value, request, cancellationToken);

            if (returnFull)
            {
                return JsonSerializer.Serialize(new
                {
                    success = true,
                    entity = fileChange,
                    message = $"File change logged: {changeType} {filePath}"
                }, JsonOptions);
            }

            return JsonSerializer.Serialize(new
            {
                success = true,
                Id = fileChange.Id,
                TicketKey = ticketKey,
                message = $"File change logged: {changeType} {filePath}"
            }, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool(Name = "query_file_history"), Description("Get the history of changes for a specific file across all tickets")]
    public async Task<string> QueryFileHistory(
        [Description("File path to query history for")] string filePath,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var history = await _fileChangeService.GetFileHistoryAsync(filePath, cancellationToken);

            return JsonSerializer.Serialize(new
            {
                found = history.Changes.Count > 0,
                history.FilePath,
                history.TotalTickets,
                changes = history.Changes.Select(c => new
                {
                    c.TicketKey,
                    c.ChangeType,
                    c.ChangeReason,
                    c.LinesAdded,
                    c.LinesRemoved,
                    c.CreatedAt
                })
            }, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool(Name = "get_hot_files"), Description("Get the most frequently modified files")]
    public async Task<string> GetHotFiles(
        [Description("Maximum number of files to return")] int limit = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var hotFiles = await _fileChangeService.GetHotFilesAsync(limit, cancellationToken);
            var fileList = hotFiles.ToList();

            return JsonSerializer.Serialize(new
            {
                count = fileList.Count,
                files = fileList.Select(f => new
                {
                    f.FilePath,
                    f.ChangeCount,
                    f.LastModified
                })
            }, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    #endregion

    #region Test Link Tools

    [McpServerTool(Name = "link_test"), Description("Link a test to a ticket")]
    public async Task<string> LinkTest(
        [Description("Ticket key (e.g., DEMO-1) or ID")] string ticketKeyOrId,
        [Description("Test file path")] string testFilePath,
        [Description("Specific test case name")] string? testName = null,
        [Description("What functionality is being tested")] string? testedFunctionality = null,
        [Description("Code file being tested")] string? linkedFilePath = null,
        [Description("Test outcome: Passed, Failed, Skipped, NotRun")] string? outcome = null,
        [Description("Claude session identifier")] string? sessionId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var ticketId = await ResolveTicketIdAsync(ticketKeyOrId, cancellationToken);
            if (ticketId == null)
                return JsonSerializer.Serialize(new { error = $"Ticket '{ticketKeyOrId}' not found" });

            var request = new LinkTestRequest
            {
                TestFilePath = testFilePath,
                TestName = testName,
                TestedFunctionality = testedFunctionality,
                LinkedFilePath = linkedFilePath,
                Outcome = outcome,
                SessionId = sessionId
            };

            var testLink = await _testLinkService.LinkTestAsync(ticketId.Value, request, cancellationToken);

            return JsonSerializer.Serialize(new
            {
                success = true,
                testLink.Id,
                testLink.TicketId,
                testLink.TestFilePath,
                testLink.TestName,
                testLink.Outcome,
                message = $"Test linked: {testFilePath}"
            }, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool(Name = "update_test_outcome"), Description("Update the outcome of a test")]
    public async Task<string> UpdateTestOutcome(
        [Description("Test link ID (GUID)")] string testLinkId,
        [Description("Test outcome: Passed, Failed, Skipped, NotRun")] string outcome,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!Guid.TryParse(testLinkId, out var id))
                return JsonSerializer.Serialize(new { error = "Invalid test link ID format" });

            var request = new UpdateTestOutcomeRequest { Outcome = outcome };
            var testLink = await _testLinkService.UpdateTestOutcomeAsync(id, request, cancellationToken);

            if (testLink == null)
                return JsonSerializer.Serialize(new { error = $"Test link '{testLinkId}' not found" });

            return JsonSerializer.Serialize(new
            {
                success = true,
                testLink.Id,
                testLink.TestFilePath,
                testLink.TestName,
                testLink.Outcome,
                testLink.LastRunAt,
                message = $"Test outcome updated to {outcome}"
            }, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool(Name = "get_file_coverage"), Description("Get test coverage information for a file")]
    public async Task<string> GetFileCoverage(
        [Description("File path to check coverage for")] string filePath,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var coverage = await _testLinkService.GetFileCoverageAsync(filePath, cancellationToken);

            return JsonSerializer.Serialize(new
            {
                coverage.FilePath,
                coverage.TotalTests,
                hasCoverage = coverage.TotalTests > 0,
                tests = coverage.Tests.Select(t => new
                {
                    t.TicketKey,
                    t.TestFilePath,
                    t.TestName,
                    t.Outcome,
                    t.LastRunAt
                })
            }, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    #endregion

    #region Technical Debt Tools

    [McpServerTool(Name = "flag_technical_debt"), Description("Flag technical debt for a ticket")]
    public async Task<string> FlagTechnicalDebt(
        [Description("Ticket key (e.g., DEMO-1) or ID")] string ticketKeyOrId,
        [Description("Title of the debt item")] string title,
        [Description("Description of the debt")] string? description = null,
        [Description("Category: Performance, Security, Maintainability, Testing, Documentation, Architecture")] string category = "Maintainability",
        [Description("Severity: Low, Medium, High, Critical")] string severity = "Medium",
        [Description("Why the shortcut was taken")] string? rationale = null,
        [Description("Comma-separated list of affected files")] string? affectedFiles = null,
        [Description("Claude session identifier")] string? sessionId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var ticketId = await ResolveTicketIdAsync(ticketKeyOrId, cancellationToken);
            if (ticketId == null)
                return JsonSerializer.Serialize(new { error = $"Ticket '{ticketKeyOrId}' not found" });

            var request = new CreateDebtRequest
            {
                Title = title,
                Description = description,
                Category = category,
                Severity = severity,
                Rationale = rationale,
                AffectedFiles = affectedFiles,
                SessionId = sessionId
            };

            var debt = await _debtService.FlagDebtAsync(ticketId.Value, request, cancellationToken);

            return JsonSerializer.Serialize(new
            {
                success = true,
                debt.Id,
                debt.OriginatingTicketKey,
                debt.Title,
                debt.Category,
                debt.Severity,
                debt.Status,
                message = $"Technical debt flagged: {title}"
            }, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool(Name = "get_debt_backlog"), Description("Get technical debt backlog with optional filters")]
    public async Task<string> GetDebtBacklog(
        [Description("Filter by status: Open, InProgress, Resolved, Accepted")] string? status = null,
        [Description("Filter by category: Performance, Security, Maintainability, Testing, Documentation, Architecture")] string? category = null,
        [Description("Filter by severity: Low, Medium, High, Critical")] string? severity = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var backlog = await _debtService.GetDebtBacklogAsync(status, category, severity, cancellationToken);

            return JsonSerializer.Serialize(new
            {
                backlog.TotalCount,
                items = backlog.Items.Select(d => new
                {
                    d.Id,
                    d.OriginatingTicketKey,
                    d.Title,
                    d.Category,
                    d.Severity,
                    d.Status,
                    d.CreatedAt
                })
            }, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool(Name = "resolve_debt"), Description("Resolve a technical debt item")]
    public async Task<string> ResolveDebt(
        [Description("Debt ID (GUID)")] string debtId,
        [Description("Resolution ticket key or ID (optional)")] string? resolutionTicketKeyOrId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!Guid.TryParse(debtId, out var id))
                return JsonSerializer.Serialize(new { error = "Invalid debt ID format" });

            Guid? resolutionTicketId = null;
            if (!string.IsNullOrEmpty(resolutionTicketKeyOrId))
            {
                resolutionTicketId = await ResolveTicketIdAsync(resolutionTicketKeyOrId, cancellationToken);
            }

            var request = new ResolveDebtRequest { ResolutionTicketId = resolutionTicketId };
            var debt = await _debtService.ResolveDebtAsync(id, request, cancellationToken);

            if (debt == null)
                return JsonSerializer.Serialize(new { error = $"Debt item '{debtId}' not found" });

            return JsonSerializer.Serialize(new
            {
                success = true,
                debt.Id,
                debt.Title,
                debt.Status,
                debt.ResolutionTicketKey,
                debt.ResolvedAt,
                message = "Technical debt resolved"
            }, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool(Name = "get_debt_summary"), Description("Get summary of technical debt by category and severity")]
    public async Task<string> GetDebtSummary(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var summary = await _debtService.GetDebtSummaryAsync(cancellationToken);

            return JsonSerializer.Serialize(new
            {
                summary.TotalOpen,
                summary.TotalResolved,
                summary.ByCategory,
                summary.BySeverity
            }, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    #endregion

    private async Task<Guid?> ResolveTicketIdAsync(string ticketKeyOrId, CancellationToken cancellationToken)
    {
        if (Guid.TryParse(ticketKeyOrId, out var id))
            return id;

        var ticket = await _ticketService.GetByKeyAsync(ticketKeyOrId.ToUpperInvariant(), cancellationToken);
        return ticket?.Id;
    }

    private async Task<(Guid? Id, string? Key)> ResolveTicketAsync(string ticketKeyOrId, CancellationToken cancellationToken)
    {
        if (Guid.TryParse(ticketKeyOrId, out var id))
        {
            var ticket = await _ticketService.GetByIdAsync(id, cancellationToken);
            return (ticket?.Id, ticket?.Key);
        }

        var ticketByKey = await _ticketService.GetByKeyAsync(ticketKeyOrId.ToUpperInvariant(), cancellationToken);
        return (ticketByKey?.Id, ticketByKey?.Key);
    }
}
