// <copyright file="HealthResponse.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

namespace FileConversionApi.Api.Controllers;

/// <summary>
/// Health check response.
/// </summary>
public record HealthResponse
{
    /// <summary>
    /// Gets the health status.
    /// </summary>
    public required string Status { get; init; }

    /// <summary>
    /// Gets the timestamp.
    /// </summary>
    public required DateTimeOffset Timestamp { get; init; }
}
