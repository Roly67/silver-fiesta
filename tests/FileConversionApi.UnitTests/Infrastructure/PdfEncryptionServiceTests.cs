// <copyright file="PdfEncryptionServiceTests.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Application.DTOs;
using FileConversionApi.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace FileConversionApi.UnitTests.Infrastructure;

/// <summary>
/// Unit tests for the <see cref="PdfEncryptionService"/> class.
/// </summary>
public class PdfEncryptionServiceTests
{
    private readonly Mock<ILogger<PdfEncryptionService>> loggerMock;
    private readonly PdfEncryptionService service;

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfEncryptionServiceTests"/> class.
    /// </summary>
    public PdfEncryptionServiceTests()
    {
        this.loggerMock = new Mock<ILogger<PdfEncryptionService>>();
        this.service = new PdfEncryptionService(this.loggerMock.Object);
    }

    /// <summary>
    /// Tests that constructor throws ArgumentNullException when logger is null.
    /// </summary>
    [Fact]
    public void Constructor_WhenLoggerIsNull_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new PdfEncryptionService(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    /// <summary>
    /// Tests that EncryptAsync returns error when user password is null.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task EncryptAsync_WhenUserPasswordIsNull_ReturnsError()
    {
        // Arrange
        var pdfData = CreateMinimalPdf();
        var options = new PasswordProtectionOptions { UserPassword = null };

        // Act
        var result = await this.service.EncryptAsync(pdfData, options);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Encryption.EmptyPassword");
    }

    /// <summary>
    /// Tests that EncryptAsync returns error when user password is empty.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task EncryptAsync_WhenUserPasswordIsEmpty_ReturnsError()
    {
        // Arrange
        var pdfData = CreateMinimalPdf();
        var options = new PasswordProtectionOptions { UserPassword = string.Empty };

        // Act
        var result = await this.service.EncryptAsync(pdfData, options);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Encryption.EmptyPassword");
    }

    /// <summary>
    /// Tests that EncryptAsync returns error when user password is whitespace.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task EncryptAsync_WhenUserPasswordIsWhitespace_ReturnsError()
    {
        // Arrange
        var pdfData = CreateMinimalPdf();
        var options = new PasswordProtectionOptions { UserPassword = "   " };

        // Act
        var result = await this.service.EncryptAsync(pdfData, options);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Encryption.EmptyPassword");
    }

    /// <summary>
    /// Tests that EncryptAsync returns error for invalid PDF data.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task EncryptAsync_WhenPdfDataIsInvalid_ReturnsError()
    {
        // Arrange
        var invalidPdfData = "not a pdf"u8.ToArray();
        var options = new PasswordProtectionOptions { UserPassword = "secret123" };

        // Act
        var result = await this.service.EncryptAsync(invalidPdfData, options);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Encryption.Failed");
    }

    /// <summary>
    /// Tests that EncryptAsync succeeds with valid PDF and password.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task EncryptAsync_WithValidPdfAndPassword_ReturnsEncryptedPdf()
    {
        // Arrange
        var pdfData = CreateMinimalPdf();
        var options = new PasswordProtectionOptions
        {
            UserPassword = "secret123",
        };

        // Act
        var result = await this.service.EncryptAsync(pdfData, options);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
    }

    /// <summary>
    /// Tests that EncryptAsync succeeds with separate owner password.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task EncryptAsync_WithOwnerPassword_ReturnsEncryptedPdf()
    {
        // Arrange
        var pdfData = CreateMinimalPdf();
        var options = new PasswordProtectionOptions
        {
            UserPassword = "userpass",
            OwnerPassword = "ownerpass",
        };

        // Act
        var result = await this.service.EncryptAsync(pdfData, options);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
    }

    /// <summary>
    /// Tests that EncryptAsync applies printing permission.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task EncryptAsync_WithPrintingDisabled_ReturnsEncryptedPdf()
    {
        // Arrange
        var pdfData = CreateMinimalPdf();
        var options = new PasswordProtectionOptions
        {
            UserPassword = "secret123",
            AllowPrinting = false,
        };

        // Act
        var result = await this.service.EncryptAsync(pdfData, options);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
    }

    /// <summary>
    /// Tests that EncryptAsync applies copying permission.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task EncryptAsync_WithCopyingDisabled_ReturnsEncryptedPdf()
    {
        // Arrange
        var pdfData = CreateMinimalPdf();
        var options = new PasswordProtectionOptions
        {
            UserPassword = "secret123",
            AllowCopyingContent = false,
        };

        // Act
        var result = await this.service.EncryptAsync(pdfData, options);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
    }

    /// <summary>
    /// Tests that EncryptAsync applies modifying permission.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task EncryptAsync_WithModifyingEnabled_ReturnsEncryptedPdf()
    {
        // Arrange
        var pdfData = CreateMinimalPdf();
        var options = new PasswordProtectionOptions
        {
            UserPassword = "secret123",
            AllowModifying = true,
        };

        // Act
        var result = await this.service.EncryptAsync(pdfData, options);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
    }

    /// <summary>
    /// Tests that EncryptAsync applies annotations permission.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task EncryptAsync_WithAnnotationsEnabled_ReturnsEncryptedPdf()
    {
        // Arrange
        var pdfData = CreateMinimalPdf();
        var options = new PasswordProtectionOptions
        {
            UserPassword = "secret123",
            AllowAnnotations = true,
        };

        // Act
        var result = await this.service.EncryptAsync(pdfData, options);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
    }

    /// <summary>
    /// Tests that EncryptAsync works with all permissions restricted.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task EncryptAsync_WithAllPermissionsRestricted_ReturnsEncryptedPdf()
    {
        // Arrange
        var pdfData = CreateMinimalPdf();
        var options = new PasswordProtectionOptions
        {
            UserPassword = "secret123",
            OwnerPassword = "admin123",
            AllowPrinting = false,
            AllowCopyingContent = false,
            AllowModifying = false,
            AllowAnnotations = false,
        };

        // Act
        var result = await this.service.EncryptAsync(pdfData, options);

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
