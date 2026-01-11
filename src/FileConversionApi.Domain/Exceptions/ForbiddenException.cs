// <copyright file="ForbiddenException.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

namespace FileConversionApi.Domain.Exceptions;

/// <summary>
/// Thrown when the user is forbidden from accessing a resource.
/// </summary>
public sealed class ForbiddenException : DomainException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ForbiddenException"/> class.
    /// </summary>
    /// <param name="resource">The resource that was forbidden.</param>
    public ForbiddenException(string resource)
        : base($"Access to '{resource}' is forbidden.")
    {
        this.Resource = resource;
    }

    /// <summary>
    /// Gets the resource that was forbidden.
    /// </summary>
    public string Resource { get; }
}
