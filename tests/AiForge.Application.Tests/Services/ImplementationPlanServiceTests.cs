using AiForge.Application.DTOs.Plans;
using AiForge.Application.Services;
using AiForge.Domain.Entities;
using AiForge.Domain.Enums;
using AiForge.Domain.Interfaces;
using AiForge.Infrastructure.Repositories;
using Moq;

namespace AiForge.Application.Tests.Services;

public class ImplementationPlanServiceTests
{
    private readonly Mock<IImplementationPlanRepository> _planRepositoryMock;
    private readonly Mock<ITicketRepository> _ticketRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly ImplementationPlanService _service;

    public ImplementationPlanServiceTests()
    {
        _planRepositoryMock = new Mock<IImplementationPlanRepository>();
        _ticketRepositoryMock = new Mock<ITicketRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _service = new ImplementationPlanService(
            _planRepositoryMock.Object,
            _ticketRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_WithValidRequest_CreatesDraftPlan()
    {
        // Arrange
        var ticketId = Guid.NewGuid();
        var ticket = new Ticket { Id = ticketId, Title = "Test Ticket" };
        var request = new CreateImplementationPlanRequest
        {
            Content = "## Implementation Plan\n\nStep 1: Do something",
            EstimatedEffort = "Medium",
            CreatedBy = "test-session"
        };

        _ticketRepositoryMock.Setup(r => r.GetByIdAsync(ticketId, default))
            .ReturnsAsync(ticket);
        _planRepositoryMock.Setup(r => r.GetNextVersionAsync(ticketId, default))
            .ReturnsAsync(1);
        _planRepositoryMock.Setup(r => r.AddAsync(It.IsAny<ImplementationPlan>(), default))
            .ReturnsAsync((ImplementationPlan p, CancellationToken _) => p);

        // Act
        var result = await _service.CreateAsync(ticketId, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Draft", result.Status);
        Assert.Equal(1, result.Version);
        Assert.Equal(request.Content, result.Content);
        Assert.Equal(request.EstimatedEffort, result.EstimatedEffort);
        Assert.Equal(request.CreatedBy, result.CreatedBy);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithNonExistentTicket_ThrowsException()
    {
        // Arrange
        var ticketId = Guid.NewGuid();
        var request = new CreateImplementationPlanRequest { Content = "Test" };

        _ticketRepositoryMock.Setup(r => r.GetByIdAsync(ticketId, default))
            .ReturnsAsync((Ticket?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CreateAsync(ticketId, request));
        Assert.Contains("not found", exception.Message);
    }

    [Fact]
    public async Task CreateAsync_SecondPlan_IncrementsVersion()
    {
        // Arrange
        var ticketId = Guid.NewGuid();
        var ticket = new Ticket { Id = ticketId };
        var request = new CreateImplementationPlanRequest { Content = "Test" };

        _ticketRepositoryMock.Setup(r => r.GetByIdAsync(ticketId, default))
            .ReturnsAsync(ticket);
        _planRepositoryMock.Setup(r => r.GetNextVersionAsync(ticketId, default))
            .ReturnsAsync(2); // Already has version 1
        _planRepositoryMock.Setup(r => r.AddAsync(It.IsAny<ImplementationPlan>(), default))
            .ReturnsAsync((ImplementationPlan p, CancellationToken _) => p);

        // Act
        var result = await _service.CreateAsync(ticketId, request);

        // Assert
        Assert.Equal(2, result.Version);
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_DraftPlan_UpdatesSuccessfully()
    {
        // Arrange
        var planId = Guid.NewGuid();
        var existingPlan = new ImplementationPlan
        {
            Id = planId,
            Content = "Original content",
            Status = PlanStatus.Draft,
            Version = 1
        };
        var request = new UpdateImplementationPlanRequest
        {
            Content = "Updated content",
            EstimatedEffort = "Large"
        };

        _planRepositoryMock.Setup(r => r.GetByIdAsync(planId, default))
            .ReturnsAsync(existingPlan);

        // Act
        var result = await _service.UpdateAsync(planId, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated content", result.Content);
        Assert.Equal("Large", result.EstimatedEffort);
        _planRepositoryMock.Verify(r => r.UpdateAsync(existingPlan, default), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ApprovedPlan_ThrowsException()
    {
        // Arrange
        var planId = Guid.NewGuid();
        var existingPlan = new ImplementationPlan
        {
            Id = planId,
            Status = PlanStatus.Approved
        };
        var request = new UpdateImplementationPlanRequest { Content = "Test" };

        _planRepositoryMock.Setup(r => r.GetByIdAsync(planId, default))
            .ReturnsAsync(existingPlan);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.UpdateAsync(planId, request));
        Assert.Contains("Only Draft plans can be updated", exception.Message);
    }

    [Fact]
    public async Task UpdateAsync_NonExistentPlan_ReturnsNull()
    {
        // Arrange
        var planId = Guid.NewGuid();
        var request = new UpdateImplementationPlanRequest { Content = "Test" };

        _planRepositoryMock.Setup(r => r.GetByIdAsync(planId, default))
            .ReturnsAsync((ImplementationPlan?)null);

        // Act
        var result = await _service.UpdateAsync(planId, request);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region ApproveAsync Tests

    [Fact]
    public async Task ApproveAsync_DraftPlan_ApprovesSuccessfully()
    {
        // Arrange
        var planId = Guid.NewGuid();
        var existingPlan = new ImplementationPlan
        {
            Id = planId,
            Status = PlanStatus.Draft,
            Version = 1
        };
        var request = new ApproveImplementationPlanRequest { ApprovedBy = "user@test.com" };

        _planRepositoryMock.Setup(r => r.GetByIdAsync(planId, default))
            .ReturnsAsync(existingPlan);

        // Act
        var result = await _service.ApproveAsync(planId, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Approved", result.Status);
        Assert.Equal("user@test.com", result.ApprovedBy);
        Assert.NotNull(result.ApprovedAt);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task ApproveAsync_AlreadyApproved_ThrowsException()
    {
        // Arrange
        var planId = Guid.NewGuid();
        var existingPlan = new ImplementationPlan
        {
            Id = planId,
            Status = PlanStatus.Approved
        };
        var request = new ApproveImplementationPlanRequest();

        _planRepositoryMock.Setup(r => r.GetByIdAsync(planId, default))
            .ReturnsAsync(existingPlan);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.ApproveAsync(planId, request));
        Assert.Contains("Only Draft plans can be approved", exception.Message);
    }

    #endregion

    #region RejectAsync Tests

    [Fact]
    public async Task RejectAsync_DraftPlan_RejectsSuccessfully()
    {
        // Arrange
        var planId = Guid.NewGuid();
        var existingPlan = new ImplementationPlan
        {
            Id = planId,
            Status = PlanStatus.Draft
        };
        var request = new RejectImplementationPlanRequest
        {
            RejectedBy = "reviewer",
            RejectionReason = "Needs more detail"
        };

        _planRepositoryMock.Setup(r => r.GetByIdAsync(planId, default))
            .ReturnsAsync(existingPlan);

        // Act
        var result = await _service.RejectAsync(planId, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Rejected", result.Status);
        Assert.Equal("reviewer", result.RejectedBy);
        Assert.Equal("Needs more detail", result.RejectionReason);
        Assert.NotNull(result.RejectedAt);
    }

    [Fact]
    public async Task RejectAsync_SupersededPlan_ThrowsException()
    {
        // Arrange
        var planId = Guid.NewGuid();
        var existingPlan = new ImplementationPlan
        {
            Id = planId,
            Status = PlanStatus.Superseded
        };
        var request = new RejectImplementationPlanRequest();

        _planRepositoryMock.Setup(r => r.GetByIdAsync(planId, default))
            .ReturnsAsync(existingPlan);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.RejectAsync(planId, request));
        Assert.Contains("Only Draft plans can be rejected", exception.Message);
    }

    #endregion

    #region SupersedeAsync Tests

    [Fact]
    public async Task SupersedeAsync_ApprovedPlan_CreatesNewVersionAndSupersedesOld()
    {
        // Arrange
        var oldPlanId = Guid.NewGuid();
        var ticketId = Guid.NewGuid();
        var oldPlan = new ImplementationPlan
        {
            Id = oldPlanId,
            TicketId = ticketId,
            Status = PlanStatus.Approved,
            Version = 1
        };
        var request = new SupersedeImplementationPlanRequest
        {
            Content = "New improved plan",
            CreatedBy = "claude"
        };

        _planRepositoryMock.Setup(r => r.GetByIdAsync(oldPlanId, default))
            .ReturnsAsync(oldPlan);
        _planRepositoryMock.Setup(r => r.GetNextVersionAsync(ticketId, default))
            .ReturnsAsync(2);
        _planRepositoryMock.Setup(r => r.AddAsync(It.IsAny<ImplementationPlan>(), default))
            .ReturnsAsync((ImplementationPlan p, CancellationToken _) => p);

        // Act
        var result = await _service.SupersedeAsync(oldPlanId, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Version);
        Assert.Equal("Draft", result.Status);
        Assert.Equal("New improved plan", result.Content);

        // Verify old plan was marked as superseded
        Assert.Equal(PlanStatus.Superseded, oldPlan.Status);
        Assert.NotNull(oldPlan.SupersededAt);
        Assert.NotNull(oldPlan.SupersededById);

        _planRepositoryMock.Verify(r => r.AddAsync(It.IsAny<ImplementationPlan>(), default), Times.Once);
        _planRepositoryMock.Verify(r => r.UpdateAsync(oldPlan, default), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task SupersedeAsync_DraftPlan_ThrowsException()
    {
        // Arrange
        var planId = Guid.NewGuid();
        var existingPlan = new ImplementationPlan
        {
            Id = planId,
            Status = PlanStatus.Draft
        };
        var request = new SupersedeImplementationPlanRequest { Content = "Test" };

        _planRepositoryMock.Setup(r => r.GetByIdAsync(planId, default))
            .ReturnsAsync(existingPlan);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.SupersedeAsync(planId, request));
        Assert.Contains("Only Approved plans can be superseded", exception.Message);
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_DraftPlan_DeletesSuccessfully()
    {
        // Arrange
        var planId = Guid.NewGuid();
        var existingPlan = new ImplementationPlan
        {
            Id = planId,
            Status = PlanStatus.Draft
        };

        _planRepositoryMock.Setup(r => r.GetByIdAsync(planId, default))
            .ReturnsAsync(existingPlan);

        // Act
        var result = await _service.DeleteAsync(planId);

        // Assert
        Assert.True(result);
        _planRepositoryMock.Verify(r => r.DeleteAsync(existingPlan, default), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ApprovedPlan_ThrowsException()
    {
        // Arrange
        var planId = Guid.NewGuid();
        var existingPlan = new ImplementationPlan
        {
            Id = planId,
            Status = PlanStatus.Approved
        };

        _planRepositoryMock.Setup(r => r.GetByIdAsync(planId, default))
            .ReturnsAsync(existingPlan);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.DeleteAsync(planId));
        Assert.Contains("Only Draft plans can be deleted", exception.Message);
    }

    [Fact]
    public async Task DeleteAsync_NonExistentPlan_ReturnsFalse()
    {
        // Arrange
        var planId = Guid.NewGuid();

        _planRepositoryMock.Setup(r => r.GetByIdAsync(planId, default))
            .ReturnsAsync((ImplementationPlan?)null);

        // Act
        var result = await _service.DeleteAsync(planId);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_ExistingPlan_ReturnsPlan()
    {
        // Arrange
        var planId = Guid.NewGuid();
        var plan = new ImplementationPlan
        {
            Id = planId,
            Content = "Test content",
            Status = PlanStatus.Approved,
            Version = 1,
            CreatedAt = DateTime.UtcNow
        };

        _planRepositoryMock.Setup(r => r.GetByIdAsync(planId, default))
            .ReturnsAsync(plan);

        // Act
        var result = await _service.GetByIdAsync(planId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(planId, result.Id);
        Assert.Equal("Test content", result.Content);
        Assert.Equal("Approved", result.Status);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentPlan_ReturnsNull()
    {
        // Arrange
        var planId = Guid.NewGuid();

        _planRepositoryMock.Setup(r => r.GetByIdAsync(planId, default))
            .ReturnsAsync((ImplementationPlan?)null);

        // Act
        var result = await _service.GetByIdAsync(planId);

        // Assert
        Assert.Null(result);
    }

    #endregion
}
