// <copyright file="OpenTelemetrySettings.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

namespace FileConversionApi.Infrastructure.Options;

/// <summary>
/// Configuration settings for OpenTelemetry tracing.
/// </summary>
public class OpenTelemetrySettings
{
    /// <summary>
    /// The configuration section name.
    /// </summary>
    public const string SectionName = "OpenTelemetry";

    /// <summary>
    /// Gets or sets a value indicating whether OpenTelemetry tracing is enabled.
    /// </summary>
    public bool EnableTracing { get; set; } = true;

    /// <summary>
    /// Gets or sets the service name for tracing.
    /// </summary>
    public string ServiceName { get; set; } = "FileConversionApi";

    /// <summary>
    /// Gets or sets the service version for tracing.
    /// </summary>
    public string? ServiceVersion { get; set; }

    /// <summary>
    /// Gets or sets the OTLP exporter endpoint (e.g., http://localhost:4317 for Jaeger).
    /// </summary>
    public string? OtlpEndpoint { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to export traces to console (useful for development).
    /// </summary>
    public bool ExportToConsole { get; set; }

    /// <summary>
    /// Gets or sets the sampling ratio (0.0 to 1.0). 1.0 means all traces are sampled.
    /// </summary>
    public double SamplingRatio { get; set; } = 1.0;
}
