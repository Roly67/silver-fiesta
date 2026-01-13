// <copyright file="ResetUserApiKeyCommand.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Domain.Primitives;

using MediatR;

namespace FileConversionApi.Application.Commands.Admin;

/// <summary>
/// Command to reset a user's API key.
/// </summary>
public record ResetUserApiKeyCommand : IRequest<Result<string>>
{
    /// <summary>
    /// Gets the user identifier.
    /// </summary>
    public required Guid UserId { get; init; }
}
