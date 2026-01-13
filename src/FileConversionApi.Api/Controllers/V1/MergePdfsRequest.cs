// <copyright file="MergePdfsRequest.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

namespace FileConversionApi.Api.Controllers.V1;

/// <summary>
/// Request for merging multiple PDF documents.
/// </summary>
public record MergePdfsRequest
{
    /// <summary>
    /// Gets the PDF documents as base64 encoded strings.
    /// </summary>
    public string[]? PdfDocuments { get; init; }

    /// <summary>
    /// Gets the output file name.
    /// </summary>
    public string? FileName { get; init; }

    /// <summary>
    /// Gets the webhook URL to notify when operation completes.
    /// </summary>
    public string? WebhookUrl { get; init; }
}
