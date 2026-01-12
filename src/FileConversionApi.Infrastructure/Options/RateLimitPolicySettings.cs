// <copyright file="RateLimitPolicySettings.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

namespace FileConversionApi.Infrastructure.Options;

/// <summary>
/// Settings for a rate limit policy.
/// </summary>
public class RateLimitPolicySettings
{
    /// <summary>
    /// Gets or sets the maximum number of requests permitted in the time window.
    /// </summary>
    public int PermitLimit { get; set; } = 100;

    /// <summary>
    /// Gets or sets the time window duration in minutes.
    /// </summary>
    public int WindowMinutes { get; set; } = 60;
}
