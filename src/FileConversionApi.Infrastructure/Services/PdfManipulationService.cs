// <copyright file="PdfManipulationService.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using System.Text.RegularExpressions;

using FileConversionApi.Application.DTOs;
using FileConversionApi.Application.Interfaces;
using FileConversionApi.Domain.Primitives;

using Microsoft.Extensions.Logging;

using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;

namespace FileConversionApi.Infrastructure.Services;

/// <summary>
/// Service for PDF manipulation operations (merge, split) using PdfSharpCore.
/// </summary>
public partial class PdfManipulationService : IPdfManipulationService
{
    private readonly ILogger<PdfManipulationService> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfManipulationService"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public PdfManipulationService(ILogger<PdfManipulationService> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public Task<Result<byte[]>> MergeAsync(
        IEnumerable<byte[]> pdfDocuments,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var documents = pdfDocuments.ToList();

            if (documents.Count == 0)
            {
                return Task.FromResult<Result<byte[]>>(new Error(
                    "Merge.NoDocuments",
                    "At least one PDF document is required for merging."));
            }

            if (documents.Count == 1)
            {
                return Task.FromResult<Result<byte[]>>(new Error(
                    "Merge.SingleDocument",
                    "At least two PDF documents are required for merging."));
            }

            this.logger.LogInformation("Merging {Count} PDF documents", documents.Count);

            using var outputDocument = new PdfDocument();

            foreach (var pdfData in documents)
            {
                cancellationToken.ThrowIfCancellationRequested();

                using var inputStream = new MemoryStream(pdfData);
                using var inputDocument = PdfReader.Open(inputStream, PdfDocumentOpenMode.Import);

                for (var i = 0; i < inputDocument.PageCount; i++)
                {
                    outputDocument.AddPage(inputDocument.Pages[i]);
                }
            }

            using var outputStream = new MemoryStream();
            outputDocument.Save(outputStream, false);

            this.logger.LogInformation(
                "Successfully merged PDFs into {PageCount} pages ({Size} bytes)",
                outputDocument.PageCount,
                outputStream.Length);

            return Task.FromResult<Result<byte[]>>(outputStream.ToArray());
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Failed to merge PDF documents");
            return Task.FromResult<Result<byte[]>>(new Error(
                "Merge.Failed",
                $"Failed to merge PDF documents: {ex.Message}"));
        }
    }

    /// <inheritdoc/>
    public Task<Result<Dictionary<string, byte[]>>> SplitAsync(
        byte[] pdfData,
        PdfSplitOptions options,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var inputStream = new MemoryStream(pdfData);
            using var inputDocument = PdfReader.Open(inputStream, PdfDocumentOpenMode.Import);

            var pageCount = inputDocument.PageCount;

            if (pageCount == 0)
            {
                return Task.FromResult<Result<Dictionary<string, byte[]>>>(new Error(
                    "Split.EmptyDocument",
                    "The PDF document has no pages to split."));
            }

            this.logger.LogInformation("Splitting PDF with {PageCount} pages", pageCount);

            var results = new Dictionary<string, byte[]>();

            if (options.SplitIntoSinglePages)
            {
                // Split into individual pages
                for (var i = 0; i < pageCount; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    using var outputDocument = new PdfDocument();
                    outputDocument.AddPage(inputDocument.Pages[i]);

                    using var outputStream = new MemoryStream();
                    outputDocument.Save(outputStream, false);

                    var fileName = $"page_{i + 1}.pdf";
                    results[fileName] = outputStream.ToArray();
                }
            }
            else if (options.PageRanges != null && options.PageRanges.Length > 0)
            {
                // Split by page ranges
                for (var rangeIndex = 0; rangeIndex < options.PageRanges.Length; rangeIndex++)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var range = options.PageRanges[rangeIndex];
                    var pageIndicesResult = ParsePageRange(range, pageCount);

                    if (pageIndicesResult.IsFailure)
                    {
                        return Task.FromResult<Result<Dictionary<string, byte[]>>>(pageIndicesResult.Error);
                    }

                    var pageIndices = pageIndicesResult.Value;

                    if (pageIndices.Count == 0)
                    {
                        continue;
                    }

                    using var outputDocument = new PdfDocument();

                    foreach (var pageIndex in pageIndices)
                    {
                        outputDocument.AddPage(inputDocument.Pages[pageIndex]);
                    }

                    using var outputStream = new MemoryStream();
                    outputDocument.Save(outputStream, false);

                    var fileName = $"pages_{range.Replace("-", "_")}.pdf";
                    results[fileName] = outputStream.ToArray();
                }
            }
            else
            {
                // Default: split into individual pages
                for (var i = 0; i < pageCount; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    using var outputDocument = new PdfDocument();
                    outputDocument.AddPage(inputDocument.Pages[i]);

                    using var outputStream = new MemoryStream();
                    outputDocument.Save(outputStream, false);

                    var fileName = $"page_{i + 1}.pdf";
                    results[fileName] = outputStream.ToArray();
                }
            }

            this.logger.LogInformation(
                "Successfully split PDF into {Count} documents",
                results.Count);

            return Task.FromResult<Result<Dictionary<string, byte[]>>>(results);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Failed to split PDF document");
            return Task.FromResult<Result<Dictionary<string, byte[]>>>(new Error(
                "Split.Failed",
                $"Failed to split PDF document: {ex.Message}"));
        }
    }

    private static Result<List<int>> ParsePageRange(string range, int pageCount)
    {
        var pageIndices = new List<int>();

        // Match patterns like "1", "1-5", "3-7"
        var match = PageRangeRegex().Match(range.Trim());

        if (!match.Success)
        {
            return new Error(
                "Split.InvalidPageRange",
                $"Invalid page range format: '{range}'. Use formats like '1', '1-5', or '3-7'.");
        }

        var startPage = int.Parse(match.Groups[1].Value);
        var endPage = match.Groups[2].Success
            ? int.Parse(match.Groups[2].Value)
            : startPage;

        if (startPage < 1 || endPage < 1)
        {
            return new Error(
                "Split.InvalidPageNumber",
                $"Page numbers must be greater than 0. Got range: '{range}'.");
        }

        if (startPage > pageCount || endPage > pageCount)
        {
            return new Error(
                "Split.PageOutOfRange",
                $"Page range '{range}' exceeds document page count ({pageCount}).");
        }

        if (startPage > endPage)
        {
            return new Error(
                "Split.InvalidPageRange",
                $"Start page cannot be greater than end page in range: '{range}'.");
        }

        for (var i = startPage; i <= endPage; i++)
        {
            pageIndices.Add(i - 1); // Convert to 0-based index
        }

        return pageIndices;
    }

    [GeneratedRegex(@"^(\d+)(?:-(\d+))?$")]
    private static partial Regex PageRangeRegex();
}
