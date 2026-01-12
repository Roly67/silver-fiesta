// <copyright file="HtmlToPdfConverterTests.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Infrastructure.Converters;
using FileConversionApi.Infrastructure.Options;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace FileConversionApi.UnitTests.Infrastructure;

/// <summary>
/// Unit tests for the <see cref="HtmlToPdfConverter"/> class.
/// </summary>
public class HtmlToPdfConverterTests
{
    private readonly Mock<ILogger<HtmlToPdfConverter>> loggerMock;
    private readonly PuppeteerSettings settings;

    /// <summary>
    /// Initializes a new instance of the <see cref="HtmlToPdfConverterTests"/> class.
    /// </summary>
    public HtmlToPdfConverterTests()
    {
        this.loggerMock = new Mock<ILogger<HtmlToPdfConverter>>();
        this.settings = new PuppeteerSettings
        {
            ExecutablePath = null,
            Timeout = 30000,
        };
    }

    /// <summary>
    /// Tests that constructor throws ArgumentNullException when settings is null.
    /// </summary>
    [Fact]
    public void Constructor_WhenSettingsIsNull_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new HtmlToPdfConverter(null!, this.loggerMock.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("settings");
    }

    /// <summary>
    /// Tests that constructor throws ArgumentNullException when logger is null.
    /// </summary>
    [Fact]
    public void Constructor_WhenLoggerIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        var options = Options.Create(this.settings);

        // Act
        var act = () => new HtmlToPdfConverter(options, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    /// <summary>
    /// Tests that SourceFormat returns html.
    /// </summary>
    [Fact]
    public void SourceFormat_WhenCalled_ReturnsHtml()
    {
        // Arrange
        var options = Options.Create(this.settings);
        var converter = new HtmlToPdfConverter(options, this.loggerMock.Object);

        // Act
        var result = converter.SourceFormat;

        // Assert
        result.Should().Be("html");
    }

    /// <summary>
    /// Tests that TargetFormat returns pdf.
    /// </summary>
    [Fact]
    public void TargetFormat_WhenCalled_ReturnsPdf()
    {
        // Arrange
        var options = Options.Create(this.settings);
        var converter = new HtmlToPdfConverter(options, this.loggerMock.Object);

        // Act
        var result = converter.TargetFormat;

        // Assert
        result.Should().Be("pdf");
    }

    /// <summary>
    /// Tests that DisposeAsync completes without error when browser is null.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task DisposeAsync_WhenBrowserIsNull_CompletesWithoutError()
    {
        // Arrange
        var options = Options.Create(this.settings);
        var converter = new HtmlToPdfConverter(options, this.loggerMock.Object);

        // Act
        var act = async () => await converter.DisposeAsync();

        // Assert
        await act.Should().NotThrowAsync();
    }

    /// <summary>
    /// Tests that DisposeAsync can be called multiple times.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task DisposeAsync_WhenCalledMultipleTimes_DoesNotThrow()
    {
        // Arrange
        var options = Options.Create(this.settings);
        var converter = new HtmlToPdfConverter(options, this.loggerMock.Object);

        // Act
        var act = async () =>
        {
            await converter.DisposeAsync();
            await converter.DisposeAsync();
            await converter.DisposeAsync();
        };

        // Assert
        await act.Should().NotThrowAsync();
    }

    /// <summary>
    /// Tests that constructor initializes with custom executable path.
    /// </summary>
    [Fact]
    public void Constructor_WithCustomExecutablePath_DoesNotThrow()
    {
        // Arrange
        var customSettings = new PuppeteerSettings
        {
            ExecutablePath = "/usr/bin/chromium",
            Timeout = 60000,
        };
        var options = Options.Create(customSettings);

        // Act
        var act = () => new HtmlToPdfConverter(options, this.loggerMock.Object);

        // Assert
        act.Should().NotThrow();
    }
}
