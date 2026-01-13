// <copyright file="RateLimitTier.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

namespace FileConversionApi.Domain.Enums;

/// <summary>
/// Represents the rate limiting tier for a user.
/// </summary>
public enum RateLimitTier
{
    /// <summary>
    /// Free tier with basic rate limits.
    /// </summary>
    Free = 0,

    /// <summary>
    /// Basic paid tier with increased limits.
    /// </summary>
    Basic = 1,

    /// <summary>
    /// Premium tier with higher limits.
    /// </summary>
    Premium = 2,

    /// <summary>
    /// Unlimited tier with no rate restrictions.
    /// </summary>
    Unlimited = 3,
}
