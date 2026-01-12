// <copyright file="MetricsSettings.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

namespace FileConversionApi.Infrastructure.Options;

/// <summary>
/// Configuration settings for Prometheus metrics.
/// </summary>
public class MetricsSettings
{
    /// <summary>
    /// The configuration section name.
    /// </summary>
    public const string SectionName = "Metrics";

    /// <summary>
    /// Gets or sets a value indicating whether metrics collection is enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the endpoint path for the metrics endpoint.
    /// </summary>
    public string Endpoint { get; set; } = "/metrics";

    /// <summary>
    /// Gets or sets a value indicating whether to include HTTP request metrics.
    /// </summary>
    public bool IncludeHttpMetrics { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to include process metrics.
    /// </summary>
    public bool IncludeProcessMetrics { get; set; } = true;
}
