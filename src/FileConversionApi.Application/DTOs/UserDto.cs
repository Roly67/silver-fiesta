// <copyright file="UserDto.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Domain.Entities;

namespace FileConversionApi.Application.DTOs;

/// <summary>
/// Data transfer object for user information.
/// </summary>
public record UserDto
{
    /// <summary>
    /// Gets the user identifier.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Gets the email address.
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// Gets a value indicating whether the user is active.
    /// </summary>
    public required bool IsActive { get; init; }

    /// <summary>
    /// Gets a value indicating whether the user is an administrator.
    /// </summary>
    public required bool IsAdmin { get; init; }

    /// <summary>
    /// Gets the date and time when the user was created.
    /// </summary>
    public required DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    /// Creates a DTO from a domain entity.
    /// </summary>
    /// <param name="user">The user entity.</param>
    /// <returns>The DTO.</returns>
    public static UserDto FromEntity(User user) =>
        new()
        {
            Id = user.Id.Value,
            Email = user.Email,
            IsActive = user.IsActive,
            IsAdmin = user.IsAdmin,
            CreatedAt = user.CreatedAt,
        };
}
