// <copyright file="ConversionTemplateDto.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using System.Text.Json;

using FileConversionApi.Domain.Entities;

namespace FileConversionApi.Application.DTOs;

/// <summary>
/// Data transfer object for conversion template information.
/// </summary>
public record ConversionTemplateDto
{
    /// <summary>
    /// Gets the template identifier.
    /// </summary>
    public required Guid Id { get; init; }

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

    /// <summary>
    /// Gets the date and time when the template was created.
    /// </summary>
    public required DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    /// Gets the date and time when the template was last updated.
    /// </summary>
    public DateTimeOffset? UpdatedAt { get; init; }

    /// <summary>
    /// Creates a DTO from a domain entity.
    /// </summary>
    /// <param name="template">The template entity.</param>
    /// <returns>The DTO.</returns>
    public static ConversionTemplateDto FromEntity(ConversionTemplate template)
    {
        ConversionOptions? options = null;

        try
        {
            options = JsonSerializer.Deserialize<ConversionOptions>(
                template.OptionsJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (JsonException)
        {
            // If deserialization fails, leave options as null
        }

        return new ConversionTemplateDto
        {
            Id = template.Id.Value,
            Name = template.Name,
            Description = template.Description,
            TargetFormat = template.TargetFormat,
            Options = options,
            CreatedAt = template.CreatedAt,
            UpdatedAt = template.UpdatedAt,
        };
    }
}
