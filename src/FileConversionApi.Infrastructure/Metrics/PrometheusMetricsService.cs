// <copyright file="PrometheusMetricsService.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Application.Interfaces;

using Prometheus;

namespace FileConversionApi.Infrastructure.Metrics;

/// <summary>
/// Prometheus implementation of the metrics service.
/// </summary>
public class PrometheusMetricsService : IMetricsService
{
    private static readonly Counter ConversionJobsTotal = Prometheus.Metrics.CreateCounter(
        "conversion_jobs_total",
        "Total conversion jobs processed",
        new CounterConfiguration
        {
            LabelNames = ["source_format", "target_format", "status"],
        });

    private static readonly Histogram ConversionDuration = Prometheus.Metrics.CreateHistogram(
        "conversion_job_duration_seconds",
        "Conversion job duration in seconds",
        new HistogramConfiguration
        {
            LabelNames = ["source_format", "target_format"],
            Buckets = [0.5, 1, 2, 5, 10, 30, 60],
        });

    private static readonly Gauge ActiveJobs = Prometheus.Metrics.CreateGauge(
        "conversion_jobs_active",
        "Currently processing conversion jobs",
        new GaugeConfiguration
        {
            LabelNames = ["source_format", "target_format"],
        });

    /// <inheritdoc/>
    public void RecordConversionStarted(string sourceFormat, string targetFormat)
    {
        ActiveJobs.WithLabels(sourceFormat, targetFormat).Inc();
    }

    /// <inheritdoc/>
    public void RecordConversionCompleted(string sourceFormat, string targetFormat, double durationSeconds)
    {
        ActiveJobs.WithLabels(sourceFormat, targetFormat).Dec();
        ConversionJobsTotal.WithLabels(sourceFormat, targetFormat, "completed").Inc();
        ConversionDuration.WithLabels(sourceFormat, targetFormat).Observe(durationSeconds);
    }

    /// <inheritdoc/>
    public void RecordConversionFailed(string sourceFormat, string targetFormat)
    {
        ActiveJobs.WithLabels(sourceFormat, targetFormat).Dec();
        ConversionJobsTotal.WithLabels(sourceFormat, targetFormat, "failed").Inc();
    }
}
