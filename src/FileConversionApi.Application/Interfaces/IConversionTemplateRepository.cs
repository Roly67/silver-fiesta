// <copyright file="IConversionTemplateRepository.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Domain.Entities;
using FileConversionApi.Domain.ValueObjects;

namespace FileConversionApi.Application.Interfaces;

/// <summary>
/// Repository interface for conversion template data access.
/// </summary>
public interface IConversionTemplateRepository
{
    /// <summary>
    /// Gets a template by identifier.
    /// </summary>
    /// <param name="id">The template identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The template if found; otherwise, null.</returns>
    Task<ConversionTemplate?> GetByIdAsync(ConversionTemplateId id, CancellationToken cancellationToken);

    /// <summary>
    /// Gets all templates for a specific user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of templates owned by the user.</returns>
    Task<IReadOnlyList<ConversionTemplate>> GetByUserIdAsync(UserId userId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets templates for a user filtered by target format.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="targetFormat">The target format to filter by.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of templates matching the criteria.</returns>
    Task<IReadOnlyList<ConversionTemplate>> GetByUserIdAndFormatAsync(
        UserId userId,
        string targetFormat,
        CancellationToken cancellationToken);

    /// <summary>
    /// Checks if a template with the given name exists for the user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="name">The template name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if a template with this name exists; otherwise, false.</returns>
    Task<bool> NameExistsForUserAsync(UserId userId, string name, CancellationToken cancellationToken);

    /// <summary>
    /// Checks if a template with the given name exists for the user, excluding a specific template.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="name">The template name.</param>
    /// <param name="excludeTemplateId">The template ID to exclude from the check.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if a template with this name exists; otherwise, false.</returns>
    Task<bool> NameExistsForUserAsync(
        UserId userId,
        string name,
        ConversionTemplateId excludeTemplateId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Adds a new template.
    /// </summary>
    /// <param name="template">The template to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the operation.</returns>
    Task AddAsync(ConversionTemplate template, CancellationToken cancellationToken);

    /// <summary>
    /// Updates an existing template.
    /// </summary>
    /// <param name="template">The template to update.</param>
    void Update(ConversionTemplate template);

    /// <summary>
    /// Deletes a template.
    /// </summary>
    /// <param name="template">The template to delete.</param>
    void Delete(ConversionTemplate template);
}
