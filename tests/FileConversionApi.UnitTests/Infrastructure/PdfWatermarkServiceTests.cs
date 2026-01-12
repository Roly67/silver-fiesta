// <copyright file="PdfWatermarkServiceTests.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Application.DTOs;
using FileConversionApi.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace FileConversionApi.UnitTests.Infrastructure;

/// <summary>
/// Unit tests for the <see cref="PdfWatermarkService"/> class.
/// </summary>
public class PdfWatermarkServiceTests
{
    private readonly Mock<ILogger<PdfWatermarkService>> loggerMock;
    private readonly PdfWatermarkService service;

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfWatermarkServiceTests"/> class.
    /// </summary>
    public PdfWatermarkServiceTests()
    {
        this.loggerMock = new Mock<ILogger<PdfWatermarkService>>();
        this.service = new PdfWatermarkService(this.loggerMock.Object);
    }

    /// <summary>
    /// Tests that constructor throws ArgumentNullException when logger is null.
    /// </summary>
    [Fact]
    public void Constructor_WhenLoggerIsNull_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new PdfWatermarkService(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    /// <summary>
    /// Tests that ApplyWatermarkAsync returns error when text is null.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task ApplyWatermarkAsync_WhenTextIsNull_ReturnsError()
    {
        // Arrange
        var pdfData = CreateMinimalPdf();
        var options = new WatermarkOptions { Text = null };

        // Act
        var result = await this.service.ApplyWatermarkAsync(pdfData, options);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Watermark.EmptyText");
    }

    /// <summary>
    /// Tests that ApplyWatermarkAsync returns error when text is empty.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task ApplyWatermarkAsync_WhenTextIsEmpty_ReturnsError()
    {
        // Arrange
        var pdfData = CreateMinimalPdf();
        var options = new WatermarkOptions { Text = string.Empty };

        // Act
        var result = await this.service.ApplyWatermarkAsync(pdfData, options);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Watermark.EmptyText");
    }

    /// <summary>
    /// Tests that ApplyWatermarkAsync returns error when text is whitespace.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task ApplyWatermarkAsync_WhenTextIsWhitespace_ReturnsError()
    {
        // Arrange
        var pdfData = CreateMinimalPdf();
        var options = new WatermarkOptions { Text = "   " };

        // Act
        var result = await this.service.ApplyWatermarkAsync(pdfData, options);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Watermark.EmptyText");
    }

    /// <summary>
    /// Tests that ApplyWatermarkAsync returns error for invalid PDF data.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task ApplyWatermarkAsync_WhenPdfDataIsInvalid_ReturnsError()
    {
        // Arrange
        var invalidPdfData = "not a pdf"u8.ToArray();
        var options = new WatermarkOptions { Text = "CONFIDENTIAL" };

        // Act
        var result = await this.service.ApplyWatermarkAsync(invalidPdfData, options);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Watermark.Failed");
    }

    /// <summary>
    /// Tests that ApplyWatermarkAsync succeeds with valid PDF and watermark text.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task ApplyWatermarkAsync_WithValidPdfAndText_ReturnsWatermarkedPdf()
    {
        // Arrange
        var pdfData = CreateMinimalPdf();
        var options = new WatermarkOptions
        {
            Text = "CONFIDENTIAL",
            FontSize = 48,
            Color = "#FF0000",
            Opacity = 0.5,
            Rotation = -45,
            Position = WatermarkPosition.Center,
            AllPages = true,
        };

        // Act
        var result = await this.service.ApplyWatermarkAsync(pdfData, options);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        result.Value.Length.Should().BeGreaterThan(pdfData.Length);
    }

    /// <summary>
    /// Tests that ApplyWatermarkAsync applies watermark to specific pages only.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task ApplyWatermarkAsync_WithSpecificPages_ReturnsWatermarkedPdf()
    {
        // Arrange
        var pdfData = CreateMinimalPdf();
        var options = new WatermarkOptions
        {
            Text = "DRAFT",
            AllPages = false,
            PageNumbers = [1],
        };

        // Act
        var result = await this.service.ApplyWatermarkAsync(pdfData, options);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
    }

    /// <summary>
    /// Tests that ApplyWatermarkAsync works with tile position.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task ApplyWatermarkAsync_WithTilePosition_ReturnsWatermarkedPdf()
    {
        // Arrange
        var pdfData = CreateMinimalPdf();
        var options = new WatermarkOptions
        {
            Text = "SAMPLE",
            Position = WatermarkPosition.Tile,
            Opacity = 0.2,
        };

        // Act
        var result = await this.service.ApplyWatermarkAsync(pdfData, options);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
    }

    /// <summary>
    /// Tests that ApplyWatermarkAsync handles invalid hex color gracefully.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task ApplyWatermarkAsync_WithInvalidColor_UsesDefaultColor()
    {
        // Arrange
        var pdfData = CreateMinimalPdf();
        var options = new WatermarkOptions
        {
            Text = "TEST",
            Color = "invalid-color",
        };

        // Act
        var result = await this.service.ApplyWatermarkAsync(pdfData, options);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
    }

    /// <summary>
    /// Tests that ApplyWatermarkAsync works with all position types.
    /// </summary>
    /// <param name="position">The watermark position.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Theory]
    [InlineData(WatermarkPosition.Center)]
    [InlineData(WatermarkPosition.TopLeft)]
    [InlineData(WatermarkPosition.TopCenter)]
    [InlineData(WatermarkPosition.TopRight)]
    [InlineData(WatermarkPosition.BottomLeft)]
    [InlineData(WatermarkPosition.BottomCenter)]
    [InlineData(WatermarkPosition.BottomRight)]
    public async Task ApplyWatermarkAsync_WithDifferentPositions_ReturnsWatermarkedPdf(WatermarkPosition position)
    {
        // Arrange
        var pdfData = CreateMinimalPdf();
        var options = new WatermarkOptions
        {
            Text = "WATERMARK",
            Position = position,
        };

        // Act
        var result = await this.service.ApplyWatermarkAsync(pdfData, options);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
    }

    private static byte[] CreateMinimalPdf()
    {
        using var document = new PdfSharpCore.Pdf.PdfDocument();
        var page = document.AddPage();

        using var gfx = PdfSharpCore.Drawing.XGraphics.FromPdfPage(page);
        var font = new PdfSharpCore.Drawing.XFont("Helvetica", 12, PdfSharpCore.Drawing.XFontStyle.Regular);
        gfx.DrawString("Test PDF", font, PdfSharpCore.Drawing.XBrushes.Black, new PdfSharpCore.Drawing.XPoint(50, 50));

        using var stream = new MemoryStream();
        document.Save(stream, false);
        return stream.ToArray();
    }
}
