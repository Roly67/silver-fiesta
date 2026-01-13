// <copyright file="IUserRateLimitService.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Domain.Entities;
using FileConversionApi.Domain.Enums;
using FileConversionApi.Domain.Primitives;
using FileConversionApi.Domain.ValueObjects;

namespace FileConversionApi.Application.Interfaces;

/// <summary>
/// Service interface for managing user rate limit settings.
/// </summary>
public interface IUserRateLimitService
{
    /// <summary>
    /// Gets the effective rate limits for a user and policy.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="policyName">The policy name (standard or conversion).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The effective rate limits.</returns>
    Task<UserEffectiveRateLimits> GetEffectiveLimitsAsync(
        UserId userId,
        string policyName,
        CancellationToken cancellationToken);

    /// <summary>
    /// Checks if a user should bypass rate limiting.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the user should bypass rate limits.</returns>
    Task<bool> ShouldBypassRateLimitAsync(UserId userId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets or creates rate limit settings for a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The user's rate limit settings.</returns>
    Task<UserRateLimitSettings> GetOrCreateSettingsAsync(UserId userId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the rate limit settings for a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The user's rate limit settings if found.</returns>
    Task<Result<UserRateLimitSettings>> GetSettingsAsync(UserId userId, CancellationToken cancellationToken);

    /// <summary>
    /// Updates the user's rate limit tier.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="tier">The new tier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    Task<Result> UpdateTierAsync(UserId userId, RateLimitTier tier, CancellationToken cancellationToken);

    /// <summary>
    /// Sets a per-user override for a policy.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="policyName">The policy name (standard or conversion).</param>
    /// <param name="permitLimit">The permit limit override, or null to clear.</param>
    /// <param name="windowMinutes">The window minutes override, or null to clear.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    Task<Result> SetPolicyOverrideAsync(
        UserId userId,
        string policyName,
        int? permitLimit,
        int? windowMinutes,
        CancellationToken cancellationToken);

    /// <summary>
    /// Clears all per-user overrides.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    Task<Result> ClearOverridesAsync(UserId userId, CancellationToken cancellationToken);

    /// <summary>
    /// Invalidates the cached settings for a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    void InvalidateCache(UserId userId);
}

/// <summary>
/// Represents the effective rate limits for a user.
/// </summary>
public record UserEffectiveRateLimits
{
    /// <summary>
    /// Gets the permit limit.
    /// </summary>
    public required int PermitLimit { get; init; }

    /// <summary>
    /// Gets the time window.
    /// </summary>
    public required TimeSpan Window { get; init; }

    /// <summary>
    /// Gets a value indicating whether rate limiting should be bypassed.
    /// </summary>
    public required bool BypassRateLimiting { get; init; }

    /// <summary>
    /// Gets the source of the limits (Admin, Tier, or Override).
    /// </summary>
    public required string Source { get; init; }
}
