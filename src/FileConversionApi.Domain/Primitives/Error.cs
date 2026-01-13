// <copyright file="Error.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

namespace FileConversionApi.Domain.Primitives;

/// <summary>
/// Represents an error with a code and message.
/// </summary>
/// <param name="Code">The error code.</param>
/// <param name="Message">The error message.</param>
public sealed record Error(string Code, string Message)
{
    /// <summary>
    /// Represents no error.
    /// </summary>
    public static readonly Error None = new(string.Empty, string.Empty);

    /// <summary>
    /// Represents a null value error.
    /// </summary>
    public static readonly Error NullValue = new("Error.NullValue", "A null value was provided.");

    /// <summary>
    /// Implicitly converts an error to a result.
    /// </summary>
    /// <param name="error">The error to convert.</param>
    public static implicit operator Result(Error error) => Result.Failure(error);

    /// <summary>
    /// Creates a validation error with the specified code and message.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="message">The error message.</param>
    /// <returns>A new validation error.</returns>
    public static Error Validation(string code, string message) => new(code, message);

    /// <summary>
    /// Creates a not found error with the specified code and message.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="message">The error message.</param>
    /// <returns>A new not found error.</returns>
    public static Error NotFound(string code, string message) => new(code, message);

    /// <summary>
    /// Creates a forbidden error with the specified code and message.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="message">The error message.</param>
    /// <returns>A new forbidden error.</returns>
    public static Error Forbidden(string code, string message) => new(code, message);

    /// <summary>
    /// Creates an unauthorized error with the specified message.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <returns>A new unauthorized error.</returns>
    public static Error Unauthorized(string message) => new("Error.Unauthorized", message);
}
