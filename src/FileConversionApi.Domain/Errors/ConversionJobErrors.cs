// <copyright file="ConversionJobErrors.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Domain.Primitives;

namespace FileConversionApi.Domain.Errors;

/// <summary>
/// Domain errors for conversion jobs.
/// </summary>
public static class ConversionJobErrors
{
    /// <summary>
    /// Conversion job not completed error.
    /// </summary>
    public static readonly Error NotCompleted =
        new("ConversionJob.NotCompleted", "The conversion job has not completed yet.");

    /// <summary>
    /// Job already processing error.
    /// </summary>
    public static readonly Error AlreadyProcessing =
        new("ConversionJob.AlreadyProcessing", "The conversion job is already being processed.");

    /// <summary>
    /// No output available error.
    /// </summary>
    public static readonly Error NoOutputAvailable =
        new("ConversionJob.NoOutputAvailable", "No output is available for this conversion job.");

    /// <summary>
    /// Conversion job not found error.
    /// </summary>
    /// <param name="id">The job identifier.</param>
    /// <returns>The error.</returns>
    public static Error NotFound(Guid id) =>
        new("ConversionJob.NotFound", $"Conversion job with ID '{id}' was not found.");

    /// <summary>
    /// Conversion failed error.
    /// </summary>
    /// <param name="message">The failure message.</param>
    /// <returns>The error.</returns>
    public static Error ConversionFailed(string message) =>
        new("ConversionJob.ConversionFailed", $"Conversion failed: {message}");

    /// <summary>
    /// Unsupported conversion error.
    /// </summary>
    /// <param name="sourceFormat">The source format.</param>
    /// <param name="targetFormat">The target format.</param>
    /// <returns>The error.</returns>
    public static Error UnsupportedConversion(string sourceFormat, string targetFormat) =>
        new("ConversionJob.UnsupportedConversion", $"Conversion from '{sourceFormat}' to '{targetFormat}' is not supported.");
}
