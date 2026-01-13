// <copyright file="OfficeConverterBase.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Application.DTOs;
using FileConversionApi.Application.Interfaces;
using FileConversionApi.Domain.Primitives;

using Microsoft.Extensions.Logging;

namespace FileConversionApi.Infrastructure.Converters;

/// <summary>
/// Base class for office document to PDF converters using LibreOffice.
/// </summary>
public abstract class OfficeConverterBase : IFileConverter
{
    private readonly ILibreOfficeService libreOfficeService;
    private readonly IPdfWatermarkService watermarkService;
    private readonly IPdfEncryptionService encryptionService;
    private readonly ILogger logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="OfficeConverterBase"/> class.
    /// </summary>
    /// <param name="libreOfficeService">The LibreOffice service.</param>
    /// <param name="watermarkService">The watermark service.</param>
    /// <param name="encryptionService">The encryption service.</param>
    /// <param name="logger">The logger.</param>
    protected OfficeConverterBase(
        ILibreOfficeService libreOfficeService,
        IPdfWatermarkService watermarkService,
        IPdfEncryptionService encryptionService,
        ILogger logger)
    {
        this.libreOfficeService = libreOfficeService ?? throw new ArgumentNullException(nameof(libreOfficeService));
        this.watermarkService = watermarkService ?? throw new ArgumentNullException(nameof(watermarkService));
        this.encryptionService = encryptionService ?? throw new ArgumentNullException(nameof(encryptionService));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public abstract string SourceFormat { get; }

    /// <inheritdoc/>
    public string TargetFormat => "pdf";

    /// <inheritdoc/>
    public async Task<Result<byte[]>> ConvertAsync(
        Stream input,
        ConversionOptions options,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);

        this.logger.LogInformation(
            "Starting {SourceFormat} to PDF conversion",
            this.SourceFormat.ToUpperInvariant());

        // Read input stream to byte array
        using var memoryStream = new MemoryStream();
        await input.CopyToAsync(memoryStream, cancellationToken).ConfigureAwait(false);
        var inputData = memoryStream.ToArray();

        // Convert using LibreOffice
        var conversionResult = await this.libreOfficeService.ConvertToPdfAsync(
            inputData,
            this.SourceFormat,
            cancellationToken).ConfigureAwait(false);

        if (conversionResult.IsFailure)
        {
            return conversionResult;
        }

        var pdfData = conversionResult.Value;

        // Apply watermark if specified
        if (options?.Watermark is not null)
        {
            this.logger.LogDebug("Applying watermark to PDF");
            var watermarkResult = await this.watermarkService.ApplyWatermarkAsync(
                pdfData,
                options.Watermark,
                cancellationToken).ConfigureAwait(false);

            if (watermarkResult.IsFailure)
            {
                return watermarkResult;
            }

            pdfData = watermarkResult.Value;
        }

        // Apply encryption if specified
        if (options?.PasswordProtection is not null)
        {
            this.logger.LogDebug("Applying password protection to PDF");
            var encryptionResult = await this.encryptionService.EncryptAsync(
                pdfData,
                options.PasswordProtection,
                cancellationToken).ConfigureAwait(false);

            if (encryptionResult.IsFailure)
            {
                return encryptionResult;
            }

            pdfData = encryptionResult.Value;
        }

        this.logger.LogInformation(
            "Successfully converted {SourceFormat} to PDF ({InputSize} bytes -> {OutputSize} bytes)",
            this.SourceFormat.ToUpperInvariant(),
            inputData.Length,
            pdfData.Length);

        return pdfData;
    }
}
