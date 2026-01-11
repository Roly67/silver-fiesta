// <copyright file="MarkdownToPdfRequest.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Application.DTOs;

namespace FileConversionApi.Api.Controllers.V1;

/// <summary>
/// Request for Markdown to PDF conversion.
/// </summary>
public record MarkdownToPdfRequest
{
    /// <summary>
    /// Gets the Markdown content to convert.
    /// </summary>
    public string? Markdown { get; init; }

    /// <summary>
    /// Gets the output file name.
    /// </summary>
    public string? FileName { get; init; }

    /// <summary>
    /// Gets the conversion options.
    /// </summary>
    public ConversionOptions? Options { get; init; }
}
