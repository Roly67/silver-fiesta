// <copyright file="ImageConversionRequest.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Application.DTOs;

namespace FileConversionApi.Api.Controllers.V1;

/// <summary>
/// Request for image format conversion.
/// </summary>
public record ImageConversionRequest
{
    /// <summary>
    /// Gets the image data as base64 encoded string.
    /// </summary>
    public string? ImageData { get; init; }

    /// <summary>
    /// Gets the source format (png, jpeg, jpg, webp, gif, bmp).
    /// </summary>
    public string? SourceFormat { get; init; }

    /// <summary>
    /// Gets the target format (png, jpeg, jpg, webp, gif, bmp).
    /// </summary>
    public string? TargetFormat { get; init; }

    /// <summary>
    /// Gets the output file name.
    /// </summary>
    public string? FileName { get; init; }

    /// <summary>
    /// Gets the conversion options.
    /// </summary>
    public ConversionOptions? Options { get; init; }

    /// <summary>
    /// Gets the webhook URL to notify when conversion completes.
    /// </summary>
    public string? WebhookUrl { get; init; }
}
