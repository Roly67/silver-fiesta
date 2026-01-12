// <copyright file="RateLimitingSettingsTests.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Infrastructure.Options;

using FluentAssertions;

namespace FileConversionApi.UnitTests.Infrastructure;

/// <summary>
/// Unit tests for <see cref="RateLimitingSettings"/>.
/// </summary>
public class RateLimitingSettingsTests
{
    /// <summary>
    /// Tests that SectionName has the correct value.
    /// </summary>
    [Fact]
    public void SectionName_ShouldBeRateLimiting()
    {
        // Assert
        RateLimitingSettings.SectionName.Should().Be("RateLimiting");
    }

    /// <summary>
    /// Tests that EnableRateLimiting defaults to true.
    /// </summary>
    [Fact]
    public void EnableRateLimiting_ShouldDefaultToTrue()
    {
        // Arrange
        var settings = new RateLimitingSettings();

        // Assert
        settings.EnableRateLimiting.Should().BeTrue();
    }

    /// <summary>
    /// Tests that StandardPolicy is initialized with defaults.
    /// </summary>
    [Fact]
    public void StandardPolicy_ShouldBeInitializedWithDefaults()
    {
        // Arrange
        var settings = new RateLimitingSettings();

        // Assert
        settings.StandardPolicy.Should().NotBeNull();
        settings.StandardPolicy.PermitLimit.Should().Be(100);
        settings.StandardPolicy.WindowMinutes.Should().Be(60);
    }

    /// <summary>
    /// Tests that ConversionPolicy is initialized with defaults.
    /// </summary>
    [Fact]
    public void ConversionPolicy_ShouldBeInitializedWithDefaults()
    {
        // Arrange
        var settings = new RateLimitingSettings();

        // Assert
        settings.ConversionPolicy.Should().NotBeNull();
        settings.ConversionPolicy.PermitLimit.Should().Be(100);
        settings.ConversionPolicy.WindowMinutes.Should().Be(60);
    }

    /// <summary>
    /// Tests that AuthPolicy is initialized with defaults.
    /// </summary>
    [Fact]
    public void AuthPolicy_ShouldBeInitializedWithDefaults()
    {
        // Arrange
        var settings = new RateLimitingSettings();

        // Assert
        settings.AuthPolicy.Should().NotBeNull();
        settings.AuthPolicy.PermitLimit.Should().Be(100);
        settings.AuthPolicy.WindowMinutes.Should().Be(60);
    }

    /// <summary>
    /// Tests that EnableRateLimiting can be set.
    /// </summary>
    [Fact]
    public void EnableRateLimiting_CanBeSet()
    {
        // Arrange
        var settings = new RateLimitingSettings();

        // Act
        settings.EnableRateLimiting = false;

        // Assert
        settings.EnableRateLimiting.Should().BeFalse();
    }

    /// <summary>
    /// Tests that StandardPolicy can be set.
    /// </summary>
    [Fact]
    public void StandardPolicy_CanBeSet()
    {
        // Arrange
        var settings = new RateLimitingSettings();
        var newPolicy = new RateLimitPolicySettings { PermitLimit = 200, WindowMinutes = 30 };

        // Act
        settings.StandardPolicy = newPolicy;

        // Assert
        settings.StandardPolicy.PermitLimit.Should().Be(200);
        settings.StandardPolicy.WindowMinutes.Should().Be(30);
    }

    /// <summary>
    /// Tests that ConversionPolicy can be set.
    /// </summary>
    [Fact]
    public void ConversionPolicy_CanBeSet()
    {
        // Arrange
        var settings = new RateLimitingSettings();
        var newPolicy = new RateLimitPolicySettings { PermitLimit = 50, WindowMinutes = 120 };

        // Act
        settings.ConversionPolicy = newPolicy;

        // Assert
        settings.ConversionPolicy.PermitLimit.Should().Be(50);
        settings.ConversionPolicy.WindowMinutes.Should().Be(120);
    }

    /// <summary>
    /// Tests that AuthPolicy can be set.
    /// </summary>
    [Fact]
    public void AuthPolicy_CanBeSet()
    {
        // Arrange
        var settings = new RateLimitingSettings();
        var newPolicy = new RateLimitPolicySettings { PermitLimit = 10, WindowMinutes = 15 };

        // Act
        settings.AuthPolicy = newPolicy;

        // Assert
        settings.AuthPolicy.PermitLimit.Should().Be(10);
        settings.AuthPolicy.WindowMinutes.Should().Be(15);
    }
}
