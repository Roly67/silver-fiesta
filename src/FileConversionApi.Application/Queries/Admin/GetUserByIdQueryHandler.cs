// <copyright file="GetUserByIdQueryHandler.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Application.DTOs;
using FileConversionApi.Application.Interfaces;
using FileConversionApi.Domain.Primitives;
using FileConversionApi.Domain.ValueObjects;

using MediatR;

namespace FileConversionApi.Application.Queries.Admin;

/// <summary>
/// Handles the GetUserByIdQuery.
/// </summary>
public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, Result<UserDto>>
{
    private readonly IUserRepository userRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetUserByIdQueryHandler"/> class.
    /// </summary>
    /// <param name="userRepository">The user repository.</param>
    public GetUserByIdQueryHandler(IUserRepository userRepository)
    {
        this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    /// <inheritdoc/>
    public async Task<Result<UserDto>> Handle(
        GetUserByIdQuery request,
        CancellationToken cancellationToken)
    {
        var userId = new UserId(request.UserId);
        var user = await this.userRepository
            .GetByIdAsync(userId, cancellationToken)
            .ConfigureAwait(false);

        if (user is null)
        {
            return new Error("Admin.UserNotFound", $"User with ID '{request.UserId}' was not found.");
        }

        return UserDto.FromEntity(user);
    }
}
