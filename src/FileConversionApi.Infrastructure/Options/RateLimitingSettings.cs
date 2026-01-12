// <copyright file="RateLimitingSettings.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

namespace FileConversionApi.Infrastructure.Options;

/// <summary>
/// Configuration settings for rate limiting.
/// </summary>
public class RateLimitingSettings
{
    /// <summary>
    /// The configuration section name.
    /// </summary>
    public const string SectionName = "RateLimiting";

    /// <summary>
    /// Gets or sets a value indicating whether rate limiting is enabled.
    /// </summary>
    public bool EnableRateLimiting { get; set; } = true;

    /// <summary>
    /// Gets or sets the standard policy settings for general API endpoints.
    /// </summary>
    public RateLimitPolicySettings StandardPolicy { get; set; } = new();

    /// <summary>
    /// Gets or sets the conversion policy settings for resource-intensive endpoints.
    /// </summary>
    public RateLimitPolicySettings ConversionPolicy { get; set; } = new();

    /// <summary>
    /// Gets or sets the auth policy settings for authentication endpoints.
    /// </summary>
    public RateLimitPolicySettings AuthPolicy { get; set; } = new();
}
