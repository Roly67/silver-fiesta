// <copyright file="BatchItemResult.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

namespace FileConversionApi.Application.DTOs;

/// <summary>
/// Represents the result of a single item in a batch conversion.
/// </summary>
public class BatchItemResult
{
    /// <summary>
    /// Gets or sets the index of the item in the original request (0-based).
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the item was successfully queued.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the conversion job details (if successful).
    /// </summary>
    public ConversionJobDto? Job { get; set; }

    /// <summary>
    /// Gets or sets the error code (if failed).
    /// </summary>
    public string? ErrorCode { get; set; }

    /// <summary>
    /// Gets or sets the error message (if failed).
    /// </summary>
    public string? ErrorMessage { get; set; }
}
