// <copyright file="GetUserByIdQuery.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Application.DTOs;
using FileConversionApi.Domain.Primitives;

using MediatR;

namespace FileConversionApi.Application.Queries.Admin;

/// <summary>
/// Query to get a user by identifier.
/// </summary>
public record GetUserByIdQuery : IRequest<Result<UserDto>>
{
    /// <summary>
    /// Gets the user identifier.
    /// </summary>
    public required Guid UserId { get; init; }
}
