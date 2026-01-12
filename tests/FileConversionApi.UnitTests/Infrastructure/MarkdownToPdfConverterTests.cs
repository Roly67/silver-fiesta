// <copyright file="MarkdownToPdfConverterTests.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Application.Interfaces;
using FileConversionApi.Infrastructure.Converters;
using FileConversionApi.Infrastructure.Options;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace FileConversionApi.UnitTests.Infrastructure;

/// <summary>
/// Unit tests for the <see cref="MarkdownToPdfConverter"/> class.
/// </summary>
public class MarkdownToPdfConverterTests
{
    private readonly Mock<ILogger<HtmlToPdfConverter>> htmlConverterLoggerMock;
    private readonly Mock<ILogger<MarkdownToPdfConverter>> loggerMock;
    private readonly Mock<IPdfWatermarkService> watermarkServiceMock;
    private readonly HtmlToPdfConverter htmlToPdfConverter;

    /// <summary>
    /// Initializes a new instance of the <see cref="MarkdownToPdfConverterTests"/> class.
    /// </summary>
    public MarkdownToPdfConverterTests()
    {
        this.htmlConverterLoggerMock = new Mock<ILogger<HtmlToPdfConverter>>();
        this.loggerMock = new Mock<ILogger<MarkdownToPdfConverter>>();
        this.watermarkServiceMock = new Mock<IPdfWatermarkService>();

        var settings = new PuppeteerSettings
        {
            ExecutablePath = null,
            Timeout = 30000,
        };
        var options = Options.Create(settings);

        this.htmlToPdfConverter = new HtmlToPdfConverter(options, this.watermarkServiceMock.Object, this.htmlConverterLoggerMock.Object);
    }

    /// <summary>
    /// Tests that constructor throws ArgumentNullException when htmlToPdfConverter is null.
    /// </summary>
    [Fact]
    public void Constructor_WhenHtmlToPdfConverterIsNull_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new MarkdownToPdfConverter(null!, this.loggerMock.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("htmlToPdfConverter");
    }

    /// <summary>
    /// Tests that constructor throws ArgumentNullException when logger is null.
    /// </summary>
    [Fact]
    public void Constructor_WhenLoggerIsNull_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new MarkdownToPdfConverter(this.htmlToPdfConverter, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    /// <summary>
    /// Tests that SourceFormat returns markdown.
    /// </summary>
    [Fact]
    public void SourceFormat_WhenCalled_ReturnsMarkdown()
    {
        // Arrange
        var converter = new MarkdownToPdfConverter(this.htmlToPdfConverter, this.loggerMock.Object);

        // Act
        var result = converter.SourceFormat;

        // Assert
        result.Should().Be("markdown");
    }

    /// <summary>
    /// Tests that TargetFormat returns pdf.
    /// </summary>
    [Fact]
    public void TargetFormat_WhenCalled_ReturnsPdf()
    {
        // Arrange
        var converter = new MarkdownToPdfConverter(this.htmlToPdfConverter, this.loggerMock.Object);

        // Act
        var result = converter.TargetFormat;

        // Assert
        result.Should().Be("pdf");
    }

    /// <summary>
    /// Tests that constructor initializes markdown pipeline.
    /// </summary>
    [Fact]
    public void Constructor_WhenCalled_InitializesMarkdownPipeline()
    {
        // Act
        var act = () => new MarkdownToPdfConverter(this.htmlToPdfConverter, this.loggerMock.Object);

        // Assert
        act.Should().NotThrow();
    }
}
