// <copyright file="ConvertImageCommand.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Application.DTOs;
using FileConversionApi.Domain.Primitives;

using MediatR;

namespace FileConversionApi.Application.Commands.Conversion;

/// <summary>
/// Command to convert an image between formats.
/// </summary>
public record ConvertImageCommand : IRequest<Result<ConversionJobDto>>
{
    /// <summary>
    /// Gets the image data as base64 encoded string.
    /// </summary>
    public string? ImageData { get; init; }

    /// <summary>
    /// Gets the source format (png, jpeg, webp, gif, bmp).
    /// </summary>
    public string? SourceFormat { get; init; }

    /// <summary>
    /// Gets the target format (png, jpeg, webp, gif, bmp).
    /// </summary>
    public string? TargetFormat { get; init; }

    /// <summary>
    /// Gets the file name for the output.
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
