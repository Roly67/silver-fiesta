// <copyright file="HealthCheckResponseWriter.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using System.Text.Json;

using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FileConversionApi.Api.HealthChecks;

/// <summary>
/// Writes health check responses in a detailed JSON format.
/// </summary>
public static class HealthCheckResponseWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
    };

    /// <summary>
    /// Writes the health report as a JSON response.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <param name="report">The health report.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task WriteAsync(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";

        var response = new HealthCheckResponse
        {
            Status = report.Status.ToString(),
            TotalDuration = report.TotalDuration.ToString(),
            Entries = report.Entries.ToDictionary(
                entry => entry.Key,
                entry => new HealthCheckEntryResponse
                {
                    Status = entry.Value.Status.ToString(),
                    Duration = entry.Value.Duration.ToString(),
                    Description = entry.Value.Description,
                    Data = entry.Value.Data.Count > 0
                        ? entry.Value.Data.ToDictionary(d => d.Key, d => d.Value)
                        : null,
                    Exception = entry.Value.Exception?.Message,
                }),
        };

        await context.Response.WriteAsJsonAsync(response, JsonOptions);
    }

    /// <summary>
    /// Response model for health check results.
    /// </summary>
    private sealed class HealthCheckResponse
    {
        /// <summary>
        /// Gets or sets the overall health status.
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the total duration of all health checks.
        /// </summary>
        public string TotalDuration { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the individual health check entries.
        /// </summary>
        public Dictionary<string, HealthCheckEntryResponse> Entries { get; set; } = [];
    }

    /// <summary>
    /// Response model for an individual health check entry.
    /// </summary>
    private sealed class HealthCheckEntryResponse
    {
        /// <summary>
        /// Gets or sets the health status.
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the duration of the health check.
        /// </summary>
        public string Duration { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the description of the health check result.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets additional data from the health check.
        /// </summary>
        public Dictionary<string, object>? Data { get; set; }

        /// <summary>
        /// Gets or sets the exception message if the health check failed.
        /// </summary>
        public string? Exception { get; set; }
    }
}
