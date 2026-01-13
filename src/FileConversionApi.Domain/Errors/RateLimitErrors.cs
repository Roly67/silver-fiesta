// <copyright file="RateLimitErrors.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Domain.Primitives;
using FileConversionApi.Domain.ValueObjects;

namespace FileConversionApi.Domain.Errors;

/// <summary>
/// Contains error definitions related to rate limiting.
/// </summary>
public static class RateLimitErrors
{
    /// <summary>
    /// Gets an error indicating the rate limit settings were not found for a user.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <returns>An error indicating settings not found.</returns>
    public static Error SettingsNotFound(UserId userId) =>
        Error.NotFound(
            "RateLimit.SettingsNotFound",
            $"Rate limit settings not found for user '{userId.Value}'.");

    /// <summary>
    /// Gets an error indicating an invalid rate limit tier was specified.
    /// </summary>
    /// <param name="tier">The invalid tier value.</param>
    /// <returns>An error indicating invalid tier.</returns>
    public static Error InvalidTier(string tier) =>
        Error.Validation(
            "RateLimit.InvalidTier",
            $"'{tier}' is not a valid rate limit tier. Valid values are: Free, Basic, Premium, Unlimited.");

    /// <summary>
    /// Gets an error indicating an invalid policy name was specified.
    /// </summary>
    /// <param name="policyName">The invalid policy name.</param>
    /// <returns>An error indicating invalid policy.</returns>
    public static Error InvalidPolicyName(string policyName) =>
        Error.Validation(
            "RateLimit.InvalidPolicyName",
            $"'{policyName}' is not a valid policy name. Valid values are: standard, conversion.");

    /// <summary>
    /// Gets an error indicating an invalid override value was specified.
    /// </summary>
    /// <param name="reason">The reason for the error.</param>
    /// <returns>An error indicating invalid override.</returns>
    public static Error InvalidOverride(string reason) =>
        Error.Validation(
            "RateLimit.InvalidOverride",
            reason);
}
