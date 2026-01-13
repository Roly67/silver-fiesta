// <copyright file="CreateConversionTemplateCommand.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Application.DTOs;
using FileConversionApi.Domain.Primitives;

using MediatR;

namespace FileConversionApi.Application.Commands.Templates;

/// <summary>
/// Command to create a new conversion template.
/// </summary>
public record CreateConversionTemplateCommand : IRequest<Result<ConversionTemplateDto>>
{
    /// <summary>
    /// Gets the user identifier who owns the template.
    /// </summary>
    public required Guid UserId { get; init; }

    /// <summary>
    /// Gets the name of the template.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the optional description.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Gets the target format for conversions.
    /// </summary>
    public required string TargetFormat { get; init; }

    /// <summary>
    /// Gets the conversion options.
    /// </summary>
    public ConversionOptions? Options { get; init; }
}
