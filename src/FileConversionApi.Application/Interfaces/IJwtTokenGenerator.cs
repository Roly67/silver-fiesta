// <copyright file="IJwtTokenGenerator.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Application.DTOs;
using FileConversionApi.Domain.Entities;

namespace FileConversionApi.Application.Interfaces;

/// <summary>
/// Interface for JWT token generation.
/// </summary>
public interface IJwtTokenGenerator
{
    /// <summary>
    /// Generates a JWT token for a user.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>The token response.</returns>
    TokenResponse GenerateToken(User user);

    /// <summary>
    /// Validates and refreshes a token.
    /// </summary>
    /// <param name="refreshToken">The refresh token.</param>
    /// <param name="user">The user.</param>
    /// <returns>A new token response.</returns>
    TokenResponse RefreshToken(string refreshToken, User user);
}
