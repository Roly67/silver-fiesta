// <copyright file="ConversionJob.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Domain.Enums;
using FileConversionApi.Domain.ValueObjects;

namespace FileConversionApi.Domain.Entities;

/// <summary>
/// Represents a file conversion job.
/// </summary>
public class ConversionJob
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConversionJob"/> class.
    /// Required for EF Core.
    /// </summary>
    private ConversionJob()
    {
    }

    /// <summary>
    /// Gets the conversion job identifier.
    /// </summary>
    public ConversionJobId Id { get; private set; }

    /// <summary>
    /// Gets the user identifier.
    /// </summary>
    public UserId UserId { get; private set; }

    /// <summary>
    /// Gets the source format.
    /// </summary>
    public string SourceFormat { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the target format.
    /// </summary>
    public string TargetFormat { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the conversion status.
    /// </summary>
    public ConversionStatus Status { get; private set; }

    /// <summary>
    /// Gets the input file name.
    /// </summary>
    public string InputFileName { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the output file name.
    /// </summary>
    public string? OutputFileName { get; private set; }

    /// <summary>
    /// Gets the output data.
    /// </summary>
    public byte[]? OutputData { get; private set; }

    /// <summary>
    /// Gets the error message if the conversion failed.
    /// </summary>
    public string? ErrorMessage { get; private set; }

    /// <summary>
    /// Gets the date and time when the job was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>
    /// Gets the date and time when the job was completed.
    /// </summary>
    public DateTimeOffset? CompletedAt { get; private set; }

    /// <summary>
    /// Gets the user who owns this job.
    /// </summary>
    public User? User { get; private set; }

    /// <summary>
    /// Gets the webhook URL to notify when the job completes.
    /// </summary>
    public string? WebhookUrl { get; private set; }

    /// <summary>
    /// Gets the storage location for the output data.
    /// </summary>
    public StorageLocation StorageLocation { get; private set; } = StorageLocation.Database;

    /// <summary>
    /// Gets the cloud storage key when output is stored in cloud storage.
    /// </summary>
    public string? CloudStorageKey { get; private set; }

    /// <summary>
    /// Creates a new conversion job.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="sourceFormat">The source format.</param>
    /// <param name="targetFormat">The target format.</param>
    /// <param name="inputFileName">The input file name.</param>
    /// <param name="webhookUrl">The optional webhook URL to notify on completion.</param>
    /// <returns>A new <see cref="ConversionJob"/>.</returns>
    public static ConversionJob Create(
        UserId userId,
        string sourceFormat,
        string targetFormat,
        string inputFileName,
        string? webhookUrl = null)
    {
        return new ConversionJob
        {
            Id = ConversionJobId.New(),
            UserId = userId,
            SourceFormat = sourceFormat.ToLowerInvariant(),
            TargetFormat = targetFormat.ToLowerInvariant(),
            InputFileName = inputFileName,
            Status = ConversionStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow,
            WebhookUrl = webhookUrl,
        };
    }

    /// <summary>
    /// Marks the job as processing.
    /// </summary>
    public void MarkAsProcessing()
    {
        this.Status = ConversionStatus.Processing;
    }

    /// <summary>
    /// Marks the job as completed with the output data stored in the database.
    /// </summary>
    /// <param name="outputFileName">The output file name.</param>
    /// <param name="outputData">The output data.</param>
    public void MarkAsCompleted(string outputFileName, byte[] outputData)
    {
        this.Status = ConversionStatus.Completed;
        this.OutputFileName = outputFileName;
        this.OutputData = outputData;
        this.StorageLocation = StorageLocation.Database;
        this.CloudStorageKey = null;
        this.CompletedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Marks the job as completed with the output data stored in cloud storage.
    /// </summary>
    /// <param name="outputFileName">The output file name.</param>
    /// <param name="cloudStorageKey">The cloud storage key.</param>
    public void MarkAsCompletedWithCloudStorage(string outputFileName, string cloudStorageKey)
    {
        this.Status = ConversionStatus.Completed;
        this.OutputFileName = outputFileName;
        this.OutputData = null;
        this.StorageLocation = StorageLocation.CloudStorage;
        this.CloudStorageKey = cloudStorageKey;
        this.CompletedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Marks the job as failed with an error message.
    /// </summary>
    /// <param name="errorMessage">The error message.</param>
    public void MarkAsFailed(string errorMessage)
    {
        this.Status = ConversionStatus.Failed;
        this.ErrorMessage = errorMessage;
        this.CompletedAt = DateTimeOffset.UtcNow;
    }
}
