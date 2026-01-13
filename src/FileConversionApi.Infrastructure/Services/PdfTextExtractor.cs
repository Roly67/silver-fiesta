// <copyright file="PdfTextExtractor.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using System.Text;

using FileConversionApi.Application.Interfaces;
using FileConversionApi.Domain.Errors;
using FileConversionApi.Domain.Primitives;

using Microsoft.Extensions.Logging;

using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Exceptions;

namespace FileConversionApi.Infrastructure.Services;

/// <summary>
/// PDF text extraction service using PdfPig library.
/// </summary>
public class PdfTextExtractor : IPdfTextExtractor
{
    private readonly ILogger<PdfTextExtractor> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfTextExtractor"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public PdfTextExtractor(ILogger<PdfTextExtractor> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public Task<Result<string>> ExtractTextAsync(
        Stream pdfStream,
        int? pageNumber,
        string? password,
        CancellationToken cancellationToken)
    {
        return Task.Run(
            () => this.ExtractTextInternal(pdfStream, pageNumber, password, cancellationToken),
            cancellationToken);
    }

    private static string ExtractPageText(Page page)
    {
        // Get text content, preserving word order
        var text = page.Text;

        // Clean up excessive whitespace while preserving paragraph breaks
        var lines = text.Split('\n')
            .Select(line => string.Join(" ", line.Split(Array.Empty<char>(), StringSplitOptions.RemoveEmptyEntries)))
            .Where(line => !string.IsNullOrWhiteSpace(line));

        return string.Join(Environment.NewLine, lines);
    }

    private Result<string> ExtractTextInternal(
        Stream pdfStream,
        int? pageNumber,
        string? password,
        CancellationToken cancellationToken)
    {
        try
        {
            this.logger.LogDebug("Starting PDF text extraction");

            // Copy stream to memory for PdfPig
            using var memoryStream = new MemoryStream();
            pdfStream.CopyTo(memoryStream);
            memoryStream.Position = 0;

            var parsingOptions = new ParsingOptions();
            if (password != null)
            {
                parsingOptions.Password = password;
            }

            using var document = PdfDocument.Open(memoryStream, parsingOptions);

            var pageCount = document.NumberOfPages;
            this.logger.LogDebug("PDF has {PageCount} pages", pageCount);

            if (pageCount == 0)
            {
                return ConversionJobErrors.ConversionFailed("PDF has no pages to extract text from.");
            }

            // Validate page number if specified
            if (pageNumber.HasValue)
            {
                if (pageNumber.Value < 1 || pageNumber.Value > pageCount)
                {
                    return ConversionJobErrors.ConversionFailed(
                        $"Page number {pageNumber.Value} is out of range. PDF has {pageCount} pages.");
                }
            }

            var textBuilder = new StringBuilder();

            if (pageNumber.HasValue)
            {
                // Extract from single page
                cancellationToken.ThrowIfCancellationRequested();
                var page = document.GetPage(pageNumber.Value);
                var pageText = ExtractPageText(page);
                textBuilder.Append(pageText);
            }
            else
            {
                // Extract from all pages
                for (var i = 1; i <= pageCount; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var page = document.GetPage(i);
                    var pageText = ExtractPageText(page);

                    if (pageCount > 1)
                    {
                        textBuilder.AppendLine($"--- Page {i} ---");
                    }

                    textBuilder.AppendLine(pageText);

                    if (i < pageCount)
                    {
                        textBuilder.AppendLine();
                    }
                }
            }

            var extractedText = textBuilder.ToString().TrimEnd();

            this.logger.LogDebug(
                "PDF text extraction completed, extracted {CharCount} characters from {PageCount} pages",
                extractedText.Length,
                pageNumber.HasValue ? 1 : pageCount);

            return extractedText;
        }
        catch (PdfDocumentEncryptedException)
        {
            this.logger.LogWarning("PDF is password protected and no valid password was provided");
            return ConversionJobErrors.ConversionFailed(
                "PDF is password protected. Please provide the correct password.");
        }
        catch (Exception ex) when (ex is not (OutOfMemoryException or StackOverflowException or OperationCanceledException))
        {
            this.logger.LogError(ex, "PDF text extraction failed");
            return ConversionJobErrors.ConversionFailed($"Failed to extract text from PDF: {ex.Message}");
        }
    }
}
