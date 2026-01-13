// <copyright file="GetUserTemplatesQuery.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Application.DTOs;
using FileConversionApi.Domain.Primitives;

using MediatR;

namespace FileConversionApi.Application.Queries.Templates;

/// <summary>
/// Query to get all templates for a user.
/// </summary>
public record GetUserTemplatesQuery : IRequest<Result<IReadOnlyList<ConversionTemplateDto>>>
{
    /// <summary>
    /// Gets the user identifier.
    /// </summary>
    public required Guid UserId { get; init; }

    /// <summary>
    /// Gets the optional target format filter.
    /// </summary>
    public string? TargetFormat { get; init; }
}
