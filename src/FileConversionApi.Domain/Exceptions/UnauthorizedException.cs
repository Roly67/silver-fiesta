// <copyright file="UnauthorizedException.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

namespace FileConversionApi.Domain.Exceptions;

/// <summary>
/// Thrown when the user is not authorized.
/// </summary>
public sealed class UnauthorizedException : DomainException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UnauthorizedException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    public UnauthorizedException(string message = "You are not authorized to perform this action.")
        : base(message)
    {
    }
}
