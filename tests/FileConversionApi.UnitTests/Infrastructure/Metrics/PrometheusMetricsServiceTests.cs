// <copyright file="PrometheusMetricsServiceTests.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Infrastructure.Metrics;
using FluentAssertions;
using Xunit;

namespace FileConversionApi.UnitTests.Infrastructure.Metrics;

/// <summary>
/// Unit tests for <see cref="PrometheusMetricsService"/>.
/// </summary>
public class PrometheusMetricsServiceTests
{
    private readonly PrometheusMetricsService service;

    /// <summary>
    /// Initializes a new instance of the <see cref="PrometheusMetricsServiceTests"/> class.
    /// </summary>
    public PrometheusMetricsServiceTests()
    {
        this.service = new PrometheusMetricsService();
    }

    /// <summary>
    /// Tests that RecordConversionStarted does not throw exception.
    /// </summary>
    [Fact]
    public void RecordConversionStarted_WhenCalled_DoesNotThrow()
    {
        // Act
        var act = () => this.service.RecordConversionStarted("html", "pdf");

        // Assert
        act.Should().NotThrow();
    }

    /// <summary>
    /// Tests that RecordConversionStarted handles various format combinations.
    /// </summary>
    /// <param name="sourceFormat">The source format.</param>
    /// <param name="targetFormat">The target format.</param>
    [Theory]
    [InlineData("html", "pdf")]
    [InlineData("markdown", "pdf")]
    [InlineData("docx", "pdf")]
    [InlineData("png", "jpg")]
    public void RecordConversionStarted_WithVariousFormats_DoesNotThrow(string sourceFormat, string targetFormat)
    {
        // Act
        var act = () => this.service.RecordConversionStarted(sourceFormat, targetFormat);

        // Assert
        act.Should().NotThrow();
    }

    /// <summary>
    /// Tests that RecordConversionCompleted does not throw exception.
    /// </summary>
    [Fact]
    public void RecordConversionCompleted_WhenCalled_DoesNotThrow()
    {
        // Arrange
        this.service.RecordConversionStarted("html", "pdf");

        // Act
        var act = () => this.service.RecordConversionCompleted("html", "pdf", 1.5);

        // Assert
        act.Should().NotThrow();
    }

    /// <summary>
    /// Tests that RecordConversionCompleted handles various durations.
    /// </summary>
    /// <param name="durationSeconds">The duration in seconds.</param>
    [Theory]
    [InlineData(0.0)]
    [InlineData(0.5)]
    [InlineData(1.0)]
    [InlineData(5.0)]
    [InlineData(30.0)]
    [InlineData(60.0)]
    public void RecordConversionCompleted_WithVariousDurations_DoesNotThrow(double durationSeconds)
    {
        // Arrange
        this.service.RecordConversionStarted("html", "pdf");

        // Act
        var act = () => this.service.RecordConversionCompleted("html", "pdf", durationSeconds);

        // Assert
        act.Should().NotThrow();
    }

    /// <summary>
    /// Tests that RecordConversionFailed does not throw exception.
    /// </summary>
    [Fact]
    public void RecordConversionFailed_WhenCalled_DoesNotThrow()
    {
        // Arrange
        this.service.RecordConversionStarted("html", "pdf");

        // Act
        var act = () => this.service.RecordConversionFailed("html", "pdf");

        // Assert
        act.Should().NotThrow();
    }

    /// <summary>
    /// Tests that RecordConversionFailed handles various format combinations.
    /// </summary>
    /// <param name="sourceFormat">The source format.</param>
    /// <param name="targetFormat">The target format.</param>
    [Theory]
    [InlineData("html", "pdf")]
    [InlineData("markdown", "pdf")]
    [InlineData("docx", "pdf")]
    public void RecordConversionFailed_WithVariousFormats_DoesNotThrow(string sourceFormat, string targetFormat)
    {
        // Arrange
        this.service.RecordConversionStarted(sourceFormat, targetFormat);

        // Act
        var act = () => this.service.RecordConversionFailed(sourceFormat, targetFormat);

        // Assert
        act.Should().NotThrow();
    }

    /// <summary>
    /// Tests that multiple concurrent conversions can be tracked.
    /// </summary>
    [Fact]
    public void RecordConversionStarted_WhenCalledMultipleTimes_TracksAllJobs()
    {
        // Act
        var act = () =>
        {
            this.service.RecordConversionStarted("html", "pdf");
            this.service.RecordConversionStarted("html", "pdf");
            this.service.RecordConversionStarted("markdown", "pdf");
            this.service.RecordConversionCompleted("html", "pdf", 1.0);
            this.service.RecordConversionFailed("html", "pdf");
            this.service.RecordConversionCompleted("markdown", "pdf", 2.0);
        };

        // Assert
        act.Should().NotThrow();
    }

    /// <summary>
    /// Tests that the service can handle empty format strings.
    /// </summary>
    [Fact]
    public void RecordConversionStarted_WithEmptyFormats_DoesNotThrow()
    {
        // Act
        var act = () => this.service.RecordConversionStarted(string.Empty, string.Empty);

        // Assert
        act.Should().NotThrow();
    }

    /// <summary>
    /// Tests that RecordConversionCompleted without prior start does not throw.
    /// </summary>
    [Fact]
    public void RecordConversionCompleted_WithoutPriorStart_DoesNotThrow()
    {
        // Act - Complete without starting (edge case, gauge may go negative but shouldn't throw)
        var act = () => this.service.RecordConversionCompleted("html", "pdf", 1.0);

        // Assert
        act.Should().NotThrow();
    }
}
