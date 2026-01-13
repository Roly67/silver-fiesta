// <copyright file="UpdateUserQuotaCommand.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Application.DTOs;
using FileConversionApi.Domain.Primitives;

using MediatR;

namespace FileConversionApi.Application.Commands.Quota;

/// <summary>
/// Command to update a user's quota limits (admin only).
/// </summary>
public record UpdateUserQuotaCommand : IRequest<Result<UsageQuotaDto>>
{
    /// <summary>
    /// Gets the user identifier.
    /// </summary>
    public required Guid UserId { get; init; }

    /// <summary>
    /// Gets the new conversions limit.
    /// </summary>
    public required int ConversionsLimit { get; init; }

    /// <summary>
    /// Gets the new bytes limit.
    /// </summary>
    public required long BytesLimit { get; init; }
}
