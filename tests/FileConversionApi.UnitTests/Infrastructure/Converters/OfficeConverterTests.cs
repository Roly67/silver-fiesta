// <copyright file="OfficeConverterTests.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Application.DTOs;
using FileConversionApi.Application.Interfaces;
using FileConversionApi.Domain.Primitives;
using FileConversionApi.Infrastructure.Converters;

using FluentAssertions;

using Microsoft.Extensions.Logging;

using Moq;

using Xunit;

namespace FileConversionApi.UnitTests.Infrastructure.Converters;

/// <summary>
/// Unit tests for office document converters.
/// </summary>
public class OfficeConverterTests
{
    private readonly Mock<ILibreOfficeService> libreOfficeServiceMock;
    private readonly Mock<IPdfWatermarkService> watermarkServiceMock;
    private readonly Mock<IPdfEncryptionService> encryptionServiceMock;
    private readonly Mock<ILogger<DocxToPdfConverter>> docxLoggerMock;
    private readonly Mock<ILogger<XlsxToPdfConverter>> xlsxLoggerMock;

    /// <summary>
    /// Initializes a new instance of the <see cref="OfficeConverterTests"/> class.
    /// </summary>
    public OfficeConverterTests()
    {
        this.libreOfficeServiceMock = new Mock<ILibreOfficeService>();
        this.watermarkServiceMock = new Mock<IPdfWatermarkService>();
        this.encryptionServiceMock = new Mock<IPdfEncryptionService>();
        this.docxLoggerMock = new Mock<ILogger<DocxToPdfConverter>>();
        this.xlsxLoggerMock = new Mock<ILogger<XlsxToPdfConverter>>();
    }

    /// <summary>
    /// Tests that DocxToPdfConverter has correct format properties.
    /// </summary>
    [Fact]
    public void DocxToPdfConverter_HasCorrectFormats()
    {
        // Arrange
        var converter = new DocxToPdfConverter(
            this.libreOfficeServiceMock.Object,
            this.watermarkServiceMock.Object,
            this.encryptionServiceMock.Object,
            this.docxLoggerMock.Object);

        // Assert
        converter.SourceFormat.Should().Be("docx");
        converter.TargetFormat.Should().Be("pdf");
    }

    /// <summary>
    /// Tests that XlsxToPdfConverter has correct format properties.
    /// </summary>
    [Fact]
    public void XlsxToPdfConverter_HasCorrectFormats()
    {
        // Arrange
        var converter = new XlsxToPdfConverter(
            this.libreOfficeServiceMock.Object,
            this.watermarkServiceMock.Object,
            this.encryptionServiceMock.Object,
            this.xlsxLoggerMock.Object);

        // Assert
        converter.SourceFormat.Should().Be("xlsx");
        converter.TargetFormat.Should().Be("pdf");
    }

