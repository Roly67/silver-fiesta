// <copyright file="SplitPdfCommand.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Application.DTOs;
using FileConversionApi.Domain.Primitives;

using MediatR;

namespace FileConversionApi.Application.Commands.Pdf;

/// <summary>
/// Command to split a PDF document into multiple PDFs.
/// </summary>
public record SplitPdfCommand : IRequest<Result<ConversionJobDto>>
{
    /// <summary>
    /// Gets the PDF document as a base64 encoded string.
    /// </summary>
    public string? PdfData { get; init; }

    /// <summary>
    /// Gets the file name for the input.
    /// </summary>
    public string? FileName { get; init; }

    /// <summary>
    /// Gets the split options.
    /// </summary>
    public PdfSplitOptions? Options { get; init; }

    /// <summary>
    /// Gets the webhook URL to notify when operation completes.
    /// </summary>
    public string? WebhookUrl { get; init; }
}
