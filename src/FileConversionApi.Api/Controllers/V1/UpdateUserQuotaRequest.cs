// <copyright file="UpdateUserQuotaRequest.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace FileConversionApi.Api.Controllers.V1;

/// <summary>
/// Request model for updating a user's quota limits.
/// </summary>
public record UpdateUserQuotaRequest
{
    /// <summary>
    /// Gets the new conversions limit.
    /// </summary>
    [Required]
    [Range(0, int.MaxValue)]
    public int ConversionsLimit { get; init; }

    /// <summary>
    /// Gets the new bytes limit.
    /// </summary>
    [Required]
    [Range(0, long.MaxValue)]
    public long BytesLimit { get; init; }
}
