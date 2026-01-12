// <copyright file="JpegToPngConverter.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using Microsoft.Extensions.Logging;

namespace FileConversionApi.Infrastructure.Converters;

/// <summary>
/// Converter for JPEG to PNG format.
/// </summary>
public class JpegToPngConverter : ImageConverterBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="JpegToPngConverter"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public JpegToPngConverter(ILogger<JpegToPngConverter> logger)
        : base(logger)
    {
    }

    /// <inheritdoc/>
    public override string SourceFormat => "jpeg";

    /// <inheritdoc/>
    public override string TargetFormat => "png";
}
