// <copyright file="IPdfWatermarkService.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Application.DTOs;
using FileConversionApi.Domain.Primitives;

namespace FileConversionApi.Application.Interfaces;

/// <summary>
/// Service for adding watermarks to PDF documents.
/// </summary>
public interface IPdfWatermarkService
{
    /// <summary>
    /// Adds a watermark to a PDF document.
    /// </summary>
    /// <param name="pdfData">The PDF document as a byte array.</param>
    /// <param name="options">The watermark options.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The watermarked PDF as a byte array.</returns>
    Task<Result<byte[]>> ApplyWatermarkAsync(
        byte[] pdfData,
        WatermarkOptions options,
        CancellationToken cancellationToken = default);
}
