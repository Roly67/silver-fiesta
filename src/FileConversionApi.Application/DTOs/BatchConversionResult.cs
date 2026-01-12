// <copyright file="BatchConversionResult.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

namespace FileConversionApi.Application.DTOs;

/// <summary>
/// Represents the result of a batch conversion request.
/// </summary>
public class BatchConversionResult
{
    /// <summary>
    /// Gets or sets the total number of items in the batch.
    /// </summary>
    public int TotalItems { get; set; }

    /// <summary>
    /// Gets or sets the number of successfully queued items.
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// Gets or sets the number of failed items.
    /// </summary>
    public int FailureCount { get; set; }

    /// <summary>
    /// Gets or sets the results for each item in the batch.
    /// </summary>
    public List<BatchItemResult> Results { get; set; } = [];
}
