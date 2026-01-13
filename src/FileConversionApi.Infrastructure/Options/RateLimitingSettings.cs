// <copyright file="RateLimitingSettings.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Domain.Enums;

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
    /// Gets or sets a value indicating whether admin users bypass rate limits.
    /// </summary>
    public bool ExemptAdmins { get; set; } = true;

    /// <summary>
    /// Gets or sets the cache duration for user rate limit settings in seconds.
    /// </summary>
    public int UserSettingsCacheSeconds { get; set; } = 300;

    /// <summary>
    /// Gets or sets the standard policy settings for general API endpoints (fallback for anonymous users).
    /// </summary>
    public RateLimitPolicySettings StandardPolicy { get; set; } = new();

    /// <summary>
    /// Gets or sets the conversion policy settings for resource-intensive endpoints (fallback for anonymous users).
    /// </summary>
    public RateLimitPolicySettings ConversionPolicy { get; set; } = new();

    /// <summary>
    /// Gets or sets the auth policy settings for authentication endpoints.
    /// </summary>
    public RateLimitPolicySettings AuthPolicy { get; set; } = new();

    /// <summary>
    /// Gets or sets the tier configurations.
    /// </summary>
    public Dictionary<string, RateLimitTierSettings> Tiers { get; set; } = new()
    {
        ["Free"] = new RateLimitTierSettings
        {
            StandardPolicy = new RateLimitPolicySettings { PermitLimit = 100, WindowMinutes = 60 },
            ConversionPolicy = new RateLimitPolicySettings { PermitLimit = 20, WindowMinutes = 60 },
        },
        ["Basic"] = new RateLimitTierSettings
        {
            StandardPolicy = new RateLimitPolicySettings { PermitLimit = 500, WindowMinutes = 60 },
            ConversionPolicy = new RateLimitPolicySettings { PermitLimit = 100, WindowMinutes = 60 },
        },
        ["Premium"] = new RateLimitTierSettings
        {
            StandardPolicy = new RateLimitPolicySettings { PermitLimit = 2000, WindowMinutes = 60 },
            ConversionPolicy = new RateLimitPolicySettings { PermitLimit = 500, WindowMinutes = 60 },
        },
        ["Unlimited"] = new RateLimitTierSettings
        {
            StandardPolicy = new RateLimitPolicySettings { PermitLimit = 100000, WindowMinutes = 60 },
            ConversionPolicy = new RateLimitPolicySettings { PermitLimit = 10000, WindowMinutes = 60 },
        },
    };

    /// <summary>
    /// Gets the tier settings for a specific tier.
    /// </summary>
    /// <param name="tier">The rate limit tier.</param>
    /// <returns>The tier settings, or default Free tier if not found.</returns>
    public RateLimitTierSettings GetTierSettings(RateLimitTier tier)
    {
        var tierName = tier.ToString();
        return this.Tiers.TryGetValue(tierName, out var settings)
            ? settings
            : this.Tiers["Free"];
    }
}
