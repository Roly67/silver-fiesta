// <copyright file="ConversionTemplate.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Domain.ValueObjects;

namespace FileConversionApi.Domain.Entities;

/// <summary>
/// Represents a reusable conversion template with predefined settings.
/// </summary>
public class ConversionTemplate
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConversionTemplate"/> class.
    /// </summary>
    /// <remarks>Required by EF Core.</remarks>
    private ConversionTemplate()
    {
    }

    /// <summary>
    /// Gets the unique identifier for this template.
    /// </summary>
    public ConversionTemplateId Id { get; private set; }

    /// <summary>
    /// Gets the identifier of the user who owns this template.
    /// </summary>
    public UserId UserId { get; private set; }

    /// <summary>
    /// Gets the name of the template.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the optional description of the template.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Gets the target format for conversions using this template.
    /// </summary>
    public string TargetFormat { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the serialized conversion options JSON.
    /// </summary>
    public string OptionsJson { get; private set; } = "{}";

    /// <summary>
    /// Gets the date and time when this template was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>
    /// Gets the date and time when this template was last updated.
    /// </summary>
    public DateTimeOffset? UpdatedAt { get; private set; }

    /// <summary>
    /// Creates a new conversion template.
    /// </summary>
    /// <param name="userId">The ID of the user creating the template.</param>
    /// <param name="name">The name of the template.</param>
    /// <param name="targetFormat">The target format (e.g., "pdf", "html").</param>
    /// <param name="optionsJson">The serialized conversion options.</param>
    /// <param name="description">Optional description.</param>
    /// <returns>A new <see cref="ConversionTemplate"/> instance.</returns>
    public static ConversionTemplate Create(
        UserId userId,
        string name,
        string targetFormat,
        string optionsJson,
        string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Template name cannot be empty.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(targetFormat))
        {
            throw new ArgumentException("Target format cannot be empty.", nameof(targetFormat));
        }

        return new ConversionTemplate
        {
            Id = ConversionTemplateId.New(),
            UserId = userId,
            Name = name.Trim(),
            Description = description?.Trim(),
            TargetFormat = targetFormat.ToLowerInvariant(),
            OptionsJson = optionsJson,
            CreatedAt = DateTimeOffset.UtcNow,
        };
    }

    /// <summary>
    /// Updates the template's name and description.
    /// </summary>
    /// <param name="name">The new name.</param>
    /// <param name="description">The new description.</param>
    public void UpdateDetails(string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Template name cannot be empty.", nameof(name));
        }

        this.Name = name.Trim();
        this.Description = description?.Trim();
        this.UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Updates the template's conversion options.
    /// </summary>
    /// <param name="optionsJson">The new serialized options.</param>
    public void UpdateOptions(string optionsJson)
    {
        this.OptionsJson = optionsJson ?? "{}";
        this.UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Updates the template's target format.
    /// </summary>
    /// <param name="targetFormat">The new target format.</param>
    public void UpdateTargetFormat(string targetFormat)
    {
        if (string.IsNullOrWhiteSpace(targetFormat))
        {
            throw new ArgumentException("Target format cannot be empty.", nameof(targetFormat));
        }

        this.TargetFormat = targetFormat.ToLowerInvariant();
        this.UpdatedAt = DateTimeOffset.UtcNow;
    }
}
