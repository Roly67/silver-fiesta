// <copyright file="PdfManipulationServiceTests.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Application.DTOs;
using FileConversionApi.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace FileConversionApi.UnitTests.Infrastructure;

/// <summary>
/// Unit tests for the <see cref="PdfManipulationService"/> class.
/// </summary>
public class PdfManipulationServiceTests
{
    private readonly Mock<ILogger<PdfManipulationService>> loggerMock;
    private readonly PdfManipulationService service;

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfManipulationServiceTests"/> class.
    /// </summary>
    public PdfManipulationServiceTests()
    {
        this.loggerMock = new Mock<ILogger<PdfManipulationService>>();
        this.service = new PdfManipulationService(this.loggerMock.Object);
    }

    /// <summary>
    /// Tests that constructor throws ArgumentNullException when logger is null.
    /// </summary>
    [Fact]
    public void Constructor_WhenLoggerIsNull_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new PdfManipulationService(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    /// <summary>
    /// Tests that MergeAsync returns error when no documents provided.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task MergeAsync_WhenNoDocumentsProvided_ReturnsError()
    {
        // Arrange
        var documents = Array.Empty<byte[]>();

        // Act
        var result = await this.service.MergeAsync(documents);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Merge.NoDocuments");
    }

    /// <summary>
    /// Tests that MergeAsync returns error when only one document provided.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task MergeAsync_WhenSingleDocumentProvided_ReturnsError()
    {
        // Arrange
        var pdfData = CreateMinimalPdf();
        var documents = new[] { pdfData };

        // Act
        var result = await this.service.MergeAsync(documents);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Merge.SingleDocument");
    }

    /// <summary>
    /// Tests that MergeAsync returns error when invalid PDF data provided.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task MergeAsync_WhenInvalidPdfDataProvided_ReturnsError()
    {
        // Arrange
        var invalidPdf = "not a pdf"u8.ToArray();
        var documents = new[] { invalidPdf, invalidPdf };

        // Act
        var result = await this.service.MergeAsync(documents);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Merge.Failed");
    }

    /// <summary>
    /// Tests that MergeAsync succeeds with valid PDFs.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task MergeAsync_WithValidPdfs_ReturnsMergedPdf()
    {
        // Arrange
        var pdf1 = CreateMinimalPdf();
        var pdf2 = CreateMinimalPdf();
        var documents = new[] { pdf1, pdf2 };

        // Act
        var result = await this.service.MergeAsync(documents);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
    }

    /// <summary>
    /// Tests that MergeAsync succeeds with three PDFs.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task MergeAsync_WithThreePdfs_ReturnsMergedPdf()
    {
        // Arrange
        var pdf1 = CreateMinimalPdf();
        var pdf2 = CreateMinimalPdf();
        var pdf3 = CreateMinimalPdf();
        var documents = new[] { pdf1, pdf2, pdf3 };

        // Act
        var result = await this.service.MergeAsync(documents);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
    }

    /// <summary>
    /// Tests that SplitAsync returns error when PDF is empty.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task SplitAsync_WhenPdfIsInvalid_ReturnsError()
    {
        // Arrange
        var invalidPdf = "not a pdf"u8.ToArray();
        var options = new PdfSplitOptions { SplitIntoSinglePages = true };

        // Act
        var result = await this.service.SplitAsync(invalidPdf, options);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Split.Failed");
    }

    /// <summary>
    /// Tests that SplitAsync splits into single pages.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task SplitAsync_WithSplitIntoSinglePages_ReturnsSinglePagePdfs()
    {
        // Arrange
        var pdfData = CreateMultiPagePdf(3);
        var options = new PdfSplitOptions { SplitIntoSinglePages = true };

        // Act
        var result = await this.service.SplitAsync(pdfData, options);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(3);
        result.Value.Keys.Should().Contain("page_1.pdf");
        result.Value.Keys.Should().Contain("page_2.pdf");
        result.Value.Keys.Should().Contain("page_3.pdf");
    }

    /// <summary>
    /// Tests that SplitAsync splits by page range.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task SplitAsync_WithPageRange_ReturnsRangePdf()
    {
        // Arrange
        var pdfData = CreateMultiPagePdf(5);
        var options = new PdfSplitOptions { PageRanges = new[] { "1-3" } };

        // Act
        var result = await this.service.SplitAsync(pdfData, options);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value.Keys.Should().Contain("pages_1_3.pdf");
    }

    /// <summary>
    /// Tests that SplitAsync handles multiple page ranges.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task SplitAsync_WithMultiplePageRanges_ReturnsMultiplePdfs()
    {
        // Arrange
        var pdfData = CreateMultiPagePdf(5);
        var options = new PdfSplitOptions { PageRanges = new[] { "1-2", "4-5" } };

        // Act
        var result = await this.service.SplitAsync(pdfData, options);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Keys.Should().Contain("pages_1_2.pdf");
        result.Value.Keys.Should().Contain("pages_4_5.pdf");
    }

    /// <summary>
    /// Tests that SplitAsync handles single page range.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task SplitAsync_WithSinglePageRange_ReturnsSinglePagePdf()
    {
        // Arrange
        var pdfData = CreateMultiPagePdf(5);
        var options = new PdfSplitOptions { PageRanges = new[] { "3" } };

        // Act
        var result = await this.service.SplitAsync(pdfData, options);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value.Keys.Should().Contain("pages_3.pdf");
    }

    /// <summary>
    /// Tests that SplitAsync returns error for invalid page range format.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task SplitAsync_WithInvalidPageRangeFormat_ReturnsError()
    {
        // Arrange
        var pdfData = CreateMultiPagePdf(5);
        var options = new PdfSplitOptions { PageRanges = new[] { "abc" } };

        // Act
        var result = await this.service.SplitAsync(pdfData, options);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Split.InvalidPageRange");
    }

    /// <summary>
    /// Tests that SplitAsync returns error when page range exceeds document.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task SplitAsync_WhenPageRangeExceedsDocument_ReturnsError()
    {
        // Arrange
        var pdfData = CreateMultiPagePdf(3);
        var options = new PdfSplitOptions { PageRanges = new[] { "1-5" } };

        // Act
        var result = await this.service.SplitAsync(pdfData, options);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Split.PageOutOfRange");
    }

    /// <summary>
    /// Tests that SplitAsync returns error when start page greater than end page.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task SplitAsync_WhenStartPageGreaterThanEndPage_ReturnsError()
    {
        // Arrange
        var pdfData = CreateMultiPagePdf(5);
        var options = new PdfSplitOptions { PageRanges = new[] { "5-2" } };

        // Act
        var result = await this.service.SplitAsync(pdfData, options);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Split.InvalidPageRange");
    }

    /// <summary>
    /// Tests that SplitAsync defaults to single pages when no options specified.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task SplitAsync_WithNoOptions_DefaultsToSinglePages()
    {
        // Arrange
        var pdfData = CreateMultiPagePdf(2);
        var options = new PdfSplitOptions();

        // Act
        var result = await this.service.SplitAsync(pdfData, options);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
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

    private static byte[] CreateMultiPagePdf(int pageCount)
    {
        using var document = new PdfSharpCore.Pdf.PdfDocument();

        for (var i = 0; i < pageCount; i++)
        {
            var page = document.AddPage();
            using var gfx = PdfSharpCore.Drawing.XGraphics.FromPdfPage(page);
            var font = new PdfSharpCore.Drawing.XFont("Helvetica", 12, PdfSharpCore.Drawing.XFontStyle.Regular);
            gfx.DrawString($"Page {i + 1}", font, PdfSharpCore.Drawing.XBrushes.Black, new PdfSharpCore.Drawing.XPoint(50, 50));
        }

        using var stream = new MemoryStream();
        document.Save(stream, false);
        return stream.ToArray();
    }
}
