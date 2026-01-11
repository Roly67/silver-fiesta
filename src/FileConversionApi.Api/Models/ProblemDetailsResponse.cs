// <copyright file="ProblemDetailsResponse.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using System.Text.Json.Serialization;

namespace FileConversionApi.Api.Models;

/// <summary>
/// RFC 7807 Problem Details response.
/// </summary>
public class ProblemDetailsResponse
{
    /// <summary>
    /// Gets or sets the problem type URI.
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = "about:blank";

    /// <summary>
    /// Gets or sets the short summary of the problem.
    /// </summary>
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the HTTP status code.
    /// </summary>
    [JsonPropertyName("status")]
    public int Status { get; set; }

    /// <summary>
    /// Gets or sets the detailed explanation.
    /// </summary>
    [JsonPropertyName("detail")]
    public string? Detail { get; set; }

    /// <summary>
    /// Gets or sets the URI of the specific occurrence.
    /// </summary>
    [JsonPropertyName("instance")]
    public string? Instance { get; set; }

    /// <summary>
    /// Gets or sets the trace ID for correlation.
    /// </summary>
    [JsonPropertyName("traceId")]
    public string? TraceId { get; set; }

    /// <summary>
    /// Gets or sets the error code.
    /// </summary>
    [JsonPropertyName("errorCode")]
    public string? ErrorCode { get; set; }

    /// <summary>
    /// Gets or sets validation errors.
    /// </summary>
    [JsonPropertyName("errors")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IDictionary<string, string[]>? Errors { get; set; }
}
