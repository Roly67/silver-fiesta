// <copyright file="DiskSpaceHealthCheckTests.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Infrastructure.HealthChecks;
using FileConversionApi.Infrastructure.Options;

using FluentAssertions;

using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace FileConversionApi.UnitTests.Infrastructure.HealthChecks;

/// <summary>
/// Unit tests for <see cref="DiskSpaceHealthCheck"/>.
/// </summary>
public class DiskSpaceHealthCheckTests
{
    /// <summary>
    /// Tests that constructor throws ArgumentNullException when settings is null.
    /// </summary>
    [Fact]
    public void Constructor_WhenSettingsIsNull_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new DiskSpaceHealthCheck(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("settings");
    }

    /// <summary>
    /// Tests that CheckHealthAsync returns Healthy when disk has sufficient space.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task CheckHealthAsync_WhenSufficientSpace_ReturnsHealthy()
    {
        // Arrange
        var settings = Options.Create(new HealthCheckSettings
        {
            DiskSpaceMinimumMB = 1, // 1 MB should always be available
        });

        var healthCheck = new DiskSpaceHealthCheck(settings);
        var context = new HealthCheckContext
        {
            Registration = new HealthCheckRegistration("diskSpace", healthCheck, null, null),
        };

        // Act
        var result = await healthCheck.CheckHealthAsync(context);

        // Assert
        result.Status.Should().Be(HealthStatus.Healthy);
        result.Description.Should().Contain("GB available");
        result.Data.Should().ContainKey("availableBytes");
        result.Data.Should().ContainKey("minimumRequiredBytes");
    }

    /// <summary>
    /// Tests that CheckHealthAsync returns Unhealthy when disk space is below minimum.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task CheckHealthAsync_WhenInsufficientSpace_ReturnsUnhealthy()
    {
        // Arrange - set an impossibly high minimum
        var settings = Options.Create(new HealthCheckSettings
        {
            DiskSpaceMinimumMB = int.MaxValue,
        });

        var healthCheck = new DiskSpaceHealthCheck(settings);
        var context = new HealthCheckContext
        {
            Registration = new HealthCheckRegistration("diskSpace", healthCheck, null, null),
        };

        // Act
        var result = await healthCheck.CheckHealthAsync(context);

        // Assert
        result.Status.Should().Be(HealthStatus.Unhealthy);
        result.Description.Should().Contain("Low disk space");
    }

    /// <summary>
    /// Tests that CheckHealthAsync returns data with correct keys.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task CheckHealthAsync_ReturnsDataWithCorrectKeys()
    {
        // Arrange
        var settings = Options.Create(new HealthCheckSettings
        {
            DiskSpaceMinimumMB = 100,
        });

        var healthCheck = new DiskSpaceHealthCheck(settings);
        var context = new HealthCheckContext
        {
            Registration = new HealthCheckRegistration("diskSpace", healthCheck, null, null),
        };

        // Act
        var result = await healthCheck.CheckHealthAsync(context);

        // Assert
        result.Data.Should().ContainKey("availableBytes");
        result.Data.Should().ContainKey("availableMB");
        result.Data.Should().ContainKey("minimumRequiredBytes");
        result.Data.Should().ContainKey("minimumRequiredMB");
        result.Data["minimumRequiredMB"].Should().Be(100);
    }
}
