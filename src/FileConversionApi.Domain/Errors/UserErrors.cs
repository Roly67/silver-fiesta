// <copyright file="UserErrors.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Domain.Primitives;

namespace FileConversionApi.Domain.Errors;

/// <summary>
/// Domain errors for users.
/// </summary>
public static class UserErrors
{
    /// <summary>
    /// Email already exists error.
    /// </summary>
    public static readonly Error EmailAlreadyExists =
        new("User.EmailAlreadyExists", "A user with this email address already exists.");

    /// <summary>
    /// Invalid credentials error.
    /// </summary>
    public static readonly Error InvalidCredentials =
        new("User.InvalidCredentials", "The provided credentials are invalid.");

    /// <summary>
    /// User is inactive error.
    /// </summary>
    public static readonly Error Inactive =
        new("User.Inactive", "The user account is inactive.");

    /// <summary>
    /// Invalid API key error.
    /// </summary>
    public static readonly Error InvalidApiKey =
        new("User.InvalidApiKey", "The provided API key is invalid.");

    /// <summary>
    /// User not found error.
    /// </summary>
    /// <param name="id">The user identifier.</param>
    /// <returns>The error.</returns>
    public static Error NotFound(Guid id) =>
        new("User.NotFound", $"User with ID '{id}' was not found.");

    /// <summary>
    /// User not found by email error.
    /// </summary>
    /// <param name="email">The email address.</param>
    /// <returns>The error.</returns>
    public static Error NotFoundByEmail(string email) =>
        new("User.NotFoundByEmail", $"User with email '{email}' was not found.");
}
