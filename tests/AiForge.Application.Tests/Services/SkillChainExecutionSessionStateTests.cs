using AiForge.Application.DTOs.SkillChains;
using AiForge.Application.Services;
using Xunit;

namespace AiForge.Application.Tests.Services;

/// <summary>
/// Tests for the ChainSessionStateOptions configuration class.
/// The actual chain-level session state integration is tested via integration tests
/// since SkillChainExecutionService requires a full DbContext.
/// </summary>
public class ChainSessionStateOptionsTests
{
    [Fact]
    public void Default_AllOptionsEnabled()
    {
        // Act
        var options = ChainSessionStateOptions.Default;

        // Assert
        Assert.True(options.AutoSaveOnLinkComplete);
        Assert.True(options.AutoLoadOnStart);
        Assert.True(options.AutoClearOnComplete);
        Assert.True(options.AutoSaveOnPause);
        Assert.True(options.AutoSaveOnCancel);
        Assert.Equal(24, options.SessionExpiryHours);
        Assert.Null(options.SessionId);
    }

    [Fact]
    public void Disabled_AllOptionsDisabled()
    {
        // Act
        var options = ChainSessionStateOptions.Disabled;

        // Assert
        Assert.False(options.AutoSaveOnLinkComplete);
        Assert.False(options.AutoLoadOnStart);
        Assert.False(options.AutoClearOnComplete);
        Assert.False(options.AutoSaveOnPause);
        Assert.False(options.AutoSaveOnCancel);
    }

    [Fact]
    public void CustomOptions_CanBeConfigured()
    {
        // Act
        var options = new ChainSessionStateOptions
        {
            AutoSaveOnLinkComplete = true,
            AutoLoadOnStart = false,
            AutoClearOnComplete = true,
            AutoSaveOnPause = false,
            AutoSaveOnCancel = false,
            SessionExpiryHours = 48,
            SessionId = "my-custom-session"
        };

        // Assert
        Assert.True(options.AutoSaveOnLinkComplete);
        Assert.False(options.AutoLoadOnStart);
        Assert.True(options.AutoClearOnComplete);
        Assert.False(options.AutoSaveOnPause);
        Assert.False(options.AutoSaveOnCancel);
        Assert.Equal(48, options.SessionExpiryHours);
        Assert.Equal("my-custom-session", options.SessionId);
    }
}

/// <summary>
/// Tests for StartChainExecutionRequest with session state options.
/// </summary>
public class StartChainExecutionRequestTests
{
    [Fact]
    public void SessionStateOptions_DefaultsToNull()
    {
        // Act
        var request = new StartChainExecutionRequest
        {
            SkillChainId = Guid.NewGuid()
        };

        // Assert
        Assert.Null(request.SessionStateOptions);
    }

    [Fact]
    public void SessionStateOptions_CanBeSet()
    {
        // Arrange
        var customOptions = new ChainSessionStateOptions
        {
            SessionId = "test-session",
            AutoClearOnComplete = false
        };

        // Act
        var request = new StartChainExecutionRequest
        {
            SkillChainId = Guid.NewGuid(),
            SessionStateOptions = customOptions
        };

        // Assert
        Assert.NotNull(request.SessionStateOptions);
        Assert.Equal("test-session", request.SessionStateOptions.SessionId);
        Assert.False(request.SessionStateOptions.AutoClearOnComplete);
    }
}

/// <summary>
/// Tests for SkillChainExecutionDto session state properties.
/// </summary>
public class SkillChainExecutionDtoSessionStateTests
{
    [Fact]
    public void SessionStateProperties_ExistOnDto()
    {
        // Act
        var dto = new SkillChainExecutionDto
        {
            Id = Guid.NewGuid(),
            SessionId = "chain-exec-123",
            SessionStateEnabled = true,
            SessionPhase = "Implementing"
        };

        // Assert
        Assert.Equal("chain-exec-123", dto.SessionId);
        Assert.True(dto.SessionStateEnabled);
        Assert.Equal("Implementing", dto.SessionPhase);
    }

    [Fact]
    public void SessionStateProperties_DefaultToNullOrFalse()
    {
        // Act
        var dto = new SkillChainExecutionDto();

        // Assert
        Assert.Null(dto.SessionId);
        Assert.False(dto.SessionStateEnabled);
        Assert.Null(dto.SessionPhase);
        Assert.Null(dto.SessionStateUpdatedAt);
    }
}
