// <copyright file="PngToWebpConverter.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using Microsoft.Extensions.Logging;

namespace FileConversionApi.Infrastructure.Converters;

/// <summary>
/// Converter for PNG to WebP format.
/// </summary>
public class PngToWebpConverter : ImageConverterBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PngToWebpConverter"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public PngToWebpConverter(ILogger<PngToWebpConverter> logger)
        : base(logger)
    {
    }

    /// <inheritdoc/>
    public override string SourceFormat => "png";

    /// <inheritdoc/>
    public override string TargetFormat => "webp";
}
