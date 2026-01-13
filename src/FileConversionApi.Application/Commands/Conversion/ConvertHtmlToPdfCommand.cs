// <copyright file="ConvertHtmlToPdfCommand.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Application.DTOs;
using FileConversionApi.Domain.Primitives;
using MediatR;

namespace FileConversionApi.Application.Commands.Conversion;

/// <summary>
/// Command to convert HTML to PDF.
/// </summary>
public record ConvertHtmlToPdfCommand : IRequest<Result<ConversionJobDto>>, IConversionCommand
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
