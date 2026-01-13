// <copyright file="EnableUserCommand.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Domain.Primitives;

using MediatR;

namespace FileConversionApi.Application.Commands.Admin;

/// <summary>
/// Command to enable a user.
/// </summary>
public record EnableUserCommand : IRequest<Result<Unit>>
{
    /// <summary>
    /// Gets the user identifier.
    /// </summary>
    public required Guid UserId { get; init; }
}
