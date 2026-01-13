// <copyright file="GetUsersQueryHandler.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Application.DTOs;
using FileConversionApi.Application.Interfaces;
using FileConversionApi.Domain.Primitives;

using MediatR;

namespace FileConversionApi.Application.Queries.Admin;

/// <summary>
/// Handles the GetUsersQuery.
/// </summary>
public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, Result<PaginatedResult<UserDto>>>
{
    private readonly IUserRepository userRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetUsersQueryHandler"/> class.
    /// </summary>
    /// <param name="userRepository">The user repository.</param>
    public GetUsersQueryHandler(IUserRepository userRepository)
    {
        this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    /// <inheritdoc/>
    public async Task<Result<PaginatedResult<UserDto>>> Handle(
        GetUsersQuery request,
        CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var (users, totalCount) = await this.userRepository
            .GetAllAsync(page, pageSize, cancellationToken)
            .ConfigureAwait(false);

        var userDtos = users.Select(UserDto.FromEntity).ToList();

        return new PaginatedResult<UserDto>
        {
            Items = userDtos,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
        };
    }
}
