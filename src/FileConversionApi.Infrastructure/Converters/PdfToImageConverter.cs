// <copyright file="PdfToImageConverter.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using System.IO.Compression;

using FileConversionApi.Application.DTOs;
using FileConversionApi.Application.Interfaces;
using FileConversionApi.Domain.Errors;
using FileConversionApi.Domain.Primitives;

using Microsoft.Extensions.Logging;

using PDFtoImage;

using SkiaSharp;

namespace FileConversionApi.Infrastructure.Converters;

/// <summary>
/// Converter for PDF to image (PNG, JPEG, WebP) using PDFtoImage library.
/// </summary>
/// <remarks>
/// This converter uses PDFtoImage (PDFium wrapper) which supports Windows, Linux, macOS, and Android 31+.
/// The CA1416 warning is suppressed because this API is designed for server-side use on supported platforms.
/// </remarks>
#pragma warning disable CA1416 // Platform compatibility - PDFtoImage supports Windows, Linux, macOS, Android 31+
public class PdfToImageConverter : IFileConverter
{
    private readonly string targetFormat;
    private readonly ILogger<PdfToImageConverter> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfToImageConverter"/> class.
    /// </summary>
    /// <param name="targetFormat">The target image format (png, jpeg, webp).</param>
    /// <param name="logger">The logger.</param>
    public PdfToImageConverter(
        string targetFormat,
        ILogger<PdfToImageConverter> logger)
    {
        this.targetFormat = targetFormat?.ToLowerInvariant() ?? throw new ArgumentNullException(nameof(targetFormat));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public string SourceFormat => "pdf";

    /// <inheritdoc/>
    public string TargetFormat => this.targetFormat;

    /// <inheritdoc/>
    public async Task<Result<byte[]>> ConvertAsync(
        Stream input,
        ConversionOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            this.logger.LogDebug(
                "Starting PDF to {Format} conversion with DPI {Dpi}",
                this.targetFormat.ToUpperInvariant(),
                options.Dpi);

            // Read PDF into memory stream for PDFtoImage
            using var pdfStream = new MemoryStream();
            await input.CopyToAsync(pdfStream, cancellationToken).ConfigureAwait(false);
            pdfStream.Position = 0;

            var pdfBytes = pdfStream.ToArray();
            var pageCount = Conversion.GetPageCount(pdfBytes, options.PdfPassword);

            this.logger.LogDebug("PDF has {PageCount} pages", pageCount);

            if (pageCount == 0)
            {
                return ConversionJobErrors.ConversionFailed("PDF has no pages to convert.");
            }

            // Validate page number if specified
            if (options.PageNumber.HasValue)
            {
                if (options.PageNumber.Value < 1 || options.PageNumber.Value > pageCount)
                {
                    return ConversionJobErrors.ConversionFailed(
                        $"Page number {options.PageNumber.Value} is out of range. PDF has {pageCount} pages.");
                }
            }

            // Single page conversion
            if (options.PageNumber.HasValue || pageCount == 1)
            {
                var pageIndex = options.PageNumber.HasValue ? options.PageNumber.Value - 1 : 0;
                var imageBytes = await this.RenderPageAsync(pdfBytes, pageIndex, options, cancellationToken)
                    .ConfigureAwait(false);

                this.logger.LogDebug(
                    "PDF to {Format} conversion completed (single page), size: {Size} bytes",
                    this.targetFormat.ToUpperInvariant(),
                    imageBytes.Length);

                return imageBytes;
            }

            // Multi-page conversion - return ZIP file
            var zipBytes = await this.RenderAllPagesAsZipAsync(pdfBytes, pageCount, options, cancellationToken)
                .ConfigureAwait(false);

            this.logger.LogDebug(
                "PDF to {Format} conversion completed ({PageCount} pages), ZIP size: {Size} bytes",
                this.targetFormat.ToUpperInvariant(),
                pageCount,
                zipBytes.Length);

            return zipBytes;
        }
        catch (Exception ex) when (ex is not (OutOfMemoryException or StackOverflowException))
        {
            this.logger.LogError(ex, "PDF to {Format} conversion failed", this.targetFormat.ToUpperInvariant());
            return ConversionJobErrors.ConversionFailed(ex.Message);
        }
    }

    private Task<byte[]> RenderPageAsync(
        byte[] pdfBytes,
        int pageIndex,
        ConversionOptions options,
        CancellationToken cancellationToken)
    {
        return Task.Run(
            () =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                using var bitmap = Conversion.ToImage(
                    pdfBytes,
                    page: new Index(pageIndex),
                    password: options.PdfPassword,
                    options: new RenderOptions(Dpi: options.Dpi));

                return this.EncodeImage(bitmap, options);
            },
            cancellationToken);
    }

    private async Task<byte[]> RenderAllPagesAsZipAsync(
        byte[] pdfBytes,
        int pageCount,
        ConversionOptions options,
        CancellationToken cancellationToken)
    {
        using var zipStream = new MemoryStream();
        using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, leaveOpen: true))
        {
            for (var i = 0; i < pageCount; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var imageBytes = await this.RenderPageAsync(pdfBytes, i, options, cancellationToken)
                    .ConfigureAwait(false);

                var entryName = $"page_{i + 1:D4}.{this.targetFormat}";
                var entry = archive.CreateEntry(entryName, CompressionLevel.Optimal);

                await using var entryStream = entry.Open();
                await entryStream.WriteAsync(imageBytes, cancellationToken).ConfigureAwait(false);
            }
        }

        return zipStream.ToArray();
    }

    private byte[] EncodeImage(SKBitmap bitmap, ConversionOptions options)
    {
        using var image = SKImage.FromBitmap(bitmap);
        var format = this.GetSkiaFormat();
        var quality = options.ImageQuality ?? 90;

        using var data = image.Encode(format, quality);
        return data.ToArray();
    }

    private SKEncodedImageFormat GetSkiaFormat()
    {
        return this.targetFormat switch
        {
            "jpeg" or "jpg" => SKEncodedImageFormat.Jpeg,
            "webp" => SKEncodedImageFormat.Webp,
            _ => SKEncodedImageFormat.Png,
        };
    }
}
#pragma warning restore CA1416
