// <copyright file="UpdateConversionTemplateCommand.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Application.DTOs;
using FileConversionApi.Domain.Primitives;

using MediatR;

namespace FileConversionApi.Application.Commands.Templates;

/// <summary>
/// Command to update an existing conversion template.
/// </summary>
public record UpdateConversionTemplateCommand : IRequest<Result<ConversionTemplateDto>>
{
    /// <summary>
    /// Gets the template identifier.
    /// </summary>
    public required Guid TemplateId { get; init; }

    /// <summary>
    /// Gets the user identifier who owns the template.
    /// </summary>
    public required Guid UserId { get; init; }

    /// <summary>
    /// Gets the new name for the template.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the new description.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Gets the new target format.
    /// </summary>
    public required string TargetFormat { get; init; }

    /// <summary>
    /// Gets the new conversion options.
    /// </summary>
    public ConversionOptions? Options { get; init; }
}
