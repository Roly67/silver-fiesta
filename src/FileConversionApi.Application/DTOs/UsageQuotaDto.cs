// <copyright file="UsageQuotaDto.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Domain.Entities;

namespace FileConversionApi.Application.DTOs;

/// <summary>
/// Data transfer object for usage quota information.
/// </summary>
public record UsageQuotaDto
{
    /// <summary>
    /// Gets the year this quota applies to.
    /// </summary>
    public required int Year { get; init; }

    /// <summary>
    /// Gets the month this quota applies to (1-12).
    /// </summary>
    public required int Month { get; init; }

    /// <summary>
    /// Gets the number of conversions used in this period.
    /// </summary>
    public required int ConversionsUsed { get; init; }

    /// <summary>
    /// Gets the maximum number of conversions allowed in this period.
    /// </summary>
    public required int ConversionsLimit { get; init; }

    /// <summary>
    /// Gets the remaining conversions available.
    /// </summary>
    public required int RemainingConversions { get; init; }

    /// <summary>
    /// Gets the total bytes processed in this period.
    /// </summary>
    public required long BytesProcessed { get; init; }

    /// <summary>
    /// Gets the maximum bytes allowed to be processed in this period.
    /// </summary>
    public required long BytesLimit { get; init; }

    /// <summary>
    /// Gets the remaining bytes available.
    /// </summary>
    public required long RemainingBytes { get; init; }

    /// <summary>
    /// Gets a value indicating whether the quota has been exceeded.
    /// </summary>
    public required bool IsQuotaExceeded { get; init; }

    /// <summary>
    /// Gets the date and time when this quota was last updated.
    /// </summary>
    public required DateTimeOffset UpdatedAt { get; init; }

    /// <summary>
    /// Creates a DTO from a domain entity.
    /// </summary>
    /// <param name="quota">The usage quota entity.</param>
    /// <returns>The DTO.</returns>
    public static UsageQuotaDto FromEntity(UsageQuota quota) =>
        new()
        {
            Year = quota.Year,
            Month = quota.Month,
            ConversionsUsed = quota.ConversionsUsed,
            ConversionsLimit = quota.ConversionsLimit,
            RemainingConversions = quota.RemainingConversions,
            BytesProcessed = quota.BytesProcessed,
            BytesLimit = quota.BytesLimit,
            RemainingBytes = quota.RemainingBytes,
            IsQuotaExceeded = quota.IsQuotaExceeded,
            UpdatedAt = quota.UpdatedAt,
        };
}
