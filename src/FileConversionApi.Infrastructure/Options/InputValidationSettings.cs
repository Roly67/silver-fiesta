// <copyright file="InputValidationSettings.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

namespace FileConversionApi.Infrastructure.Options;

/// <summary>
/// Configuration settings for input validation.
/// </summary>
public class InputValidationSettings
{
    /// <summary>
    /// The configuration section name.
    /// </summary>
    public const string SectionName = "InputValidation";

    /// <summary>
    /// Gets or sets a value indicating whether input validation is enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum file size in bytes for uploaded files.
    /// Default is 50MB.
    /// </summary>
    public long MaxFileSizeBytes { get; set; } = 50 * 1024 * 1024;

    /// <summary>
    /// Gets or sets the maximum HTML content length in bytes.
    /// Default is 10MB.
    /// </summary>
    public long MaxHtmlContentBytes { get; set; } = 10 * 1024 * 1024;

    /// <summary>
    /// Gets or sets the maximum Markdown content length in bytes.
    /// Default is 5MB.
    /// </summary>
    public long MaxMarkdownContentBytes { get; set; } = 5 * 1024 * 1024;

    /// <summary>
    /// Gets or sets the URL validation settings for HTML to PDF conversion.
    /// </summary>
    public UrlValidationSettings UrlValidation { get; set; } = new();

    /// <summary>
    /// Gets or sets the allowed content types for file uploads.
    /// </summary>
    public ContentTypeValidationSettings ContentTypeValidation { get; set; } = new();
}
