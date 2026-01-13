// <copyright file="GetUserQuotaHistoryQuery.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Application.DTOs;
using FileConversionApi.Domain.Primitives;

using MediatR;

namespace FileConversionApi.Application.Queries.Quota;

/// <summary>
/// Query to get a user's quota history (admin only).
/// </summary>
public record GetUserQuotaHistoryQuery : IRequest<Result<IReadOnlyList<UsageQuotaDto>>>
{
    /// <summary>
    /// Gets the user identifier.
    /// </summary>
    public required Guid UserId { get; init; }

    /// <summary>
    /// Gets the number of months to retrieve.
    /// </summary>
    public int Months { get; init; } = 12;
}
