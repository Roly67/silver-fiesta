// <copyright file="RateLimitTierConfigDto.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

namespace FileConversionApi.Application.DTOs;

/// <summary>
/// Data transfer object for rate limit tier configuration.
/// </summary>
public record RateLimitTierConfigDto
{
    /// <summary>
    /// Gets the tier name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the standard policy permit limit.
    /// </summary>
    public required int StandardPermitLimit { get; init; }

    /// <summary>
    /// Gets the standard policy window in minutes.
    /// </summary>
    public required int StandardWindowMinutes { get; init; }

    /// <summary>
    /// Gets the conversion policy permit limit.
    /// </summary>
    public required int ConversionPermitLimit { get; init; }

    /// <summary>
    /// Gets the conversion policy window in minutes.
    /// </summary>
    public required int ConversionWindowMinutes { get; init; }
}
