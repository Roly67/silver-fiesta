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

    /// <summary>
    /// Gets or sets a value indicating whether to capture the full scrollable page (for HTML to image).
    /// </summary>
    public bool FullPage { get; set; } = true;

    /// <summary>
    /// Gets or sets the viewport width in pixels (for HTML to image).
    /// </summary>
    public int ViewportWidth { get; set; } = 1920;

    /// <summary>
    /// Gets or sets the viewport height in pixels (for HTML to image).
    /// </summary>
    public int ViewportHeight { get; set; } = 1080;

    /// <summary>
    /// Gets or sets the watermark options for PDF output.
    /// </summary>
    public WatermarkOptions? Watermark { get; set; }

    /// <summary>
    /// Gets or sets the password protection options for PDF output.
    /// </summary>
    public PasswordProtectionOptions? PasswordProtection { get; set; }

    /// <summary>
    /// Gets or sets the DPI (dots per inch) for PDF to image rendering.
    /// </summary>
    public int Dpi { get; set; } = 150;

    /// <summary>
    /// Gets or sets the specific page number to convert (1-based). If null, all pages are converted.
    /// </summary>
    public int? PageNumber { get; set; }

    /// <summary>
    /// Gets or sets the password for opening password-protected PDFs.
    /// </summary>
    public string? PdfPassword { get; set; }
}
