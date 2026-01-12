// <copyright file="ImageConverterBase.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Application.DTOs;
using FileConversionApi.Application.Interfaces;
using FileConversionApi.Domain.Errors;
using FileConversionApi.Domain.Primitives;

using Microsoft.Extensions.Logging;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace FileConversionApi.Infrastructure.Converters;

/// <summary>
/// Base class for image format converters using ImageSharp.
/// </summary>
public abstract class ImageConverterBase : IFileConverter
{
    private readonly ILogger logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ImageConverterBase"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    protected ImageConverterBase(ILogger logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public abstract string SourceFormat { get; }

    /// <inheritdoc/>
    public abstract string TargetFormat { get; }

    /// <inheritdoc/>
    public async Task<Result<byte[]>> ConvertAsync(
        Stream input,
        ConversionOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            this.logger.LogDebug(
                "Starting image conversion from {SourceFormat} to {TargetFormat}",
                this.SourceFormat,
                this.TargetFormat);

            using var image = await Image.LoadAsync(input, cancellationToken).ConfigureAwait(false);

            // Apply resize if dimensions are specified
            if (options.ImageWidth.HasValue || options.ImageHeight.HasValue)
            {
                var width = options.ImageWidth ?? 0;
                var height = options.ImageHeight ?? 0;

                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Size = new Size(width, height),
                    Mode = ResizeMode.Max,
                }));

                this.logger.LogDebug("Resized image to {Width}x{Height}", image.Width, image.Height);
            }

            using var outputStream = new MemoryStream();
            var encoder = this.GetEncoder(options);

            await image.SaveAsync(outputStream, encoder, cancellationToken).ConfigureAwait(false);

            this.logger.LogInformation(
                "Image conversion from {SourceFormat} to {TargetFormat} completed successfully",
                this.SourceFormat,
                this.TargetFormat);

            return outputStream.ToArray();
        }
        catch (UnknownImageFormatException ex)
        {
            this.logger.LogError(ex, "Unknown image format");
            return ConversionJobErrors.ConversionFailed($"Unknown or unsupported image format: {ex.Message}");
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Image conversion failed");
            return ConversionJobErrors.ConversionFailed(ex.Message);
        }
    }

    /// <summary>
    /// Gets the encoder for the target format.
    /// </summary>
    /// <param name="options">The conversion options.</param>
    /// <returns>The image encoder.</returns>
    protected virtual IImageEncoder GetEncoder(ConversionOptions options)
    {
        var quality = options.ImageQuality ?? 90;

        return this.TargetFormat.ToLowerInvariant() switch
        {
            "jpeg" or "jpg" => new JpegEncoder { Quality = quality },
            "png" => new PngEncoder { CompressionLevel = PngCompressionLevel.DefaultCompression },
            "webp" => new WebpEncoder { Quality = quality },
            "gif" => new GifEncoder(),
            "bmp" => new BmpEncoder(),
            _ => throw new NotSupportedException($"Target format '{this.TargetFormat}' is not supported."),
        };
    }
}
