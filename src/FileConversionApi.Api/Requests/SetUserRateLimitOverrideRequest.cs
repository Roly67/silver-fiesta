// <copyright file="SetUserRateLimitOverrideRequest.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

namespace FileConversionApi.Api.Requests;

/// <summary>
/// Request to set a rate limit override for a user.
/// </summary>
public record SetUserRateLimitOverrideRequest
{
    /// <summary>
    /// Gets the permit limit.
    /// </summary>
    public required int PermitLimit { get; init; }

    /// <summary>
    /// Gets the window in minutes.
    /// </summary>
    public required int WindowMinutes { get; init; }
}
