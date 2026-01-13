// <copyright file="SetUserRateLimitTierCommand.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Domain.Enums;
using FileConversionApi.Domain.Primitives;

using MediatR;

namespace FileConversionApi.Application.Commands.RateLimit;

/// <summary>
/// Command to set a user's rate limit tier (admin only).
/// </summary>
public record SetUserRateLimitTierCommand : IRequest<Result>
{
    /// <summary>
    /// Gets the user identifier.
    /// </summary>
    public required Guid UserId { get; init; }

    /// <summary>
    /// Gets the tier to set.
    /// </summary>
    public required RateLimitTier Tier { get; init; }
}
