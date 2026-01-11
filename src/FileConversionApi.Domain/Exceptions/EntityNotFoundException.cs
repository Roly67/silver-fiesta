// <copyright file="EntityNotFoundException.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

namespace FileConversionApi.Domain.Exceptions;

/// <summary>
/// Thrown when an entity is not found.
/// </summary>
public sealed class EntityNotFoundException : DomainException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityNotFoundException"/> class.
    /// </summary>
    /// <param name="entityName">The name of the entity.</param>
    /// <param name="id">The identifier of the entity.</param>
    public EntityNotFoundException(string entityName, object id)
        : base($"{entityName} with ID '{id}' was not found.")
    {
        this.EntityName = entityName;
        this.EntityId = id;
    }

    /// <summary>
    /// Gets the entity name.
    /// </summary>
    public string EntityName { get; }

    /// <summary>
    /// Gets the entity ID.
    /// </summary>
    public object EntityId { get; }
}
