// <copyright file="GetUserRateLimitSettingsQuery.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Application.DTOs;
using FileConversionApi.Domain.Primitives;

using MediatR;

namespace FileConversionApi.Application.Queries.RateLimit;

/// <summary>
/// Query to get a user's rate limit settings (admin only).
/// </summary>
public record GetUserRateLimitSettingsQuery : IRequest<Result<UserRateLimitSettingsDto>>
{
    /// <summary>
    /// Gets the user identifier.
    /// </summary>
    public required Guid UserId { get; init; }
}
