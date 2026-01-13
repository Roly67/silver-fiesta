// <copyright file="RateLimitTierSettings.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

namespace FileConversionApi.Infrastructure.Options;

/// <summary>
/// Settings for a rate limit tier.
/// </summary>
public class RateLimitTierSettings
{
    /// <summary>
    /// Gets or sets the standard policy settings for this tier.
    /// </summary>
    public RateLimitPolicySettings StandardPolicy { get; set; } = new();

    /// <summary>
    /// Gets or sets the conversion policy settings for this tier.
    /// </summary>
    public RateLimitPolicySettings ConversionPolicy { get; set; } = new();
}
