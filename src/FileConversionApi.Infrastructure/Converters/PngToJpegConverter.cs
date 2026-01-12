// <copyright file="PngToJpegConverter.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using Microsoft.Extensions.Logging;

namespace FileConversionApi.Infrastructure.Converters;

/// <summary>
/// Converter for PNG to JPEG format.
/// </summary>
public class PngToJpegConverter : ImageConverterBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PngToJpegConverter"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public PngToJpegConverter(ILogger<PngToJpegConverter> logger)
        : base(logger)
    {
    }

    /// <inheritdoc/>
    public override string SourceFormat => "png";

    /// <inheritdoc/>
    public override string TargetFormat => "jpeg";
}
