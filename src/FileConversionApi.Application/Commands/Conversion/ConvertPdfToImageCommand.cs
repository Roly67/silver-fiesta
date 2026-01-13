// <copyright file="ConvertPdfToImageCommand.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Application.DTOs;
using FileConversionApi.Domain.Primitives;

using MediatR;

namespace FileConversionApi.Application.Commands.Conversion;

/// <summary>
/// Command to convert PDF to image (PNG, JPEG, or WebP).
/// </summary>
public record ConvertPdfToImageCommand : IRequest<Result<ConversionJobDto>>, IConversionCommand
{
    /// <summary>
    /// Gets the PDF data as base64.
    /// </summary>
    public string? PdfData { get; init; }

    /// <summary>
    /// Gets the file name for the output.
    /// </summary>
    public string? FileName { get; init; }

    /// <summary>
    /// Gets the target image format (png, jpeg, webp).
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
