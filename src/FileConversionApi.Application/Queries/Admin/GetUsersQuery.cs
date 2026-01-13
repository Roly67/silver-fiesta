// <copyright file="GetUsersQuery.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Application.DTOs;
using FileConversionApi.Domain.Primitives;

using MediatR;

namespace FileConversionApi.Application.Queries.Admin;

/// <summary>
/// Query to get all users with pagination.
/// </summary>
public record GetUsersQuery : IRequest<Result<PaginatedResult<UserDto>>>
{
    /// <summary>
    /// Gets the page number (1-based).
    /// </summary>
    public int Page { get; init; } = 1;

    /// <summary>
    /// Gets the page size.
    /// </summary>
    public int PageSize { get; init; } = 20;
}
