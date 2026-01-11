// <copyright file="LoginRequest.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

namespace FileConversionApi.Api.Controllers.V1;

/// <summary>
/// Request for user login.
/// </summary>
public record LoginRequest
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
