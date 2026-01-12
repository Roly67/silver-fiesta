// <copyright file="ConversionOptions.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

namespace FileConversionApi.Application.DTOs;

/// <summary>
/// Options for file conversion.
/// </summary>
public class ConversionOptions
{
    /// <summary>
    /// Gets or sets the page size (e.g., A4, Letter).
    /// </summary>
    public string? PageSize { get; set; } = "A4";

    /// <summary>
    /// Gets or sets a value indicating whether to use landscape orientation.
    /// </summary>
    public bool Landscape { get; set; }

    /// <summary>
    /// Gets or sets the top margin in pixels.
    /// </summary>
    public int MarginTop { get; set; } = 20;

    /// <summary>
    /// Gets or sets the bottom margin in pixels.
    /// </summary>
    public int MarginBottom { get; set; } = 20;

    /// <summary>
    /// Gets or sets the left margin in pixels.
    /// </summary>
    public int MarginLeft { get; set; } = 20;

    /// <summary>
    /// Gets or sets the right margin in pixels.
    /// </summary>
    public int MarginRight { get; set; } = 20;

    /// <summary>
    /// Gets or sets the header template HTML.
    /// </summary>
    public string? HeaderTemplate { get; set; }

    /// <summary>
    /// Gets or sets the footer template HTML.
    /// </summary>
    public string? FooterTemplate { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to wait for JavaScript.
    /// </summary>
    public bool WaitForJavaScript { get; set; } = true;

    /// <summary>
    /// Gets or sets the JavaScript timeout in milliseconds.
    /// </summary>
    public int JavaScriptTimeout { get; set; } = 30000;

    /// <summary>
    /// Gets or sets the target image width in pixels (for image conversions).
    /// </summary>
    public int? ImageWidth { get; set; }

    /// <summary>
    /// Gets or sets the target image height in pixels (for image conversions).
    /// </summary>
    public int? ImageHeight { get; set; }

    /// <summary>
    /// Gets or sets the image quality (1-100, for JPEG/WebP).
    /// </summary>
    public int? ImageQuality { get; set; }
}
