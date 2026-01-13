// <copyright file="UserRateLimitSettings.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Domain.Enums;
using FileConversionApi.Domain.ValueObjects;

namespace FileConversionApi.Domain.Entities;

/// <summary>
/// Represents a user's rate limiting settings including tier and optional overrides.
/// </summary>
public class UserRateLimitSettings
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UserRateLimitSettings"/> class.
    /// </summary>
    /// <remarks>Required by EF Core.</remarks>
    private UserRateLimitSettings()
    {
    }

    /// <summary>
    /// Gets the unique identifier for this settings record.
    /// </summary>
    public UserRateLimitSettingsId Id { get; private set; }

    /// <summary>
    /// Gets the identifier of the user these settings belong to.
    /// </summary>
    public UserId UserId { get; private set; }

    /// <summary>
    /// Gets the rate limiting tier for the user.
    /// </summary>
    public RateLimitTier Tier { get; private set; }

    /// <summary>
    /// Gets the per-user override for standard policy permit limit.
    /// Null means use tier default.
    /// </summary>
    public int? StandardPolicyPermitLimit { get; private set; }

    /// <summary>
    /// Gets the per-user override for standard policy window in minutes.
    /// Null means use tier default.
    /// </summary>
    public int? StandardPolicyWindowMinutes { get; private set; }

    /// <summary>
    /// Gets the per-user override for conversion policy permit limit.
    /// Null means use tier default.
    /// </summary>
    public int? ConversionPolicyPermitLimit { get; private set; }

    /// <summary>
    /// Gets the per-user override for conversion policy window in minutes.
    /// Null means use tier default.
    /// </summary>
    public int? ConversionPolicyWindowMinutes { get; private set; }

    /// <summary>
    /// Gets the date and time when this record was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>
    /// Gets the date and time when this record was last updated.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the user has any standard policy overrides.
    /// </summary>
    public bool HasStandardPolicyOverride =>
        this.StandardPolicyPermitLimit.HasValue || this.StandardPolicyWindowMinutes.HasValue;

    /// <summary>
    /// Gets a value indicating whether the user has any conversion policy overrides.
    /// </summary>
    public bool HasConversionPolicyOverride =>
        this.ConversionPolicyPermitLimit.HasValue || this.ConversionPolicyWindowMinutes.HasValue;

    /// <summary>
    /// Gets a value indicating whether the user has any overrides.
    /// </summary>
    public bool HasAnyOverride => this.HasStandardPolicyOverride || this.HasConversionPolicyOverride;

    /// <summary>
    /// Creates a new user rate limit settings record.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="tier">The rate limit tier. Defaults to Free.</param>
    /// <returns>A new <see cref="UserRateLimitSettings"/> instance.</returns>
    public static UserRateLimitSettings Create(UserId userId, RateLimitTier tier = RateLimitTier.Free)
    {
        var now = DateTimeOffset.UtcNow;
        return new UserRateLimitSettings
        {
            Id = UserRateLimitSettingsId.New(),
            UserId = userId,
            Tier = tier,
            StandardPolicyPermitLimit = null,
            StandardPolicyWindowMinutes = null,
            ConversionPolicyPermitLimit = null,
            ConversionPolicyWindowMinutes = null,
            CreatedAt = now,
            UpdatedAt = now,
        };
    }

    /// <summary>
    /// Updates the user's rate limit tier.
    /// </summary>
    /// <param name="tier">The new tier.</param>
    public void UpdateTier(RateLimitTier tier)
    {
        this.Tier = tier;
        this.UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Sets a per-user override for the standard policy.
    /// </summary>
    /// <param name="permitLimit">The permit limit override, or null to use tier default.</param>
    /// <param name="windowMinutes">The window minutes override, or null to use tier default.</param>
    public void SetStandardPolicyOverride(int? permitLimit, int? windowMinutes)
    {
        if (permitLimit.HasValue && permitLimit.Value < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(permitLimit), "Permit limit cannot be negative.");
        }

        if (windowMinutes.HasValue && windowMinutes.Value <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(windowMinutes), "Window minutes must be positive.");
        }

        this.StandardPolicyPermitLimit = permitLimit;
        this.StandardPolicyWindowMinutes = windowMinutes;
        this.UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Sets a per-user override for the conversion policy.
    /// </summary>
    /// <param name="permitLimit">The permit limit override, or null to use tier default.</param>
    /// <param name="windowMinutes">The window minutes override, or null to use tier default.</param>
    public void SetConversionPolicyOverride(int? permitLimit, int? windowMinutes)
    {
        if (permitLimit.HasValue && permitLimit.Value < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(permitLimit), "Permit limit cannot be negative.");
        }

        if (windowMinutes.HasValue && windowMinutes.Value <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(windowMinutes), "Window minutes must be positive.");
        }

        this.ConversionPolicyPermitLimit = permitLimit;
        this.ConversionPolicyWindowMinutes = windowMinutes;
        this.UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Clears all per-user overrides, reverting to tier defaults.
    /// </summary>
    public void ClearAllOverrides()
    {
        this.StandardPolicyPermitLimit = null;
        this.StandardPolicyWindowMinutes = null;
        this.ConversionPolicyPermitLimit = null;
        this.ConversionPolicyWindowMinutes = null;
        this.UpdatedAt = DateTimeOffset.UtcNow;
    }
}
