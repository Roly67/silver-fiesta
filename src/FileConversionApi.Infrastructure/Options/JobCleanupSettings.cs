// <copyright file="JobCleanupSettings.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

namespace FileConversionApi.Infrastructure.Options;

/// <summary>
/// Configuration settings for the job cleanup background service.
/// </summary>
public class JobCleanupSettings
{
    /// <summary>
    /// The configuration section name.
    /// </summary>
    public const string SectionName = "JobCleanup";

    /// <summary>
    /// Gets or sets a value indicating whether the cleanup service is enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the interval in minutes between cleanup runs.
    /// </summary>
    public int RunIntervalMinutes { get; set; } = 60;

    /// <summary>
    /// Gets or sets the number of days to retain completed jobs.
    /// </summary>
    public int CompletedJobRetentionDays { get; set; } = 7;

    /// <summary>
    /// Gets or sets the number of days to retain failed jobs.
    /// </summary>
    public int FailedJobRetentionDays { get; set; } = 30;

    /// <summary>
    /// Gets or sets the maximum number of jobs to delete per cleanup run.
    /// </summary>
    public int BatchSize { get; set; } = 100;
}
