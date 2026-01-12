// <copyright file="ChromiumHealthCheckTests.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Infrastructure.HealthChecks;
using FileConversionApi.Infrastructure.Options;

using FluentAssertions;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Moq;

namespace FileConversionApi.UnitTests.Infrastructure.HealthChecks;

/// <summary>
/// Unit tests for <see cref="ChromiumHealthCheck"/>.
/// </summary>
public class ChromiumHealthCheckTests
{
    private readonly Mock<ILogger<ChromiumHealthCheck>> loggerMock;
    private readonly IOptions<PuppeteerSettings> puppeteerSettings;
    private readonly IOptions<HealthCheckSettings> healthCheckSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChromiumHealthCheckTests"/> class.
    /// </summary>
    public ChromiumHealthCheckTests()
    {
        this.loggerMock = new Mock<ILogger<ChromiumHealthCheck>>();
        this.puppeteerSettings = Options.Create(new PuppeteerSettings());
        this.healthCheckSettings = Options.Create(new HealthCheckSettings { ChromiumTimeoutSeconds = 30 });
    }

    /// <summary>
    /// Tests that constructor throws ArgumentNullException when settings is null.
    /// </summary>
    [Fact]
    public void Constructor_WhenSettingsIsNull_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new ChromiumHealthCheck(
            null!,
            this.healthCheckSettings,
            this.loggerMock.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("settings");
    }

    /// <summary>
    /// Tests that constructor throws ArgumentNullException when healthCheckSettings is null.
    /// </summary>
    [Fact]
    public void Constructor_WhenHealthCheckSettingsIsNull_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new ChromiumHealthCheck(
            this.puppeteerSettings,
            null!,
            this.loggerMock.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("healthCheckSettings");
    }

    /// <summary>
    /// Tests that constructor throws ArgumentNullException when logger is null.
    /// </summary>
    [Fact]
    public void Constructor_WhenLoggerIsNull_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new ChromiumHealthCheck(
            this.puppeteerSettings,
            this.healthCheckSettings,
            null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    /// <summary>
    /// Tests that constructor succeeds with valid parameters.
    /// </summary>
    [Fact]
    public void Constructor_WithValidParameters_Succeeds()
    {
        // Act
        var healthCheck = new ChromiumHealthCheck(
            this.puppeteerSettings,
            this.healthCheckSettings,
            this.loggerMock.Object);

        // Assert
        healthCheck.Should().NotBeNull();
    }
}
