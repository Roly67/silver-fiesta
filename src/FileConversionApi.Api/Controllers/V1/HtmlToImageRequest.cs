// <copyright file="HtmlToImageRequest.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Application.DTOs;

namespace FileConversionApi.Api.Controllers.V1;

/// <summary>
/// Request for HTML to image conversion.
/// </summary>
public record HtmlToImageRequest
{
    /// <summary>
    /// Gets the HTML content to convert.
    /// </summary>
    public string? HtmlContent { get; init; }

    /// <summary>
    /// Gets the URL to convert.
    /// </summary>
    public string? Url { get; init; }

    /// <summary>
    /// Gets the output file name.
    /// </summary>
    public string? FileName { get; init; }

    /// <summary>
    /// Gets the target image format (png, jpeg, webp). Defaults to png.
    /// </summary>
    public string TargetFormat { get; init; } = "png";

    /// <summary>
    /// Gets the conversion options.
    /// </summary>
    public ConversionOptions? Options { get; init; }

    /// <summary>
    /// Gets the webhook URL to notify when conversion completes.
    /// </summary>
    public string? WebhookUrl { get; init; }
}
