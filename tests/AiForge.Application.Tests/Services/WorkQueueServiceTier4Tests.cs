using AiForge.Application.Services;
using AiForge.Domain.Entities;
using AiForge.Domain.Enums;
using AiForge.Domain.ValueObjects;
using AiForge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace AiForge.Application.Tests.Services;

public class WorkQueueServiceTier4Tests : IDisposable
{
    private readonly AiForgeDbContext _context;
    private readonly WorkQueueService _service;
    private readonly Guid _projectId;

    public WorkQueueServiceTier4Tests()
    {
        var options = new DbContextOptionsBuilder<AiForgeDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AiForgeDbContext(options);
        var logger = new Mock<ILogger<WorkQueueService>>();
        _service = new WorkQueueService(_context, logger.Object);

        // Set up required parent entities
        _projectId = Guid.NewGuid();

        var project = new Project
        {
            Id = _projectId,
            Key = "TEST",
            Name = "Test Project"
        };
        _context.Projects.Add(project);
        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    #region Tier 4 Context Tests

    [Fact]
    public async Task GetTieredContextAsync_Tier4_EmptyQueueItems_ReturnsEmptyArrays()
    {
        // Arrange
        var queue = CreateWorkQueue("Empty Queue");
        _context.WorkQueues.Add(queue);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetTieredContextAsync(queue.Id, 4);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Tier4);
        Assert.Empty(result.Tier4.RecentFileSnapshots);
        Assert.Empty(result.Tier4.RelatedFiles);
    }

