// <copyright file="ClearUserRateLimitOverridesCommand.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Domain.Primitives;

using MediatR;

namespace FileConversionApi.Application.Commands.RateLimit;

/// <summary>
/// Command to clear all per-user rate limit overrides (admin only).
/// </summary>
public record ClearUserRateLimitOverridesCommand : IRequest<Result>
{
    /// <summary>
    /// Gets the user identifier.
    /// </summary>
    public required Guid UserId { get; init; }
}
