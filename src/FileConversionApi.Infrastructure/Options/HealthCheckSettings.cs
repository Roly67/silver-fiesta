// <copyright file="HealthCheckSettings.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

namespace FileConversionApi.Infrastructure.Options;

/// <summary>
/// Configuration settings for health checks.
/// </summary>
public class HealthCheckSettings
{
    /// <summary>
    /// The configuration section name.
    /// </summary>
    public const string SectionName = "HealthChecks";

    /// <summary>
    /// Gets or sets the minimum disk space required in megabytes.
    /// </summary>
    public int DiskSpaceMinimumMB { get; set; } = 100;

    /// <summary>
    /// Gets or sets the timeout for Chromium health check in seconds.
    /// </summary>
    public int ChromiumTimeoutSeconds { get; set; } = 30;
}
