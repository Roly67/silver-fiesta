// <copyright file="GetTemplateByIdQuery.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Application.DTOs;
using FileConversionApi.Domain.Primitives;

using MediatR;

namespace FileConversionApi.Application.Queries.Templates;

/// <summary>
/// Query to get a specific template by ID.
/// </summary>
public record GetTemplateByIdQuery : IRequest<Result<ConversionTemplateDto>>
{
    /// <summary>
    /// Gets the template identifier.
    /// </summary>
    public required Guid TemplateId { get; init; }

    /// <summary>
    /// Gets the user identifier (for ownership validation).
    /// </summary>
    public required Guid UserId { get; init; }
}
