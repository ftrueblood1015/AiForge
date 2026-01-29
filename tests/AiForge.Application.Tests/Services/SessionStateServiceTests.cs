using AiForge.Application.DTOs.SessionState;
using AiForge.Application.Services;
using AiForge.Domain.Entities;
using AiForge.Domain.Enums;
using AiForge.Domain.Interfaces;
using AiForge.Infrastructure.Repositories;
using Moq;

namespace AiForge.Application.Tests.Services;

public class SessionStateServiceTests
{
    private readonly Mock<ISessionStateRepository> _repositoryMock;
    private readonly Mock<ITicketRepository> _ticketRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly SessionStateService _service;

    public SessionStateServiceTests()
    {
        _repositoryMock = new Mock<ISessionStateRepository>();
        _ticketRepositoryMock = new Mock<ITicketRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _service = new SessionStateService(
            _repositoryMock.Object,
            _ticketRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    #region SaveAsync Tests

    [Fact]
    public async Task SaveAsync_NewSession_CreatesSessionState()
    {
        // Arrange
        var request = new SaveSessionStateRequest
        {
            SessionId = "test-session-123",
            CurrentPhase = "Implementing",
            WorkingSummary = "Working on feature X",
            ExpiresInHours = 24
        };

        _repositoryMock.Setup(r => r.GetBySessionIdAsync("test-session-123", default))
            .ReturnsAsync((SessionState?)null);
        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<SessionState>(), default))
            .ReturnsAsync((SessionState s, CancellationToken _) => s);

        // Act
        var result = await _service.SaveAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("test-session-123", result.SessionId);
        Assert.Equal("Implementing", result.CurrentPhase);
        Assert.Equal("Working on feature X", result.WorkingSummary);
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<SessionState>(), default), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task SaveAsync_ExistingSession_UpdatesSessionState()
    {
        // Arrange
        var existingState = new SessionState
        {
            Id = Guid.NewGuid(),
            SessionId = "test-session-123",
            CurrentPhase = SessionPhase.Researching,
            WorkingSummary = "Old summary",
            CreatedAt = DateTime.UtcNow.AddHours(-1),
            UpdatedAt = DateTime.UtcNow.AddHours(-1),
            ExpiresAt = DateTime.UtcNow.AddHours(23)
        };

        var request = new SaveSessionStateRequest
        {
            SessionId = "test-session-123",
            CurrentPhase = "Implementing",
            WorkingSummary = "New summary",
            ExpiresInHours = 24
        };

        _repositoryMock.Setup(r => r.GetBySessionIdAsync("test-session-123", default))
            .ReturnsAsync(existingState);

        // Act
        var result = await _service.SaveAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("test-session-123", result.SessionId);
        Assert.Equal("Implementing", result.CurrentPhase);
        Assert.Equal("New summary", result.WorkingSummary);
        _repositoryMock.Verify(r => r.UpdateAsync(existingState, default), Times.Once);
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<SessionState>(), default), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task SaveAsync_WithTicketId_AssociatesWithTicket()
    {
        // Arrange
        var ticketId = Guid.NewGuid();
        var ticket = new Ticket { Id = ticketId, Key = "AIFORGE-1" };
        var request = new SaveSessionStateRequest
        {
            SessionId = "test-session-123",
            TicketId = ticketId,
            CurrentPhase = "Planning"
        };

        _repositoryMock.Setup(r => r.GetBySessionIdAsync("test-session-123", default))
            .ReturnsAsync((SessionState?)null);
        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<SessionState>(), default))
            .ReturnsAsync((SessionState s, CancellationToken _) => s);
        _ticketRepositoryMock.Setup(r => r.GetByIdAsync(ticketId, default))
            .ReturnsAsync(ticket);

        // Act
        var result = await _service.SaveAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ticketId, result.TicketId);
        Assert.Equal("AIFORGE-1", result.TicketKey);
    }

    [Fact]
    public async Task SaveAsync_WithCheckpoint_SerializesCheckpointData()
    {
        // Arrange
        var request = new SaveSessionStateRequest
        {
            SessionId = "test-session-123",
            CurrentPhase = "Implementing",
            Checkpoint = new Dictionary<string, object>
            {
                { "currentFile", "test.cs" },
                { "lineNumber", 42 }
            }
        };

        _repositoryMock.Setup(r => r.GetBySessionIdAsync("test-session-123", default))
            .ReturnsAsync((SessionState?)null);
        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<SessionState>(), default))
            .ReturnsAsync((SessionState s, CancellationToken _) => s);

        // Act
        var result = await _service.SaveAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Checkpoint);
        Assert.True(result.Checkpoint.ContainsKey("currentFile"));
    }

    #endregion

    #region LoadAsync Tests

    [Fact]
    public async Task LoadAsync_ExistingSession_ReturnsSessionState()
    {
        // Arrange
        var sessionState = new SessionState
        {
            Id = Guid.NewGuid(),
            SessionId = "test-session-123",
            CurrentPhase = SessionPhase.Implementing,
            WorkingSummary = "Test summary",
            CreatedAt = DateTime.UtcNow.AddHours(-1),
            UpdatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(23)
        };

        _repositoryMock.Setup(r => r.GetBySessionIdAsync("test-session-123", default))
            .ReturnsAsync(sessionState);

        // Act
        var result = await _service.LoadAsync("test-session-123");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("test-session-123", result.SessionId);
        Assert.Equal("Implementing", result.CurrentPhase);
        Assert.Equal("Test summary", result.WorkingSummary);
    }

    [Fact]
    public async Task LoadAsync_NonExistentSession_ReturnsNull()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetBySessionIdAsync("non-existent", default))
            .ReturnsAsync((SessionState?)null);

        // Act
        var result = await _service.LoadAsync("non-existent");

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region ClearAsync Tests

    [Fact]
    public async Task ClearAsync_ExistingSession_DeletesAndReturnsTrue()
    {
        // Arrange
        var sessionState = new SessionState
        {
            Id = Guid.NewGuid(),
            SessionId = "test-session-123"
        };

        _repositoryMock.Setup(r => r.GetBySessionIdAsync("test-session-123", default))
            .ReturnsAsync(sessionState);

        // Act
        var result = await _service.ClearAsync("test-session-123");

        // Assert
        Assert.True(result);
        _repositoryMock.Verify(r => r.DeleteAsync(sessionState, default), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task ClearAsync_NonExistentSession_ReturnsFalse()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetBySessionIdAsync("non-existent", default))
            .ReturnsAsync((SessionState?)null);

        // Act
        var result = await _service.ClearAsync("non-existent");

        // Assert
        Assert.False(result);
        _repositoryMock.Verify(r => r.DeleteAsync(It.IsAny<SessionState>(), default), Times.Never);
    }

    #endregion

    #region CleanupExpiredAsync Tests

    [Fact]
    public async Task CleanupExpiredAsync_WithExpiredSessions_DeletesAndReturnsCount()
    {
        // Arrange
        var expiredSessions = new List<SessionState>
        {
            new() { Id = Guid.NewGuid(), SessionId = "expired-1" },
            new() { Id = Guid.NewGuid(), SessionId = "expired-2" },
            new() { Id = Guid.NewGuid(), SessionId = "expired-3" }
        };

        _repositoryMock.Setup(r => r.GetExpiredAsync(It.IsAny<DateTime>(), default))
            .ReturnsAsync(expiredSessions);

        // Act
        var result = await _service.CleanupExpiredAsync();

        // Assert
        Assert.Equal(3, result);
        _repositoryMock.Verify(r => r.DeleteRangeAsync(expiredSessions, default), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task CleanupExpiredAsync_NoExpiredSessions_ReturnsZero()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetExpiredAsync(It.IsAny<DateTime>(), default))
            .ReturnsAsync(new List<SessionState>());

        // Act
        var result = await _service.CleanupExpiredAsync();

        // Assert
        Assert.Equal(0, result);
        _repositoryMock.Verify(r => r.DeleteRangeAsync(It.IsAny<IEnumerable<SessionState>>(), default), Times.Never);
    }

    #endregion
}
