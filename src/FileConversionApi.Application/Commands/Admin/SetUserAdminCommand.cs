// <copyright file="SetUserAdminCommand.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Domain.Primitives;

using MediatR;

namespace FileConversionApi.Application.Commands.Admin;

/// <summary>
/// Command to grant or revoke admin privileges for a user.
/// </summary>
public record SetUserAdminCommand : IRequest<Result<Unit>>
{
    /// <summary>
    /// Gets the user identifier.
    /// </summary>
    public required Guid UserId { get; init; }

    /// <summary>
    /// Gets a value indicating whether to grant admin privileges.
    /// </summary>
    public required bool IsAdmin { get; init; }
}
