// <copyright file="HealthCheckSettingsTests.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Infrastructure.Options;

using FluentAssertions;

namespace FileConversionApi.UnitTests.Infrastructure.HealthChecks;

/// <summary>
/// Unit tests for <see cref="HealthCheckSettings"/>.
/// </summary>
public class HealthCheckSettingsTests
{
    /// <summary>
    /// Tests that SectionName has the correct value.
    /// </summary>
    [Fact]
    public void SectionName_ShouldBeHealthChecks()
    {
        // Assert
        HealthCheckSettings.SectionName.Should().Be("HealthChecks");
    }

    /// <summary>
    /// Tests that DiskSpaceMinimumMB defaults to 100.
    /// </summary>
    [Fact]
    public void DiskSpaceMinimumMB_ShouldDefaultTo100()
    {
        // Arrange
        var settings = new HealthCheckSettings();

        // Assert
        settings.DiskSpaceMinimumMB.Should().Be(100);
    }

    /// <summary>
    /// Tests that ChromiumTimeoutSeconds defaults to 30.
    /// </summary>
    [Fact]
    public void ChromiumTimeoutSeconds_ShouldDefaultTo30()
    {
        // Arrange
        var settings = new HealthCheckSettings();

        // Assert
        settings.ChromiumTimeoutSeconds.Should().Be(30);
    }

    /// <summary>
    /// Tests that DiskSpaceMinimumMB can be set.
    /// </summary>
    [Fact]
    public void DiskSpaceMinimumMB_CanBeSet()
    {
        // Arrange
        var settings = new HealthCheckSettings();

        // Act
        settings.DiskSpaceMinimumMB = 500;

        // Assert
        settings.DiskSpaceMinimumMB.Should().Be(500);
    }

    /// <summary>
    /// Tests that ChromiumTimeoutSeconds can be set.
    /// </summary>
    [Fact]
    public void ChromiumTimeoutSeconds_CanBeSet()
    {
        // Arrange
        var settings = new HealthCheckSettings();

        // Act
        settings.ChromiumTimeoutSeconds = 60;

        // Assert
        settings.ChromiumTimeoutSeconds.Should().Be(60);
    }

    /// <summary>
    /// Tests that all properties can be set via object initializer.
    /// </summary>
    [Fact]
    public void ObjectInitializer_ShouldSetAllProperties()
    {
        // Arrange & Act
        var settings = new HealthCheckSettings
        {
            DiskSpaceMinimumMB = 200,
            ChromiumTimeoutSeconds = 45,
        };

        // Assert
        settings.DiskSpaceMinimumMB.Should().Be(200);
        settings.ChromiumTimeoutSeconds.Should().Be(45);
    }
}
