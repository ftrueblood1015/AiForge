using AiForge.Application.DTOs.Planning;
using AiForge.Application.Services;
using AiForge.Domain.Entities;
using AiForge.Domain.Enums;
using AiForge.Domain.Interfaces;
using AiForge.Infrastructure.Repositories;
using Moq;

namespace AiForge.Application.Tests.Services;

public class SummaryServiceTests
{
    private readonly Mock<ITicketRepository> _ticketRepositoryMock;
    private readonly Mock<IProgressEntryRepository> _progressRepositoryMock;
    private readonly Mock<IReasoningLogRepository> _reasoningRepositoryMock;
    private readonly Mock<IPlanningSessionRepository> _sessionRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly SummaryService _service;

    public SummaryServiceTests()
    {
        _ticketRepositoryMock = new Mock<ITicketRepository>();
        _progressRepositoryMock = new Mock<IProgressEntryRepository>();
        _reasoningRepositoryMock = new Mock<IReasoningLogRepository>();
        _sessionRepositoryMock = new Mock<IPlanningSessionRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _service = new SummaryService(
            _ticketRepositoryMock.Object,
            _progressRepositoryMock.Object,
            _reasoningRepositoryMock.Object,
            _sessionRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    #region UpdateProgressSummaryAsync Tests

    [Fact]
    public async Task UpdateProgressSummaryAsync_WithEntries_UpdatesTicketSummary()
    {
        // Arrange
        var ticketId = Guid.NewGuid();
        var ticket = new Ticket { Id = ticketId, Title = "Test Ticket" };
        var entries = new List<ProgressEntry>
        {
            new() { Id = Guid.NewGuid(), TicketId = ticketId, Content = "Completed first task", Outcome = ProgressOutcome.Success, CreatedAt = DateTime.UtcNow.AddHours(-2) },
            new() { Id = Guid.NewGuid(), TicketId = ticketId, Content = "Working on second task", Outcome = ProgressOutcome.Success, CreatedAt = DateTime.UtcNow.AddHours(-1) },
            new() { Id = Guid.NewGuid(), TicketId = ticketId, Content = "Latest progress entry", Outcome = ProgressOutcome.Success, CreatedAt = DateTime.UtcNow }
        };

        _ticketRepositoryMock.Setup(r => r.GetByIdAsync(ticketId, default))
            .ReturnsAsync(ticket);
        _progressRepositoryMock.Setup(r => r.GetByTicketIdAsync(ticketId, null, default))
            .ReturnsAsync(entries);

        // Act
        await _service.UpdateProgressSummaryAsync(ticketId);

        // Assert
        Assert.NotNull(ticket.ProgressSummary);
        Assert.Contains("3 completed", ticket.ProgressSummary);
        Assert.NotNull(ticket.OutcomeStatistics);
        Assert.NotNull(ticket.SummaryUpdatedAt);
        _ticketRepositoryMock.Verify(r => r.UpdateAsync(ticket, default), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task UpdateProgressSummaryAsync_WithBlockedEntry_IncludesBlockerInfo()
    {
        // Arrange
        var ticketId = Guid.NewGuid();
        var ticket = new Ticket { Id = ticketId, Title = "Test Ticket" };
        var entries = new List<ProgressEntry>
        {
            new() { Id = Guid.NewGuid(), TicketId = ticketId, Content = "First task done", Outcome = ProgressOutcome.Success, CreatedAt = DateTime.UtcNow.AddHours(-2) },
            new() { Id = Guid.NewGuid(), TicketId = ticketId, Content = "Blocked by missing API key", Outcome = ProgressOutcome.Blocked, CreatedAt = DateTime.UtcNow }
        };

        _ticketRepositoryMock.Setup(r => r.GetByIdAsync(ticketId, default))
            .ReturnsAsync(ticket);
        _progressRepositoryMock.Setup(r => r.GetByTicketIdAsync(ticketId, null, default))
            .ReturnsAsync(entries);

        // Act
        await _service.UpdateProgressSummaryAsync(ticketId);

        // Assert
        Assert.NotNull(ticket.ProgressSummary);
        Assert.Contains("Blocked", ticket.ProgressSummary);
    }

    [Fact]
    public async Task UpdateProgressSummaryAsync_NonExistentTicket_DoesNothing()
    {
        // Arrange
        var ticketId = Guid.NewGuid();

        _ticketRepositoryMock.Setup(r => r.GetByIdAsync(ticketId, default))
            .ReturnsAsync((Ticket?)null);

        // Act
        await _service.UpdateProgressSummaryAsync(ticketId);

        // Assert
        _ticketRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Ticket>(), default), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task UpdateProgressSummaryAsync_CalculatesOutcomeStatisticsCorrectly()
    {
        // Arrange
        var ticketId = Guid.NewGuid();
        var ticket = new Ticket { Id = ticketId, Title = "Test Ticket" };
        var entries = new List<ProgressEntry>
        {
            new() { Id = Guid.NewGuid(), TicketId = ticketId, Content = "Success 1", Outcome = ProgressOutcome.Success, CreatedAt = DateTime.UtcNow.AddHours(-4) },
            new() { Id = Guid.NewGuid(), TicketId = ticketId, Content = "Success 2", Outcome = ProgressOutcome.Success, CreatedAt = DateTime.UtcNow.AddHours(-3) },
            new() { Id = Guid.NewGuid(), TicketId = ticketId, Content = "Failure 1", Outcome = ProgressOutcome.Failure, CreatedAt = DateTime.UtcNow.AddHours(-2) },
            new() { Id = Guid.NewGuid(), TicketId = ticketId, Content = "Partial 1", Outcome = ProgressOutcome.Partial, CreatedAt = DateTime.UtcNow.AddHours(-1) },
            new() { Id = Guid.NewGuid(), TicketId = ticketId, Content = "Blocked 1", Outcome = ProgressOutcome.Blocked, CreatedAt = DateTime.UtcNow }
        };

        _ticketRepositoryMock.Setup(r => r.GetByIdAsync(ticketId, default))
            .ReturnsAsync(ticket);
        _progressRepositoryMock.Setup(r => r.GetByTicketIdAsync(ticketId, null, default))
            .ReturnsAsync(entries);

        // Act
        await _service.UpdateProgressSummaryAsync(ticketId);

        // Assert
        Assert.NotNull(ticket.OutcomeStatistics);
        Assert.Contains("\"success\":2", ticket.OutcomeStatistics.ToLower());
        Assert.Contains("\"failure\":1", ticket.OutcomeStatistics.ToLower());
        Assert.Contains("\"partial\":1", ticket.OutcomeStatistics.ToLower());
        Assert.Contains("\"blocked\":1", ticket.OutcomeStatistics.ToLower());
    }

    #endregion

    #region UpdateDecisionSummaryAsync Tests

    [Fact]
    public async Task UpdateDecisionSummaryAsync_WithLogs_UpdatesTicketSummary()
    {
        // Arrange
        var ticketId = Guid.NewGuid();
        var ticket = new Ticket { Id = ticketId, Title = "Test Ticket" };
        var logs = new List<ReasoningLog>
        {
            new() { Id = Guid.NewGuid(), TicketId = ticketId, DecisionPoint = "Database choice", ChosenOption = "SQL Server", Rationale = "Team familiarity", ConfidencePercent = 90, CreatedAt = DateTime.UtcNow.AddHours(-1) },
            new() { Id = Guid.NewGuid(), TicketId = ticketId, DecisionPoint = "ORM choice", ChosenOption = "EF Core", Rationale = "Standard .NET ORM", ConfidencePercent = 85, CreatedAt = DateTime.UtcNow }
        };

        _ticketRepositoryMock.Setup(r => r.GetByIdAsync(ticketId, default))
            .ReturnsAsync(ticket);
        _reasoningRepositoryMock.Setup(r => r.GetByTicketIdAsync(ticketId, null, default))
            .ReturnsAsync(logs);

        // Act
        await _service.UpdateDecisionSummaryAsync(ticketId);

        // Assert
        Assert.NotNull(ticket.DecisionSummary);
        Assert.Contains("2 decisions made", ticket.DecisionSummary);
        Assert.NotNull(ticket.SummaryUpdatedAt);
        _ticketRepositoryMock.Verify(r => r.UpdateAsync(ticket, default), Times.Once);
    }

    [Fact]
    public async Task UpdateDecisionSummaryAsync_NoLogs_SetsDefaultMessage()
    {
        // Arrange
        var ticketId = Guid.NewGuid();
        var ticket = new Ticket { Id = ticketId, Title = "Test Ticket" };

        _ticketRepositoryMock.Setup(r => r.GetByIdAsync(ticketId, default))
            .ReturnsAsync(ticket);
        _reasoningRepositoryMock.Setup(r => r.GetByTicketIdAsync(ticketId, null, default))
            .ReturnsAsync(new List<ReasoningLog>());

        // Act
        await _service.UpdateDecisionSummaryAsync(ticketId);

        // Assert
        Assert.Equal("No decisions logged yet.", ticket.DecisionSummary);
    }

    [Fact]
    public async Task UpdateDecisionSummaryAsync_IncludesHighConfidenceDecisions()
    {
        // Arrange
        var ticketId = Guid.NewGuid();
        var ticket = new Ticket { Id = ticketId, Title = "Test Ticket" };
        var logs = new List<ReasoningLog>
        {
            new() { Id = Guid.NewGuid(), TicketId = ticketId, DecisionPoint = "Low confidence", ChosenOption = "Option A", Rationale = "Maybe", ConfidencePercent = 50, CreatedAt = DateTime.UtcNow.AddHours(-2) },
            new() { Id = Guid.NewGuid(), TicketId = ticketId, DecisionPoint = "High confidence", ChosenOption = "Option B", Rationale = "Definitely", ConfidencePercent = 95, CreatedAt = DateTime.UtcNow }
        };

        _ticketRepositoryMock.Setup(r => r.GetByIdAsync(ticketId, default))
            .ReturnsAsync(ticket);
        _reasoningRepositoryMock.Setup(r => r.GetByTicketIdAsync(ticketId, null, default))
            .ReturnsAsync(logs);

        // Act
        await _service.UpdateDecisionSummaryAsync(ticketId);

        // Assert
        Assert.NotNull(ticket.DecisionSummary);
        Assert.Contains("Key:", ticket.DecisionSummary);
        Assert.Contains("High confidence", ticket.DecisionSummary);
        // Low confidence decision should not be included in key decisions
    }

    #endregion

    #region GetPlanningDataSummaryAsync Tests

    [Fact]
    public async Task GetPlanningDataSummaryAsync_ReturnsCorrectSummary()
    {
        // Arrange
        var ticketId = Guid.NewGuid();
        var ticket = new Ticket
        {
            Id = ticketId,
            Title = "Test Ticket",
            ProgressSummary = "Test progress summary",
            DecisionSummary = "Test decision summary",
            OutcomeStatistics = "{\"success\":3,\"failure\":1,\"partial\":0,\"blocked\":0}",
            SummaryUpdatedAt = DateTime.UtcNow
        };
        var sessions = new List<PlanningSession>
        {
            new() { Id = Guid.NewGuid(), TicketId = ticketId, InitialUnderstanding = "Test understanding", CreatedAt = DateTime.UtcNow }
        };
        var logs = new List<ReasoningLog>
        {
            new() { Id = Guid.NewGuid(), TicketId = ticketId, DecisionPoint = "Test decision", ChosenOption = "Option A", Rationale = "Because", ConfidencePercent = 80, CreatedAt = DateTime.UtcNow }
        };
        var entries = new List<ProgressEntry>
        {
            new() { Id = Guid.NewGuid(), TicketId = ticketId, Content = "Test entry 1", Outcome = ProgressOutcome.Success, CreatedAt = DateTime.UtcNow.AddHours(-1) },
            new() { Id = Guid.NewGuid(), TicketId = ticketId, Content = "Test entry 2", Outcome = ProgressOutcome.Success, CreatedAt = DateTime.UtcNow }
        };

        _ticketRepositoryMock.Setup(r => r.GetByIdAsync(ticketId, default))
            .ReturnsAsync(ticket);
        _sessionRepositoryMock.Setup(r => r.GetByTicketIdAsync(ticketId, default))
            .ReturnsAsync(sessions);
        _reasoningRepositoryMock.Setup(r => r.GetByTicketIdAsync(ticketId, null, default))
            .ReturnsAsync(logs);
        _progressRepositoryMock.Setup(r => r.GetByTicketIdAsync(ticketId, null, default))
            .ReturnsAsync(entries);

        // Act
        var result = await _service.GetPlanningDataSummaryAsync(ticketId, 5);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test progress summary", result.ProgressSummary);
        Assert.Equal("Test decision summary", result.DecisionSummary);
        Assert.Equal(1, result.TotalSessions);
        Assert.Equal(1, result.TotalReasoningLogs);
        Assert.Equal(2, result.TotalProgressEntries);
        Assert.NotNull(result.OutcomeStatistics);
        Assert.Equal(3, result.OutcomeStatistics.Success);
        Assert.True(result.FullHistoryAvailable);
    }

    [Fact]
    public async Task GetPlanningDataSummaryAsync_ReturnsRecentItemsInCorrectOrder()
    {
        // Arrange
        var ticketId = Guid.NewGuid();
        var ticket = new Ticket { Id = ticketId, Title = "Test Ticket" };
        var entries = new List<ProgressEntry>
        {
            new() { Id = Guid.NewGuid(), TicketId = ticketId, Content = "Oldest", Outcome = ProgressOutcome.Success, CreatedAt = DateTime.UtcNow.AddHours(-3) },
            new() { Id = Guid.NewGuid(), TicketId = ticketId, Content = "Middle", Outcome = ProgressOutcome.Success, CreatedAt = DateTime.UtcNow.AddHours(-2) },
            new() { Id = Guid.NewGuid(), TicketId = ticketId, Content = "Newest", Outcome = ProgressOutcome.Success, CreatedAt = DateTime.UtcNow }
        };

        _ticketRepositoryMock.Setup(r => r.GetByIdAsync(ticketId, default))
            .ReturnsAsync(ticket);
        _sessionRepositoryMock.Setup(r => r.GetByTicketIdAsync(ticketId, default))
            .ReturnsAsync(new List<PlanningSession>());
        _reasoningRepositoryMock.Setup(r => r.GetByTicketIdAsync(ticketId, null, default))
            .ReturnsAsync(new List<ReasoningLog>());
        _progressRepositoryMock.Setup(r => r.GetByTicketIdAsync(ticketId, null, default))
            .ReturnsAsync(entries);

        // Act
        var result = await _service.GetPlanningDataSummaryAsync(ticketId, 5);

        // Assert
        Assert.Equal(3, result.RecentProgress.Count);
        Assert.Equal("Newest", result.RecentProgress[0].Content);
        Assert.Equal("Middle", result.RecentProgress[1].Content);
        Assert.Equal("Oldest", result.RecentProgress[2].Content);
        Assert.Equal("Newest", result.LastProgressEntry?.Content);
    }

    [Fact]
    public async Task GetPlanningDataSummaryAsync_RespectsRecentCountParameter()
    {
        // Arrange
        var ticketId = Guid.NewGuid();
        var ticket = new Ticket { Id = ticketId, Title = "Test Ticket" };
        var entries = Enumerable.Range(1, 10)
            .Select(i => new ProgressEntry
            {
                Id = Guid.NewGuid(),
                TicketId = ticketId,
                Content = $"Entry {i}",
                Outcome = ProgressOutcome.Success,
                CreatedAt = DateTime.UtcNow.AddHours(-i)
            })
            .ToList();

        _ticketRepositoryMock.Setup(r => r.GetByIdAsync(ticketId, default))
            .ReturnsAsync(ticket);
        _sessionRepositoryMock.Setup(r => r.GetByTicketIdAsync(ticketId, default))
            .ReturnsAsync(new List<PlanningSession>());
        _reasoningRepositoryMock.Setup(r => r.GetByTicketIdAsync(ticketId, null, default))
            .ReturnsAsync(new List<ReasoningLog>());
        _progressRepositoryMock.Setup(r => r.GetByTicketIdAsync(ticketId, null, default))
            .ReturnsAsync(entries);

        // Act
        var result = await _service.GetPlanningDataSummaryAsync(ticketId, recentCount: 3);

        // Assert
        Assert.Equal(3, result.RecentProgress.Count);
        Assert.Equal(10, result.TotalProgressEntries);
    }

    [Fact]
    public async Task GetPlanningDataSummaryAsync_CalculatesStatsFromEntriesWhenNotOnTicket()
    {
        // Arrange
        var ticketId = Guid.NewGuid();
        var ticket = new Ticket
        {
            Id = ticketId,
            Title = "Test Ticket",
            OutcomeStatistics = null // No cached statistics
        };
        var entries = new List<ProgressEntry>
        {
            new() { Id = Guid.NewGuid(), TicketId = ticketId, Content = "Success 1", Outcome = ProgressOutcome.Success, CreatedAt = DateTime.UtcNow.AddHours(-2) },
            new() { Id = Guid.NewGuid(), TicketId = ticketId, Content = "Success 2", Outcome = ProgressOutcome.Success, CreatedAt = DateTime.UtcNow.AddHours(-1) },
            new() { Id = Guid.NewGuid(), TicketId = ticketId, Content = "Failure 1", Outcome = ProgressOutcome.Failure, CreatedAt = DateTime.UtcNow }
        };

        _ticketRepositoryMock.Setup(r => r.GetByIdAsync(ticketId, default))
            .ReturnsAsync(ticket);
        _sessionRepositoryMock.Setup(r => r.GetByTicketIdAsync(ticketId, default))
            .ReturnsAsync(new List<PlanningSession>());
        _reasoningRepositoryMock.Setup(r => r.GetByTicketIdAsync(ticketId, null, default))
            .ReturnsAsync(new List<ReasoningLog>());
        _progressRepositoryMock.Setup(r => r.GetByTicketIdAsync(ticketId, null, default))
            .ReturnsAsync(entries);

        // Act
        var result = await _service.GetPlanningDataSummaryAsync(ticketId, 5);

        // Assert
        Assert.NotNull(result.OutcomeStatistics);
        Assert.Equal(2, result.OutcomeStatistics.Success);
        Assert.Equal(1, result.OutcomeStatistics.Failure);
        Assert.Equal(0, result.OutcomeStatistics.Partial);
        Assert.Equal(0, result.OutcomeStatistics.Blocked);
        Assert.Equal(3, result.OutcomeStatistics.Total);
    }

    #endregion
}
