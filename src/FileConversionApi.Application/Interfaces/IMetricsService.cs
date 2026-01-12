// <copyright file="IMetricsService.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

namespace FileConversionApi.Application.Interfaces;

/// <summary>
/// Interface for recording application metrics.
/// </summary>
public interface IMetricsService
{
    /// <summary>
    /// Records that a conversion job has started.
    /// </summary>
    /// <param name="sourceFormat">The source format.</param>
    /// <param name="targetFormat">The target format.</param>
    void RecordConversionStarted(string sourceFormat, string targetFormat);

    /// <summary>
    /// Records that a conversion job has completed successfully.
    /// </summary>
    /// <param name="sourceFormat">The source format.</param>
    /// <param name="targetFormat">The target format.</param>
    /// <param name="durationSeconds">The duration in seconds.</param>
    void RecordConversionCompleted(string sourceFormat, string targetFormat, double durationSeconds);

    /// <summary>
    /// Records that a conversion job has failed.
    /// </summary>
    /// <param name="sourceFormat">The source format.</param>
    /// <param name="targetFormat">The target format.</param>
    void RecordConversionFailed(string sourceFormat, string targetFormat);
}
