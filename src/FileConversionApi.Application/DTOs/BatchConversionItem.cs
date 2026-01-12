// <copyright file="BatchConversionItem.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

namespace FileConversionApi.Application.DTOs;

/// <summary>
/// Represents a single item in a batch conversion request.
/// </summary>
public class BatchConversionItem
{
    /// <summary>
    /// Gets or sets the conversion type.
    /// Valid values: "html-to-pdf", "markdown-to-pdf", "markdown-to-html", "image".
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// Gets or sets the HTML content (for html-to-pdf conversion).
    /// </summary>
    public string? HtmlContent { get; set; }

    /// <summary>
    /// Gets or sets the URL to convert (for html-to-pdf conversion).
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// Gets or sets the Markdown content (for markdown-to-pdf and markdown-to-html conversions).
    /// </summary>
    public string? Markdown { get; set; }

    /// <summary>
    /// Gets or sets the image data as base64 (for image conversion).
    /// </summary>
    public string? ImageData { get; set; }

    /// <summary>
    /// Gets or sets the source format (for image conversion).
    /// </summary>
    public string? SourceFormat { get; set; }

    /// <summary>
    /// Gets or sets the target format (for image conversion).
    /// </summary>
    public string? TargetFormat { get; set; }

    /// <summary>
    /// Gets or sets the output file name.
    /// </summary>
    public string? FileName { get; set; }

    /// <summary>
    /// Gets or sets the conversion options.
    /// </summary>
    public ConversionOptions? Options { get; set; }
}
