// <copyright file="IPdfManipulationService.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Application.DTOs;
using FileConversionApi.Domain.Primitives;

namespace FileConversionApi.Application.Interfaces;

/// <summary>
/// Service for PDF manipulation operations (merge, split).
/// </summary>
public interface IPdfManipulationService
{
    /// <summary>
    /// Merges multiple PDF documents into a single PDF.
    /// </summary>
    /// <param name="pdfDocuments">The PDF documents to merge (in order).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The merged PDF as a byte array.</returns>
    Task<Result<byte[]>> MergeAsync(
        IEnumerable<byte[]> pdfDocuments,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Splits a PDF document into multiple PDFs based on options.
    /// </summary>
    /// <param name="pdfData">The PDF document to split.</param>
    /// <param name="options">The split options.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A dictionary of output file names to PDF byte arrays.</returns>
    Task<Result<Dictionary<string, byte[]>>> SplitAsync(
        byte[] pdfData,
        PdfSplitOptions options,
        CancellationToken cancellationToken = default);
}
