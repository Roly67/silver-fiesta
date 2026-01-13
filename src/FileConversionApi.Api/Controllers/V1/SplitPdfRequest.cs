// <copyright file="SplitPdfRequest.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Application.DTOs;

namespace FileConversionApi.Api.Controllers.V1;

/// <summary>
/// Request for splitting a PDF document.
/// </summary>
public record SplitPdfRequest
{
    /// <summary>
    /// Gets the PDF document as a base64 encoded string.
    /// </summary>
    public string? PdfData { get; init; }

    /// <summary>
    /// Gets the input file name.
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
