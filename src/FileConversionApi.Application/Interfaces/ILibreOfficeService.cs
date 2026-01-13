// <copyright file="ILibreOfficeService.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Domain.Primitives;

namespace FileConversionApi.Application.Interfaces;

/// <summary>
/// Service for converting documents using LibreOffice.
/// </summary>
public interface ILibreOfficeService
{
    /// <summary>
    /// Converts a document to PDF using LibreOffice.
    /// </summary>
    /// <param name="inputData">The input document data.</param>
    /// <param name="inputFormat">The input file format (e.g., "docx", "xlsx").</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The PDF document as a byte array.</returns>
    Task<Result<byte[]>> ConvertToPdfAsync(
        byte[] inputData,
        string inputFormat,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the supported input formats.
    /// </summary>
    /// <returns>An enumerable of supported format extensions.</returns>
    IEnumerable<string> GetSupportedFormats();

    /// <summary>
    /// Checks if LibreOffice is available and properly configured.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if LibreOffice is available, false otherwise.</returns>
    Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default);
}
