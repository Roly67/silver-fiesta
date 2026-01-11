// <copyright file="ConversionStatus.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

namespace FileConversionApi.Domain.Enums;

/// <summary>
/// Represents the status of a file conversion job.
/// </summary>
public enum ConversionStatus
{
    /// <summary>
    /// The conversion job is pending processing.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// The conversion job is currently being processed.
    /// </summary>
    Processing = 1,

    /// <summary>
    /// The conversion job has completed successfully.
    /// </summary>
    Completed = 2,

    /// <summary>
    /// The conversion job has failed.
    /// </summary>
    Failed = 3,
}
