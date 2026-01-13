// <copyright file="CreateTemplateRequest.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Application.DTOs;

namespace FileConversionApi.Api.Controllers.V1;

/// <summary>
/// Request for creating a conversion template.
/// </summary>
public record CreateTemplateRequest
{
    /// <summary>
    /// Gets the name of the template.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the optional description.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Gets the target format for conversions (e.g., "pdf", "html", "png").
    /// </summary>
    public required string TargetFormat { get; init; }

    /// <summary>
    /// Gets the conversion options.
    /// </summary>
    public ConversionOptions? Options { get; init; }
}
