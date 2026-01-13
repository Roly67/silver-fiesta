// <copyright file="RateLimitPolicyDto.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

namespace FileConversionApi.Application.DTOs;

/// <summary>
/// Data transfer object for rate limit policy information.
/// </summary>
public record RateLimitPolicyDto
{
    /// <summary>
    /// Gets the effective permit limit after considering tier and overrides.
    /// </summary>
    public required int EffectivePermitLimit { get; init; }

    /// <summary>
    /// Gets the effective window in minutes after considering tier and overrides.
    /// </summary>
    public required int EffectiveWindowMinutes { get; init; }

    /// <summary>
    /// Gets the per-user override for permit limit, if set.
    /// </summary>
    public int? OverridePermitLimit { get; init; }

    /// <summary>
    /// Gets the per-user override for window minutes, if set.
    /// </summary>
    public int? OverrideWindowMinutes { get; init; }

    /// <summary>
    /// Gets the source of the effective limits (Tier or Override).
    /// </summary>
    public required string Source { get; init; }
}
