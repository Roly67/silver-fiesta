// <copyright file="WebpToPngConverter.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using Microsoft.Extensions.Logging;

namespace FileConversionApi.Infrastructure.Converters;

/// <summary>
/// Converter for WebP to PNG format.
/// </summary>
public class WebpToPngConverter : ImageConverterBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WebpToPngConverter"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public WebpToPngConverter(ILogger<WebpToPngConverter> logger)
        : base(logger)
    {
    }

    /// <inheritdoc/>
    public override string SourceFormat => "webp";

    /// <inheritdoc/>
    public override string TargetFormat => "png";
}
