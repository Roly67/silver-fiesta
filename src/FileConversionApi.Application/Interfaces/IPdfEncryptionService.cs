// <copyright file="IPdfEncryptionService.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Application.DTOs;
using FileConversionApi.Domain.Primitives;

namespace FileConversionApi.Application.Interfaces;

/// <summary>
/// Service for encrypting PDF documents with password protection.
/// </summary>
public interface IPdfEncryptionService
{
    /// <summary>
    /// Encrypts a PDF document with password protection.
    /// </summary>
    /// <param name="pdfData">The PDF document as a byte array.</param>
    /// <param name="options">The password protection options.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The encrypted PDF as a byte array.</returns>
    Task<Result<byte[]>> EncryptAsync(
        byte[] pdfData,
        PasswordProtectionOptions options,
        CancellationToken cancellationToken = default);
}
