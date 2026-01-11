// <copyright file="HtmlToPdfRequest.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Application.DTOs;

namespace FileConversionApi.Api.Controllers.V1;

/// <summary>
/// Request for HTML to PDF conversion.
/// </summary>
public record HtmlToPdfRequest
{
    /// <summary>
    /// Gets the HTML content to convert.
    /// </summary>
    public string? HtmlContent { get; init; }

    /// <summary>
    /// Gets the URL to convert.
    /// </summary>
    public string? Url { get; init; }

    /// <summary>
    /// Gets the output file name.
    /// </summary>
    public string? FileName { get; init; }

    /// <summary>
    /// Gets the conversion options.
    /// </summary>
    public ConversionOptions? Options { get; init; }
}
