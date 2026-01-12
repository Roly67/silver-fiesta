// <copyright file="WatermarkOptions.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

namespace FileConversionApi.Application.DTOs;

/// <summary>
/// Options for PDF watermarking.
/// </summary>
public class WatermarkOptions
{
    /// <summary>
    /// Gets or sets the watermark text.
    /// </summary>
    public string? Text { get; set; }

    /// <summary>
    /// Gets or sets the font size for text watermarks.
    /// </summary>
    public int FontSize { get; set; } = 48;

    /// <summary>
    /// Gets or sets the font family name.
    /// </summary>
    public string FontFamily { get; set; } = "Helvetica";

    /// <summary>
    /// Gets or sets the watermark color in hex format (e.g., "#FF0000" for red).
    /// </summary>
    public string Color { get; set; } = "#808080";

    /// <summary>
    /// Gets or sets the opacity (0.0 to 1.0).
    /// </summary>
    public double Opacity { get; set; } = 0.3;

    /// <summary>
    /// Gets or sets the rotation angle in degrees.
    /// </summary>
    public double Rotation { get; set; } = -45;

    /// <summary>
    /// Gets or sets the watermark position.
    /// </summary>
    public WatermarkPosition Position { get; set; } = WatermarkPosition.Center;

    /// <summary>
    /// Gets or sets a value indicating whether to apply watermark to all pages.
    /// </summary>
    public bool AllPages { get; set; } = true;

    /// <summary>
    /// Gets or sets the specific page numbers to watermark (1-based). Ignored if AllPages is true.
    /// </summary>
    public int[]? PageNumbers { get; set; }
}