    /// <summary>
    /// Tests that DocxToPdfConverter converts successfully with LibreOffice.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task DocxToPdfConverter_ConvertAsync_ConvertsSuccessfully()
    {
        // Arrange
        var converter = new DocxToPdfConverter(
            this.libreOfficeServiceMock.Object,
            this.watermarkServiceMock.Object,
            this.encryptionServiceMock.Object,
            this.docxLoggerMock.Object);

        var inputData = new byte[] { 0x50, 0x4B, 0x03, 0x04 }; // DOCX is a ZIP file
        var pdfData = new byte[] { 0x25, 0x50, 0x44, 0x46 }; // %PDF

        this.libreOfficeServiceMock
            .Setup(x => x.ConvertToPdfAsync(It.IsAny<byte[]>(), "docx", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<byte[]>.Success(pdfData));

        using var inputStream = new MemoryStream(inputData);
        var options = new ConversionOptions();

        // Act
        var result = await converter.ConvertAsync(inputStream, options, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(pdfData);
    }

    /// <summary>
    /// Tests that XlsxToPdfConverter converts successfully with LibreOffice.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task XlsxToPdfConverter_ConvertAsync_ConvertsSuccessfully()
    {
        // Arrange
        var converter = new XlsxToPdfConverter(
            this.libreOfficeServiceMock.Object,
            this.watermarkServiceMock.Object,
            this.encryptionServiceMock.Object,
            this.xlsxLoggerMock.Object);

        var inputData = new byte[] { 0x50, 0x4B, 0x03, 0x04 }; // XLSX is a ZIP file
        var pdfData = new byte[] { 0x25, 0x50, 0x44, 0x46 }; // %PDF

        this.libreOfficeServiceMock
            .Setup(x => x.ConvertToPdfAsync(It.IsAny<byte[]>(), "xlsx", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<byte[]>.Success(pdfData));

        using var inputStream = new MemoryStream(inputData);
        var options = new ConversionOptions();

        // Act
        var result = await converter.ConvertAsync(inputStream, options, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(pdfData);
    }

    /// <summary>
    /// Tests that converter returns failure when LibreOffice fails.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task DocxToPdfConverter_ConvertAsync_ReturnsFailure_WhenLibreOfficeFails()
    {
        // Arrange
        var converter = new DocxToPdfConverter(
            this.libreOfficeServiceMock.Object,
            this.watermarkServiceMock.Object,
            this.encryptionServiceMock.Object,
            this.docxLoggerMock.Object);

        var inputData = new byte[] { 0x50, 0x4B, 0x03, 0x04 };
        var error = new Error("LibreOffice.ConversionFailed", "Conversion timed out");

        this.libreOfficeServiceMock
            .Setup(x => x.ConvertToPdfAsync(It.IsAny<byte[]>(), "docx", It.IsAny<CancellationToken>()))
            .ReturnsAsync(error);

        using var inputStream = new MemoryStream(inputData);
        var options = new ConversionOptions();

        // Act
        var result = await converter.ConvertAsync(inputStream, options, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("LibreOffice.ConversionFailed");
    }

    /// <summary>
    /// Tests that converter applies watermark when specified.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task DocxToPdfConverter_ConvertAsync_AppliesWatermark_WhenSpecified()
    {
        // Arrange
        var converter = new DocxToPdfConverter(
            this.libreOfficeServiceMock.Object,
            this.watermarkServiceMock.Object,
            this.encryptionServiceMock.Object,
            this.docxLoggerMock.Object);

        var inputData = new byte[] { 0x50, 0x4B, 0x03, 0x04 };
        var pdfData = new byte[] { 0x25, 0x50, 0x44, 0x46 };
        var watermarkedPdfData = new byte[] { 0x25, 0x50, 0x44, 0x46, 0x01 };

        this.libreOfficeServiceMock
            .Setup(x => x.ConvertToPdfAsync(It.IsAny<byte[]>(), "docx", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<byte[]>.Success(pdfData));

        this.watermarkServiceMock
            .Setup(x => x.ApplyWatermarkAsync(pdfData, It.IsAny<WatermarkOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<byte[]>.Success(watermarkedPdfData));

        using var inputStream = new MemoryStream(inputData);
        var options = new ConversionOptions
        {
            Watermark = new WatermarkOptions { Text = "CONFIDENTIAL" },
        };

        // Act
        var result = await converter.ConvertAsync(inputStream, options, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(watermarkedPdfData);
        this.watermarkServiceMock.Verify(
            x => x.ApplyWatermarkAsync(pdfData, It.IsAny<WatermarkOptions>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that converter applies encryption when specified.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task DocxToPdfConverter_ConvertAsync_AppliesEncryption_WhenSpecified()
    {
        // Arrange
        var converter = new DocxToPdfConverter(
            this.libreOfficeServiceMock.Object,
            this.watermarkServiceMock.Object,
            this.encryptionServiceMock.Object,
            this.docxLoggerMock.Object);

        var inputData = new byte[] { 0x50, 0x4B, 0x03, 0x04 };
        var pdfData = new byte[] { 0x25, 0x50, 0x44, 0x46 };
        var encryptedPdfData = new byte[] { 0x25, 0x50, 0x44, 0x46, 0x02 };

        this.libreOfficeServiceMock
            .Setup(x => x.ConvertToPdfAsync(It.IsAny<byte[]>(), "docx", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<byte[]>.Success(pdfData));

        this.encryptionServiceMock
            .Setup(x => x.EncryptAsync(pdfData, It.IsAny<PasswordProtectionOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<byte[]>.Success(encryptedPdfData));

        using var inputStream = new MemoryStream(inputData);
        var options = new ConversionOptions
        {
            PasswordProtection = new PasswordProtectionOptions { UserPassword = "secret" },
        };

        // Act
        var result = await converter.ConvertAsync(inputStream, options, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(encryptedPdfData);
        this.encryptionServiceMock.Verify(
            x => x.EncryptAsync(pdfData, It.IsAny<PasswordProtectionOptions>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