    [Fact]
    public async Task GetTieredContextAsync_Tier4_ItemsWithNoSnapshots_ReturnsEmptySnapshots()
    {
        // Arrange
        var ticket = CreateTicket("Test Ticket");
        _context.Tickets.Add(ticket);

        var queue = CreateWorkQueue("Queue with items");
        queue.Items.Add(new WorkQueueItem
        {
            Id = Guid.NewGuid(),
            WorkQueueId = queue.Id,
            WorkItemId = ticket.Id,
            WorkItemType = WorkItemType.Task,
            Position = 1,
            Status = WorkQueueItemStatus.Pending
        });
        _context.WorkQueues.Add(queue);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetTieredContextAsync(queue.Id, 4);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Tier4);
        Assert.Empty(result.Tier4.RecentFileSnapshots);
        Assert.Empty(result.Tier4.RelatedFiles);
    }

    [Fact]
    public async Task GetTieredContextAsync_Tier4_WithFileSnapshots_ReturnsMappedSummaries()
    {
        // Arrange
        var ticket = CreateTicket("Test Ticket");
        _context.Tickets.Add(ticket);

        var handoff = new HandoffDocument
        {
            Id = Guid.NewGuid(),
            TicketId = ticket.Id,
            Title = "Test Handoff",
            Summary = "Summary",
            Content = "Content",
            Type = HandoffType.SessionEnd
        };
        _context.HandoffDocuments.Add(handoff);

        var snapshot = new FileSnapshot
        {
            Id = Guid.NewGuid(),
            HandoffId = handoff.Id,
            FilePath = "src/Test.cs",
            ContentBefore = "old code",
            ContentAfter = "new code",
            Language = "csharp",
            CreatedAt = DateTime.UtcNow
        };
        _context.FileSnapshots.Add(snapshot);

        var queue = CreateWorkQueue("Queue with snapshots");
        queue.Items.Add(new WorkQueueItem
        {
            Id = Guid.NewGuid(),
            WorkQueueId = queue.Id,
            WorkItemId = ticket.Id,
            WorkItemType = WorkItemType.Task,
            Position = 1,
            Status = WorkQueueItemStatus.Pending
        });
        _context.WorkQueues.Add(queue);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetTieredContextAsync(queue.Id, 4);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Tier4);
        Assert.Single(result.Tier4.RecentFileSnapshots);

        var snapshotSummary = result.Tier4.RecentFileSnapshots[0];
        Assert.Equal("src/Test.cs", snapshotSummary.FilePath);
        Assert.Equal("Modified", snapshotSummary.ChangeType);
    }

    [Fact]
    public async Task GetTieredContextAsync_Tier4_InfersChangeType_Created()
    {
        // Arrange
        var ticket = CreateTicket("Test Ticket");
        _context.Tickets.Add(ticket);

        var handoff = new HandoffDocument
        {
            Id = Guid.NewGuid(),
            TicketId = ticket.Id,
            Title = "Test Handoff",
            Summary = "Summary",
            Content = "Content",
            Type = HandoffType.SessionEnd
        };
        _context.HandoffDocuments.Add(handoff);

        // New file: no ContentBefore
        var snapshot = new FileSnapshot
        {
            Id = Guid.NewGuid(),
            HandoffId = handoff.Id,
            FilePath = "src/NewFile.cs",
            ContentBefore = null,
            ContentAfter = "new code",
            Language = "csharp",
            CreatedAt = DateTime.UtcNow
        };
        _context.FileSnapshots.Add(snapshot);

        var queue = CreateWorkQueue("Queue");
        queue.Items.Add(new WorkQueueItem
        {
            Id = Guid.NewGuid(),
            WorkQueueId = queue.Id,
            WorkItemId = ticket.Id,
            WorkItemType = WorkItemType.Task,
            Position = 1,
            Status = WorkQueueItemStatus.Pending
        });
        _context.WorkQueues.Add(queue);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetTieredContextAsync(queue.Id, 4);

        // Assert
        Assert.NotNull(result?.Tier4);
        Assert.Single(result.Tier4.RecentFileSnapshots);
        Assert.Equal("Created", result.Tier4.RecentFileSnapshots[0].ChangeType);
    }

    [Fact]
    public async Task GetTieredContextAsync_Tier4_InfersChangeType_Deleted()
    {
        // Arrange
        var ticket = CreateTicket("Test Ticket");
        _context.Tickets.Add(ticket);

        var handoff = new HandoffDocument
        {
            Id = Guid.NewGuid(),
            TicketId = ticket.Id,
            Title = "Test Handoff",
            Summary = "Summary",
            Content = "Content",
            Type = HandoffType.SessionEnd
        };
        _context.HandoffDocuments.Add(handoff);

        // Deleted file: no ContentAfter
        var snapshot = new FileSnapshot
        {
            Id = Guid.NewGuid(),
            HandoffId = handoff.Id,
            FilePath = "src/DeletedFile.cs",
            ContentBefore = "old code",
            ContentAfter = null,
            Language = "csharp",
            CreatedAt = DateTime.UtcNow
        };
        _context.FileSnapshots.Add(snapshot);

        var queue = CreateWorkQueue("Queue");
        queue.Items.Add(new WorkQueueItem
        {
            Id = Guid.NewGuid(),
            WorkQueueId = queue.Id,
            WorkItemId = ticket.Id,
            WorkItemType = WorkItemType.Task,
            Position = 1,
            Status = WorkQueueItemStatus.Pending
        });
        _context.WorkQueues.Add(queue);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetTieredContextAsync(queue.Id, 4);

        // Assert
        Assert.NotNull(result?.Tier4);
        Assert.Single(result.Tier4.RecentFileSnapshots);
        Assert.Equal("Deleted", result.Tier4.RecentFileSnapshots[0].ChangeType);
    }

    [Fact]
    public async Task GetTieredContextAsync_Tier4_WithFileChanges_ReturnsRelatedFiles()
    {
        // Arrange
        var ticket = CreateTicket("Test Ticket");
        _context.Tickets.Add(ticket);

        var fileChange1 = new FileChange
        {
            Id = Guid.NewGuid(),
            TicketId = ticket.Id,
            FilePath = "src/Service.cs",
            ChangeType = FileChangeType.Modified
        };
        var fileChange2 = new FileChange
        {
            Id = Guid.NewGuid(),
            TicketId = ticket.Id,
            FilePath = "src/Controller.cs",
            ChangeType = FileChangeType.Created
        };
        _context.FileChanges.AddRange(fileChange1, fileChange2);

        var queue = CreateWorkQueue("Queue with file changes");
        queue.Items.Add(new WorkQueueItem
        {
            Id = Guid.NewGuid(),
            WorkQueueId = queue.Id,
            WorkItemId = ticket.Id,
            WorkItemType = WorkItemType.Task,
            Position = 1,
            Status = WorkQueueItemStatus.Pending
        });
        _context.WorkQueues.Add(queue);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetTieredContextAsync(queue.Id, 4);

        // Assert
        Assert.NotNull(result?.Tier4);
        Assert.Equal(2, result.Tier4.RelatedFiles.Count);
        Assert.Contains("src/Service.cs", result.Tier4.RelatedFiles);
        Assert.Contains("src/Controller.cs", result.Tier4.RelatedFiles);
    }

    [Fact]
    public async Task GetTieredContextAsync_Tier4_CompletedItems_ExcludedFromQuery()
    {
        // Arrange
        var completedTicket = CreateTicket("Completed Ticket");
        var pendingTicket = CreateTicket("Pending Ticket");
        _context.Tickets.AddRange(completedTicket, pendingTicket);

        // File change for completed ticket (should be excluded)
        var completedFileChange = new FileChange
        {
            Id = Guid.NewGuid(),
            TicketId = completedTicket.Id,
            FilePath = "src/Completed.cs",
            ChangeType = FileChangeType.Modified
        };
        // File change for pending ticket (should be included)
        var pendingFileChange = new FileChange
        {
            Id = Guid.NewGuid(),
            TicketId = pendingTicket.Id,
            FilePath = "src/Pending.cs",
            ChangeType = FileChangeType.Modified
        };
        _context.FileChanges.AddRange(completedFileChange, pendingFileChange);

        var queue = CreateWorkQueue("Mixed status queue");
        queue.Items.Add(new WorkQueueItem
        {
            Id = Guid.NewGuid(),
            WorkQueueId = queue.Id,
            WorkItemId = completedTicket.Id,
            WorkItemType = WorkItemType.Task,
            Position = 1,
            Status = WorkQueueItemStatus.Completed // Completed - should be excluded
        });
        queue.Items.Add(new WorkQueueItem
        {
            Id = Guid.NewGuid(),
            WorkQueueId = queue.Id,
            WorkItemId = pendingTicket.Id,
            WorkItemType = WorkItemType.Task,
            Position = 2,
            Status = WorkQueueItemStatus.Pending // Pending - should be included
        });
        _context.WorkQueues.Add(queue);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetTieredContextAsync(queue.Id, 4);

        // Assert
        Assert.NotNull(result?.Tier4);
        Assert.Single(result.Tier4.RelatedFiles);
        Assert.Equal("src/Pending.cs", result.Tier4.RelatedFiles[0]);
    }

    [Fact]
    public async Task GetTieredContextAsync_Tier4_DeduplicatesRelatedFiles()
    {
        // Arrange
        var ticket = CreateTicket("Test Ticket");
        _context.Tickets.Add(ticket);

        // Same file modified multiple times
        for (int i = 0; i < 5; i++)
        {
            _context.FileChanges.Add(new FileChange
            {
                Id = Guid.NewGuid(),
                TicketId = ticket.Id,
                FilePath = "src/Repeated.cs", // Same file
                ChangeType = FileChangeType.Modified,
                CreatedAt = DateTime.UtcNow.AddMinutes(-i)
            });
        }

        var queue = CreateWorkQueue("Queue");
        queue.Items.Add(new WorkQueueItem
        {
            Id = Guid.NewGuid(),
            WorkQueueId = queue.Id,
            WorkItemId = ticket.Id,
            WorkItemType = WorkItemType.Task,
            Position = 1,
            Status = WorkQueueItemStatus.Pending
        });
        _context.WorkQueues.Add(queue);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetTieredContextAsync(queue.Id, 4);

        // Assert
        Assert.NotNull(result?.Tier4);
        Assert.Single(result.Tier4.RelatedFiles); // Should be deduplicated
        Assert.Equal("src/Repeated.cs", result.Tier4.RelatedFiles[0]);
    }

    #endregion

    #region Helper Methods

    private WorkQueue CreateWorkQueue(string name) => new()
    {
        Id = Guid.NewGuid(),
        ProjectId = _projectId,
        Name = name,
        Status = WorkQueueStatus.Active,
        Context = new ContextHelper
        {
            CurrentFocus = "Test focus",
            LastUpdated = DateTime.UtcNow
        }
    };

    private Ticket CreateTicket(string title) => new()
    {
        Id = Guid.NewGuid(),
        ProjectId = _projectId,
        Title = title,
        Status = TicketStatus.ToDo,
        Type = TicketType.Task,
        Priority = Priority.Medium
    };

    #endregion
}
