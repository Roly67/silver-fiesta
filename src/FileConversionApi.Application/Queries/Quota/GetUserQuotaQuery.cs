// <copyright file="GetUserQuotaQuery.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Application.DTOs;
using FileConversionApi.Domain.Primitives;

using MediatR;

namespace FileConversionApi.Application.Queries.Quota;

/// <summary>
/// Query to get a user's current quota (admin only).
/// </summary>
public record GetUserQuotaQuery : IRequest<Result<UsageQuotaDto>>
{
    /// <summary>
    /// Gets the user identifier.
    /// </summary>
    public required Guid UserId { get; init; }
}
