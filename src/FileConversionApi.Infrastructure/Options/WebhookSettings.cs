// <copyright file="WebhookSettings.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

namespace FileConversionApi.Infrastructure.Options;

/// <summary>
/// Configuration settings for webhook notifications.
/// </summary>
public class WebhookSettings
{
    /// <summary>
    /// The configuration section name.
    /// </summary>
    public const string SectionName = "WebhookSettings";

    /// <summary>
    /// Gets or sets the HTTP request timeout in seconds.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Gets or sets the maximum number of retry attempts.
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Gets or sets the delay between retries in milliseconds.
    /// </summary>
    public int RetryDelayMilliseconds { get; set; } = 1000;
}
