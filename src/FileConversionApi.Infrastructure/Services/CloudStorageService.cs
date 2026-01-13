// <copyright file="CloudStorageService.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using Amazon;
using Amazon.S3;
using Amazon.S3.Model;

using FileConversionApi.Application.Interfaces;
using FileConversionApi.Domain.Primitives;
using FileConversionApi.Infrastructure.Options;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FileConversionApi.Infrastructure.Services;

/// <summary>
/// S3-compatible cloud storage service implementation.
/// </summary>
public class CloudStorageService : ICloudStorageService, IDisposable
{
    private readonly CloudStorageSettings settings;
    private readonly ILogger<CloudStorageService> logger;
    private readonly IAmazonS3? s3Client;
    private bool disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="CloudStorageService"/> class.
    /// </summary>
    /// <param name="settings">The cloud storage settings.</param>
    /// <param name="logger">The logger.</param>
    public CloudStorageService(
        IOptions<CloudStorageSettings> settings,
        ILogger<CloudStorageService> logger)
    {
        this.settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

        if (this.settings.Enabled)
        {
            this.s3Client = this.CreateS3Client();
            this.logger.LogInformation(
                "Cloud storage enabled. ServiceUrl: {ServiceUrl}, Bucket: {Bucket}",
                this.settings.ServiceUrl,
                this.settings.BucketName);
        }
        else
        {
            this.logger.LogInformation("Cloud storage is disabled. Using database storage.");
        }
    }

    /// <inheritdoc/>
    public bool IsEnabled => this.settings.Enabled;

    /// <inheritdoc/>
    public async Task<Result<string>> UploadAsync(
        byte[] data,
        string key,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentType);

        if (!this.settings.Enabled || this.s3Client is null)
        {
            return new Error("CloudStorage.Disabled", "Cloud storage is not enabled.");
        }

        try
        {
            this.logger.LogDebug(
                "Uploading {Size} bytes to {Bucket}/{Key}",
                data.Length,
                this.settings.BucketName,
                key);

            using var stream = new MemoryStream(data);
            var request = new PutObjectRequest
            {
                BucketName = this.settings.BucketName,
                Key = key,
                InputStream = stream,
                ContentType = contentType,
            };

            var response = await this.s3Client.PutObjectAsync(request, cancellationToken)
                .ConfigureAwait(false);

            this.logger.LogInformation(
                "Successfully uploaded {Size} bytes to {Bucket}/{Key}",
                data.Length,
                this.settings.BucketName,
                key);

            return Result<string>.Success(key);
        }
        catch (AmazonS3Exception ex)
        {
            this.logger.LogError(
                ex,
                "S3 error uploading to {Bucket}/{Key}: {ErrorCode} - {Message}",
                this.settings.BucketName,
                key,
                ex.ErrorCode,
                ex.Message);

            return new Error(
                $"CloudStorage.S3Error.{ex.ErrorCode}",
                $"Failed to upload to cloud storage: {ex.Message}");
        }
        catch (Exception ex)
        {
            this.logger.LogError(
                ex,
                "Error uploading to {Bucket}/{Key}",
                this.settings.BucketName,
                key);

            return new Error(
                "CloudStorage.UploadFailed",
                $"Failed to upload to cloud storage: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<Result<byte[]>> DownloadAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        if (!this.settings.Enabled || this.s3Client is null)
        {
            return new Error("CloudStorage.Disabled", "Cloud storage is not enabled.");
        }

        try
        {
            this.logger.LogDebug(
                "Downloading from {Bucket}/{Key}",
                this.settings.BucketName,
                key);

            var request = new GetObjectRequest
            {
                BucketName = this.settings.BucketName,
                Key = key,
            };

            using var response = await this.s3Client.GetObjectAsync(request, cancellationToken)
                .ConfigureAwait(false);

            using var memoryStream = new MemoryStream();
            await response.ResponseStream.CopyToAsync(memoryStream, cancellationToken)
                .ConfigureAwait(false);

            var data = memoryStream.ToArray();

            this.logger.LogInformation(
                "Successfully downloaded {Size} bytes from {Bucket}/{Key}",
                data.Length,
                this.settings.BucketName,
                key);

            return Result<byte[]>.Success(data);
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            this.logger.LogWarning(
                "Object not found: {Bucket}/{Key}",
                this.settings.BucketName,
                key);

            return new Error(
                "CloudStorage.NotFound",
                $"Object not found in cloud storage: {key}");
        }
        catch (AmazonS3Exception ex)
        {
            this.logger.LogError(
                ex,
                "S3 error downloading from {Bucket}/{Key}: {ErrorCode} - {Message}",
                this.settings.BucketName,
                key,
                ex.ErrorCode,
                ex.Message);

            return new Error(
                $"CloudStorage.S3Error.{ex.ErrorCode}",
                $"Failed to download from cloud storage: {ex.Message}");
        }
        catch (Exception ex)
        {
            this.logger.LogError(
                ex,
                "Error downloading from {Bucket}/{Key}",
                this.settings.BucketName,
                key);

            return new Error(
                "CloudStorage.DownloadFailed",
                $"Failed to download from cloud storage: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<Result> DeleteAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        if (!this.settings.Enabled || this.s3Client is null)
        {
            return new Error("CloudStorage.Disabled", "Cloud storage is not enabled.");
        }

        try
        {
            this.logger.LogDebug(
                "Deleting from {Bucket}/{Key}",
                this.settings.BucketName,
                key);

            var request = new DeleteObjectRequest
            {
                BucketName = this.settings.BucketName,
                Key = key,
            };

            await this.s3Client.DeleteObjectAsync(request, cancellationToken)
                .ConfigureAwait(false);

            this.logger.LogInformation(
                "Successfully deleted {Bucket}/{Key}",
                this.settings.BucketName,
                key);

            return Result.Success();
        }
        catch (AmazonS3Exception ex)
        {
            this.logger.LogError(
                ex,
                "S3 error deleting from {Bucket}/{Key}: {ErrorCode} - {Message}",
                this.settings.BucketName,
                key,
                ex.ErrorCode,
                ex.Message);

            return new Error(
                $"CloudStorage.S3Error.{ex.ErrorCode}",
                $"Failed to delete from cloud storage: {ex.Message}");
        }
        catch (Exception ex)
        {
            this.logger.LogError(
                ex,
                "Error deleting from {Bucket}/{Key}",
                this.settings.BucketName,
                key);

            return new Error(
                "CloudStorage.DeleteFailed",
                $"Failed to delete from cloud storage: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes managed resources.
    /// </summary>
    /// <param name="disposing">Whether to dispose managed resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!this.disposed)
        {
            if (disposing)
            {
                this.s3Client?.Dispose();
            }

            this.disposed = true;
        }
    }

    private IAmazonS3 CreateS3Client()
    {
        var config = new AmazonS3Config
        {
            ServiceURL = this.settings.ServiceUrl,
            ForcePathStyle = this.settings.ForcePathStyle,
        };

        // Only set RegionEndpoint if not using a custom service URL
        if (string.IsNullOrEmpty(this.settings.ServiceUrl) ||
            this.settings.ServiceUrl.Contains("amazonaws.com", StringComparison.OrdinalIgnoreCase))
        {
            config.RegionEndpoint = RegionEndpoint.GetBySystemName(this.settings.Region);
        }

        return new AmazonS3Client(
            this.settings.AccessKey,
            this.settings.SecretKey,
            config);
    }
}
