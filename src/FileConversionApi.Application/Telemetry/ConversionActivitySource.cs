// <copyright file="ConversionActivitySource.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using System.Diagnostics;

using OpenTelemetry.Trace;

namespace FileConversionApi.Application.Telemetry;

/// <summary>
/// Provides activity sources for conversion operations tracing.
/// </summary>
public static class ConversionActivitySource
{
    /// <summary>
    /// The name of the activity source.
    /// </summary>
    public const string Name = "FileConversionApi.Conversions";

    /// <summary>
    /// Gets the activity source for conversion operations.
    /// </summary>
    public static ActivitySource Source { get; } = new(Name, "1.0.0");

    /// <summary>
    /// Starts a new activity for a conversion operation.
    /// </summary>
    /// <param name="conversionType">The type of conversion (e.g., "html-to-pdf").</param>
    /// <param name="jobId">The conversion job ID.</param>
    /// <returns>The started activity, or null if no listener is registered.</returns>
    public static Activity? StartConversion(string conversionType, Guid jobId)
    {
        var activity = Source.StartActivity($"conversion.{conversionType}", ActivityKind.Internal);

        if (activity is not null)
        {
            activity.SetTag("conversion.type", conversionType);
            activity.SetTag("conversion.job_id", jobId.ToString());
        }

        return activity;
    }

    /// <summary>
    /// Records conversion completion on an activity.
    /// </summary>
    /// <param name="activity">The activity to update.</param>
    /// <param name="outputSize">The size of the output in bytes.</param>
    /// <param name="durationMs">The duration in milliseconds.</param>
    public static void RecordConversionCompleted(Activity? activity, long outputSize, double durationMs)
    {
        if (activity is null)
        {
            return;
        }

        activity.SetTag("conversion.output_size_bytes", outputSize);
        activity.SetTag("conversion.duration_ms", durationMs);
        activity.SetStatus(ActivityStatusCode.Ok);
    }

    /// <summary>
    /// Records conversion failure on an activity.
    /// </summary>
    /// <param name="activity">The activity to update.</param>
    /// <param name="errorMessage">The error message.</param>
    /// <param name="exception">The optional exception that caused the failure.</param>
    public static void RecordConversionFailed(Activity? activity, string errorMessage, Exception? exception = null)
    {
        if (activity is null)
        {
            return;
        }

        activity.SetTag("conversion.error", errorMessage);
        activity.SetStatus(ActivityStatusCode.Error, errorMessage);

        if (exception is not null)
        {
            activity.AddException(exception);
        }
    }

    /// <summary>
    /// Starts a new activity for a PDF operation.
    /// </summary>
    /// <param name="operationType">The type of PDF operation (e.g., "merge", "split", "watermark").</param>
    /// <param name="jobId">The job ID.</param>
    /// <returns>The started activity, or null if no listener is registered.</returns>
    public static Activity? StartPdfOperation(string operationType, Guid jobId)
    {
        var activity = Source.StartActivity($"pdf.{operationType}", ActivityKind.Internal);

        if (activity is not null)
        {
            activity.SetTag("pdf.operation", operationType);
            activity.SetTag("pdf.job_id", jobId.ToString());
        }

        return activity;
    }

    /// <summary>
    /// Starts a new activity for a batch conversion.
    /// </summary>
    /// <param name="itemCount">The number of items in the batch.</param>
    /// <returns>The started activity, or null if no listener is registered.</returns>
    public static Activity? StartBatchConversion(int itemCount)
    {
        var activity = Source.StartActivity("conversion.batch", ActivityKind.Internal);

        if (activity is not null)
        {
            activity.SetTag("batch.item_count", itemCount);
        }

        return activity;
    }

    /// <summary>
    /// Records batch conversion completion.
    /// </summary>
    /// <param name="activity">The activity to update.</param>
    /// <param name="successCount">The number of successful conversions.</param>
    /// <param name="failureCount">The number of failed conversions.</param>
    public static void RecordBatchCompleted(Activity? activity, int successCount, int failureCount)
    {
        if (activity is null)
        {
            return;
        }

        activity.SetTag("batch.success_count", successCount);
        activity.SetTag("batch.failure_count", failureCount);
        activity.SetStatus(failureCount == 0 ? ActivityStatusCode.Ok : ActivityStatusCode.Error);
    }
}
