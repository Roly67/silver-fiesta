// <copyright file="RegisterRequest.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

namespace FileConversionApi.Api.Controllers.V1;

/// <summary>
/// Request for user registration.
/// </summary>
public record RegisterRequest
{
    /// <summary>
    /// Gets the email address.
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// Gets the password.
    /// </summary>
    public required string Password { get; init; }
}
