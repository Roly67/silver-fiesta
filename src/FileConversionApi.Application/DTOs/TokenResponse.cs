// <copyright file="TokenResponse.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

namespace FileConversionApi.Application.DTOs;

/// <summary>
/// Response containing JWT tokens.
/// </summary>
public record TokenResponse
{
    /// <summary>
    /// Gets the access token.
    /// </summary>
    public required string AccessToken { get; init; }

    /// <summary>
    /// Gets the refresh token.
    /// </summary>
    public required string RefreshToken { get; init; }

    /// <summary>
    /// Gets the token type.
    /// </summary>
    public string TokenType { get; init; } = "Bearer";

    /// <summary>
    /// Gets the expiration time in seconds.
    /// </summary>
    public required int ExpiresIn { get; init; }
}
