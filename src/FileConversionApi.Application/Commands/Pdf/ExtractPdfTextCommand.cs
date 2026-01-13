// <copyright file="ExtractPdfTextCommand.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Application.DTOs;
using FileConversionApi.Domain.Primitives;

using MediatR;

namespace FileConversionApi.Application.Commands.Pdf;

/// <summary>
/// Command to extract text from a PDF document.
/// </summary>
public record ExtractPdfTextCommand : IRequest<Result<ConversionJobDto>>
{
    /// <summary>
    /// Gets the PDF data as base64.
    /// </summary>
    public string? PdfData { get; init; }

    /// <summary>
    /// Gets the file name.
    /// </summary>
    public string? FileName { get; init; }

    /// <summary>
    /// Gets the specific page number to extract (1-based). If null, extracts from all pages.
    /// </summary>
    public int? PageNumber { get; init; }

    /// <summary>
    /// Gets the password for encrypted PDFs.
    /// </summary>
    public string? Password { get; init; }

    /// <summary>
    /// Gets the webhook URL to notify when extraction completes.
    /// </summary>
    public string? WebhookUrl { get; init; }
}
