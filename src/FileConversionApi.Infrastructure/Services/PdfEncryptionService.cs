// <copyright file="PdfEncryptionService.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Application.DTOs;
using FileConversionApi.Application.Interfaces;
using FileConversionApi.Domain.Primitives;

using Microsoft.Extensions.Logging;

using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;
using PdfSharpCore.Pdf.Security;

namespace FileConversionApi.Infrastructure.Services;

/// <summary>
/// Service for encrypting PDF documents with password protection using PdfSharpCore.
/// </summary>
public class PdfEncryptionService : IPdfEncryptionService
{
    private readonly ILogger<PdfEncryptionService> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfEncryptionService"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public PdfEncryptionService(ILogger<PdfEncryptionService> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public Task<Result<byte[]>> EncryptAsync(
        byte[] pdfData,
        PasswordProtectionOptions options,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(options.UserPassword))
            {
                return Task.FromResult<Result<byte[]>>(new Error(
                    "Encryption.EmptyPassword",
                    "User password cannot be empty."));
            }

            this.logger.LogInformation("Applying password protection to PDF");

            using var inputStream = new MemoryStream(pdfData);
            using var document = PdfReader.Open(inputStream, PdfDocumentOpenMode.Modify);

            var securitySettings = document.SecuritySettings;

            // Set passwords
            securitySettings.UserPassword = options.UserPassword;
            securitySettings.OwnerPassword = options.OwnerPassword ?? options.UserPassword;

            // Set permissions
            securitySettings.PermitPrint = options.AllowPrinting;
            securitySettings.PermitExtractContent = options.AllowCopyingContent;
            securitySettings.PermitModifyDocument = options.AllowModifying;
            securitySettings.PermitAnnotations = options.AllowAnnotations;

            // Additional security settings
            securitySettings.PermitAccessibilityExtractContent = options.AllowCopyingContent;
            securitySettings.PermitAssembleDocument = options.AllowModifying;
            securitySettings.PermitFormsFill = options.AllowAnnotations;
            securitySettings.PermitFullQualityPrint = options.AllowPrinting;

            using var outputStream = new MemoryStream();
            document.Save(outputStream, false);

            this.logger.LogInformation(
                "Successfully applied password protection to PDF ({Size} bytes)",
                outputStream.Length);

            return Task.FromResult<Result<byte[]>>(outputStream.ToArray());
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Failed to encrypt PDF");
            return Task.FromResult<Result<byte[]>>(new Error(
                "Encryption.Failed",
                $"Failed to encrypt PDF: {ex.Message}"));
        }
    }
}
