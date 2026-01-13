// <copyright file="DeleteConversionTemplateCommand.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Domain.Primitives;

using MediatR;

namespace FileConversionApi.Application.Commands.Templates;

/// <summary>
/// Command to delete a conversion template.
/// </summary>
public record DeleteConversionTemplateCommand : IRequest<Result<Unit>>
{
    /// <summary>
    /// Gets the template identifier.
    /// </summary>
    public required Guid TemplateId { get; init; }

    /// <summary>
    /// Gets the user identifier who owns the template.
    /// </summary>
    public required Guid UserId { get; init; }
}
