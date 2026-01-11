// <copyright file="FileDownloadResult.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

namespace FileConversionApi.Application.Queries.Conversion;

/// <summary>
/// Result containing file download information.
/// </summary>
public record FileDownloadResult
{
    /// <summary>
    /// Gets the file content.
    /// </summary>
    public required byte[] Content { get; init; }

    /// <summary>
    /// Gets the file name.
    /// </summary>
    public required string FileName { get; init; }

    /// <summary>
    /// Gets the content type.
    /// </summary>
    public required string ContentType { get; init; }
}
