// <copyright file="RefreshTokenRequest.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

namespace FileConversionApi.Api.Controllers.V1;

/// <summary>
/// Request for token refresh.
/// </summary>
public record RefreshTokenRequest
{
    /// <summary>
    /// Gets the refresh token.
    /// </summary>
    public required string RefreshToken { get; init; }
}
