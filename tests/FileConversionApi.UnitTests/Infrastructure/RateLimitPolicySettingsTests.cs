// <copyright file="RateLimitPolicySettingsTests.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Infrastructure.Options;

using FluentAssertions;

namespace FileConversionApi.UnitTests.Infrastructure;

/// <summary>
/// Unit tests for <see cref="RateLimitPolicySettings"/>.
/// </summary>
public class RateLimitPolicySettingsTests
{
    /// <summary>
    /// Tests that PermitLimit defaults to 100.
    /// </summary>
    [Fact]
    public void PermitLimit_ShouldDefaultTo100()
    {
        // Arrange
        var settings = new RateLimitPolicySettings();

        // Assert
        settings.PermitLimit.Should().Be(100);
    }

    /// <summary>
    /// Tests that WindowMinutes defaults to 60.
    /// </summary>
    [Fact]
    public void WindowMinutes_ShouldDefaultTo60()
    {
        // Arrange
        var settings = new RateLimitPolicySettings();

        // Assert
        settings.WindowMinutes.Should().Be(60);
    }

    /// <summary>
    /// Tests that PermitLimit can be set.
    /// </summary>
    [Fact]
    public void PermitLimit_CanBeSet()
    {
        // Arrange
        var settings = new RateLimitPolicySettings();

        // Act
        settings.PermitLimit = 50;

        // Assert
        settings.PermitLimit.Should().Be(50);
    }

    /// <summary>
    /// Tests that WindowMinutes can be set.
    /// </summary>
    [Fact]
    public void WindowMinutes_CanBeSet()
    {
        // Arrange
        var settings = new RateLimitPolicySettings();

        // Act
        settings.WindowMinutes = 30;

        // Assert
        settings.WindowMinutes.Should().Be(30);
    }

    /// <summary>
    /// Tests that all properties can be set via object initializer.
    /// </summary>
    [Fact]
    public void ObjectInitializer_ShouldSetAllProperties()
    {
        // Arrange & Act
        var settings = new RateLimitPolicySettings
        {
            PermitLimit = 25,
            WindowMinutes = 15,
        };

        // Assert
        settings.PermitLimit.Should().Be(25);
        settings.WindowMinutes.Should().Be(15);
    }
}
