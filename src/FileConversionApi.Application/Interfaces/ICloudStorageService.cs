// <copyright file="ICloudStorageService.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Domain.Primitives;

namespace FileConversionApi.Application.Interfaces;

/// <summary>
/// Service interface for S3-compatible cloud storage operations.
/// </summary>
public interface ICloudStorageService
{
    /// <summary>
    /// Gets a value indicating whether cloud storage is enabled.
    /// </summary>
    bool IsEnabled { get; }

    /// <summary>
    /// Uploads data to cloud storage.
    /// </summary>
    /// <param name="data">The data to upload.</param>
    /// <param name="key">The storage key (path).</param>
    /// <param name="contentType">The content type of the data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the storage key on success.</returns>
    Task<Result<string>> UploadAsync(
        byte[] data,
        string key,
        string contentType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads data from cloud storage.
    /// </summary>
    /// <param name="key">The storage key (path).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the data on success.</returns>
    Task<Result<byte[]>> DownloadAsync(
        string key,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an object from cloud storage.
    /// </summary>
    /// <param name="key">The storage key (path).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    Task<Result> DeleteAsync(
        string key,
        CancellationToken cancellationToken = default);
}
