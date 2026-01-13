// <copyright file="SetUserRateLimitOverrideCommand.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Domain.Primitives;

using MediatR;

namespace FileConversionApi.Application.Commands.RateLimit;

/// <summary>
/// Command to set a per-user rate limit override for a specific policy (admin only).
/// </summary>
public record SetUserRateLimitOverrideCommand : IRequest<Result>
{
    /// <summary>
    /// Gets the user identifier.
    /// </summary>
    public required Guid UserId { get; init; }

    /// <summary>
    /// Gets the policy name (standard or conversion).
    /// </summary>
    public required string PolicyName { get; init; }

    /// <summary>
    /// Gets the permit limit override, or null to use tier default.
    /// </summary>
    public int? PermitLimit { get; init; }

    /// <summary>
    /// Gets the window minutes override, or null to use tier default.
    /// </summary>
    public int? WindowMinutes { get; init; }
}
