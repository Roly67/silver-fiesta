// <copyright file="IPdfTextExtractor.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Domain.Primitives;

namespace FileConversionApi.Application.Interfaces;

/// <summary>
/// Interface for PDF text extraction service.
/// </summary>
public interface IPdfTextExtractor
{
    /// <summary>
    /// Extracts text from a PDF document.
    /// </summary>
    /// <param name="pdfStream">The PDF content stream.</param>
    /// <param name="pageNumber">Optional specific page number (1-based). If null, extracts from all pages.</param>
    /// <param name="password">Optional password for encrypted PDFs.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the extracted text or an error.</returns>
    Task<Result<string>> ExtractTextAsync(
        Stream pdfStream,
        int? pageNumber,
        string? password,
        CancellationToken cancellationToken);
}
