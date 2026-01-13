// <copyright file="ContentTypeValidationSettings.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

namespace FileConversionApi.Infrastructure.Options;

/// <summary>
/// Settings for content type validation.
/// </summary>
public class ContentTypeValidationSettings
{
    /// <summary>
    /// Gets or sets a value indicating whether content type validation is enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the allowed content types for HTML conversion.
    /// </summary>
    public List<string> AllowedHtmlContentTypes { get; set; } =
    [
        "text/html",
        "text/plain",
        "application/xhtml+xml",
    ];

    /// <summary>
    /// Gets or sets the allowed content types for Markdown conversion.
    /// </summary>
    public List<string> AllowedMarkdownContentTypes { get; set; } =
    [
        "text/markdown",
        "text/plain",
        "text/x-markdown",
    ];

    /// <summary>
    /// Gets or sets the allowed content types for image conversion.
    /// </summary>
    public List<string> AllowedImageContentTypes { get; set; } =
    [
        "image/jpeg",
        "image/png",
        "image/gif",
        "image/webp",
        "image/bmp",
        "image/tiff",
    ];
}
