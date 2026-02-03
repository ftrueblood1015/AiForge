using AiForge.Application.Extensions;
using AiForge.Application.Interfaces;
using AiForge.Application.Services;
using Moq;

namespace AiForge.Application.Tests.Extensions;

public class ProjectAccessExtensionsTests
{
    private readonly Mock<IProjectMemberService> _projectMemberServiceMock;
    private readonly Mock<IUserContext> _userContextMock;

    public ProjectAccessExtensionsTests()
    {
        _projectMemberServiceMock = new Mock<IProjectMemberService>();
        _userContextMock = new Mock<IUserContext>();
    }

    [Fact]
    public async Task GetAccessibleProjectIdsOrNullAsync_ServiceAccount_ReturnsNull()
    {
        // Arrange
        _userContextMock.Setup(x => x.IsServiceAccount).Returns(true);

        // Act
        var result = await _projectMemberServiceMock.Object
            .GetAccessibleProjectIdsOrNullAsync(_userContextMock.Object);

        // Assert
        Assert.Null(result);
        _projectMemberServiceMock.Verify(
            x => x.GetAccessibleProjectIdsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetAccessibleProjectIdsOrNullAsync_Admin_ReturnsNull()
    {
        // Arrange
        _userContextMock.Setup(x => x.IsServiceAccount).Returns(false);
        _userContextMock.Setup(x => x.IsAdmin).Returns(true);

        // Act
        var result = await _projectMemberServiceMock.Object
            .GetAccessibleProjectIdsOrNullAsync(_userContextMock.Object);

        // Assert
        Assert.Null(result);
        _projectMemberServiceMock.Verify(
            x => x.GetAccessibleProjectIdsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetAccessibleProjectIdsOrNullAsync_UnauthenticatedUser_ReturnsEmptySet()
    {
        // Arrange
        _userContextMock.Setup(x => x.IsServiceAccount).Returns(false);
        _userContextMock.Setup(x => x.IsAdmin).Returns(false);
        _userContextMock.Setup(x => x.UserId).Returns((Guid?)null);

        // Act
        var result = await _projectMemberServiceMock.Object
            .GetAccessibleProjectIdsOrNullAsync(_userContextMock.Object);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAccessibleProjectIdsOrNullAsync_RegularUser_ReturnsAccessibleProjects()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var projectId1 = Guid.NewGuid();
        var projectId2 = Guid.NewGuid();
        var accessibleProjects = new[] { projectId1, projectId2 };

        _userContextMock.Setup(x => x.IsServiceAccount).Returns(false);
        _userContextMock.Setup(x => x.IsAdmin).Returns(false);
        _userContextMock.Setup(x => x.UserId).Returns(userId);
        _projectMemberServiceMock
            .Setup(x => x.GetAccessibleProjectIdsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(accessibleProjects);

        // Act
        var result = await _projectMemberServiceMock.Object
            .GetAccessibleProjectIdsOrNullAsync(_userContextMock.Object);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Contains(projectId1, result);
        Assert.Contains(projectId2, result);
    }

    [Fact]
    public async Task GetAccessibleProjectIdsOrNullAsync_UserWithNoProjects_ReturnsEmptySet()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _userContextMock.Setup(x => x.IsServiceAccount).Returns(false);
        _userContextMock.Setup(x => x.IsAdmin).Returns(false);
        _userContextMock.Setup(x => x.UserId).Returns(userId);
        _projectMemberServiceMock
            .Setup(x => x.GetAccessibleProjectIdsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Guid>());

        // Act
        var result = await _projectMemberServiceMock.Object
            .GetAccessibleProjectIdsOrNullAsync(_userContextMock.Object);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void ShouldFilter_NullAccessibleProjects_ReturnsFalse()
    {
        // Arrange
        HashSet<Guid>? accessibleProjects = null;

        // Act
        var result = accessibleProjects.ShouldFilter();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ShouldFilter_EmptyAccessibleProjects_ReturnsTrue()
    {
        // Arrange
        var accessibleProjects = new HashSet<Guid>();

        // Act
        var result = accessibleProjects.ShouldFilter();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void HasAccess_NullAccessibleProjects_ReturnsTrue()
    {
        // Arrange
        HashSet<Guid>? accessibleProjects = null;
        var projectId = Guid.NewGuid();

        // Act
        var result = accessibleProjects.HasAccess(projectId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void HasAccess_ProjectInAccessibleList_ReturnsTrue()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var accessibleProjects = new HashSet<Guid> { projectId };

        // Act
        var result = accessibleProjects.HasAccess(projectId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void HasAccess_ProjectNotInAccessibleList_ReturnsFalse()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var otherProjectId = Guid.NewGuid();
        var accessibleProjects = new HashSet<Guid> { otherProjectId };

        // Act
        var result = accessibleProjects.HasAccess(projectId);

        // Assert
        Assert.False(result);
    }
}
