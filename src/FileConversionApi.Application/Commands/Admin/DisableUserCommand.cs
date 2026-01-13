// <copyright file="DisableUserCommand.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Domain.Primitives;

using MediatR;

namespace FileConversionApi.Application.Commands.Admin;

/// <summary>
/// Command to disable a user.
/// </summary>
public record DisableUserCommand : IRequest<Result<Unit>>
{
    /// <summary>
    /// Gets the user identifier.
    /// </summary>
    public required Guid UserId { get; init; }
}
