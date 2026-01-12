// <copyright file="JpegToWebpConverter.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using Microsoft.Extensions.Logging;

namespace FileConversionApi.Infrastructure.Converters;

/// <summary>
/// Converter for JPEG to WebP format.
/// </summary>
public class JpegToWebpConverter : ImageConverterBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="JpegToWebpConverter"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public JpegToWebpConverter(ILogger<JpegToWebpConverter> logger)
        : base(logger)
    {
    }

    /// <inheritdoc/>
    public override string SourceFormat => "jpeg";

    /// <inheritdoc/>
    public override string TargetFormat => "webp";
}
