// <copyright file="ExtractPdfTextRequest.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

namespace FileConversionApi.Api.Controllers.V1;

/// <summary>
/// Request for PDF text extraction.
/// </summary>
public record ExtractPdfTextRequest
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
