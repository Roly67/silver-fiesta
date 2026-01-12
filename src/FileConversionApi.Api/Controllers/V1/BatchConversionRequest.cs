// <copyright file="BatchConversionRequest.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Application.DTOs;

namespace FileConversionApi.Api.Controllers.V1;

/// <summary>
/// Request for batch conversion of multiple files.
/// </summary>
public record BatchConversionRequest
{
    /// <summary>
    /// Gets the conversion items in the batch (max 20 items).
    /// </summary>
    public List<BatchConversionItem>? Items { get; init; }

    /// <summary>
    /// Gets the webhook URL to notify when conversions complete.
    /// </summary>
    public string? WebhookUrl { get; init; }
}
