// <copyright file="UserRateLimitSettingsDto.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Domain.Entities;

namespace FileConversionApi.Application.DTOs;

/// <summary>
/// Data transfer object for user rate limit settings.
/// </summary>
public record UserRateLimitSettingsDto
{
    /// <summary>
    /// Gets the user identifier.
    /// </summary>
    public required Guid UserId { get; init; }

    /// <summary>
    /// Gets the rate limit tier name.
    /// </summary>
    public required string Tier { get; init; }

    /// <summary>
    /// Gets the standard policy settings.
    /// </summary>
    public required RateLimitPolicyDto StandardPolicy { get; init; }

    /// <summary>
    /// Gets the conversion policy settings.
    /// </summary>
    public required RateLimitPolicyDto ConversionPolicy { get; init; }

    /// <summary>
    /// Gets a value indicating whether the user has any overrides.
    /// </summary>
    public required bool HasAnyOverride { get; init; }

    /// <summary>
    /// Gets the date and time when these settings were last updated.
    /// </summary>
    public required DateTimeOffset UpdatedAt { get; init; }

    /// <summary>
    /// Creates a DTO from a domain entity with effective limits.
    /// </summary>
    /// <param name="settings">The user rate limit settings entity.</param>
    /// <param name="standardEffective">The effective standard policy limits.</param>
    /// <param name="conversionEffective">The effective conversion policy limits.</param>
    /// <returns>The DTO.</returns>
    public static UserRateLimitSettingsDto FromEntity(
        UserRateLimitSettings settings,
        (int PermitLimit, int WindowMinutes) standardEffective,
        (int PermitLimit, int WindowMinutes) conversionEffective) =>
        new()
        {
            UserId = settings.UserId.Value,
            Tier = settings.Tier.ToString(),
            StandardPolicy = new RateLimitPolicyDto
            {
                EffectivePermitLimit = standardEffective.PermitLimit,
                EffectiveWindowMinutes = standardEffective.WindowMinutes,
                OverridePermitLimit = settings.StandardPolicyPermitLimit,
                OverrideWindowMinutes = settings.StandardPolicyWindowMinutes,
                Source = settings.HasStandardPolicyOverride ? "Override" : "Tier",
            },
            ConversionPolicy = new RateLimitPolicyDto
            {
                EffectivePermitLimit = conversionEffective.PermitLimit,
                EffectiveWindowMinutes = conversionEffective.WindowMinutes,
                OverridePermitLimit = settings.ConversionPolicyPermitLimit,
                OverrideWindowMinutes = settings.ConversionPolicyWindowMinutes,
                Source = settings.HasConversionPolicyOverride ? "Override" : "Tier",
            },
            HasAnyOverride = settings.HasAnyOverride,
            UpdatedAt = settings.UpdatedAt,
        };
}
