// <copyright file="WebpToJpegConverter.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using Microsoft.Extensions.Logging;

namespace FileConversionApi.Infrastructure.Converters;

/// <summary>
/// Converter for WebP to JPEG format.
/// </summary>
public class WebpToJpegConverter : ImageConverterBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WebpToJpegConverter"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public WebpToJpegConverter(ILogger<WebpToJpegConverter> logger)
        : base(logger)
    {
    }

    /// <inheritdoc/>
    public override string SourceFormat => "webp";

    /// <inheritdoc/>
    public override string TargetFormat => "jpeg";
}
