// <copyright file="SetUserRateLimitTierRequest.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

namespace FileConversionApi.Api.Requests;

/// <summary>
/// Request to set a user's rate limit tier.
/// </summary>
public record SetUserRateLimitTierRequest
{
    /// <summary>
    /// Gets the tier name (Free, Basic, Premium, Unlimited).
    /// </summary>
    public required string Tier { get; init; }
}
