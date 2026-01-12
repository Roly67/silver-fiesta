// <copyright file="PdfSplitOptions.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

namespace FileConversionApi.Application.DTOs;

/// <summary>
/// Options for PDF splitting.
/// </summary>
public class PdfSplitOptions
{
    /// <summary>
    /// Gets or sets the page ranges to extract (e.g., "1-3", "5", "7-10").
    /// If null or empty, each page will be split into a separate PDF.
    /// </summary>
    public string[]? PageRanges { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to split into individual pages.
    /// When true, ignores PageRanges and creates one PDF per page.
    /// </summary>
    public bool SplitIntoSinglePages { get; set; }
}
