// <copyright file="PdfToImageRequest.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Application.DTOs;

namespace FileConversionApi.Api.Controllers.V1;

/// <summary>
/// Request for PDF to image conversion.
/// </summary>
public record PdfToImageRequest
{
    /// <summary>
    /// Gets the PDF data as base64.
    /// </summary>
    public string? PdfData { get; init; }

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
