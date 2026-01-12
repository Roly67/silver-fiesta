// <copyright file="ConvertMarkdownToHtmlCommand.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Application.DTOs;
using FileConversionApi.Domain.Primitives;

using MediatR;

namespace FileConversionApi.Application.Commands.Conversion;

/// <summary>
/// Command to convert Markdown to HTML.
/// </summary>
public record ConvertMarkdownToHtmlCommand : IRequest<Result<ConversionJobDto>>
{
    /// <summary>
    /// Gets the Markdown content to convert.
    /// </summary>
    public string? Markdown { get; init; }

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
