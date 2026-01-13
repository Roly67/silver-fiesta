// <copyright file="CloudStorageSettings.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

namespace FileConversionApi.Infrastructure.Options;

/// <summary>
/// Configuration settings for S3-compatible cloud storage.
/// </summary>
public class CloudStorageSettings
{
    /// <summary>
    /// The configuration section name.
    /// </summary>
    public const string SectionName = "CloudStorage";

    /// <summary>
    /// Gets or sets a value indicating whether cloud storage is enabled.
    /// When disabled, outputs are stored in the database.
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// Gets or sets the S3-compatible service URL.
    /// Examples: https://s3.amazonaws.com, http://localhost:9000 (MinIO).
    /// </summary>
    public string ServiceUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the bucket name for storing conversion outputs.
    /// </summary>
    public string BucketName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the access key for authentication.
    /// </summary>
    public string AccessKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the secret key for authentication.
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the AWS region. Default is us-east-1.
    /// </summary>
    public string Region { get; set; } = "us-east-1";

    /// <summary>
    /// Gets or sets a value indicating whether to use path-style addressing.
    /// Required for MinIO and some S3-compatible services.
    /// </summary>
    public bool ForcePathStyle { get; set; } = false;

    /// <summary>
    /// Gets or sets the presigned URL expiration time in minutes.
    /// </summary>
    public int PresignedUrlExpirationMinutes { get; set; } = 60;
}
