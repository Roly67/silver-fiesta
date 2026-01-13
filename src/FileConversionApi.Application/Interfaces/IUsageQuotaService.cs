// <copyright file="IUsageQuotaService.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Domain.Entities;
using FileConversionApi.Domain.Primitives;
using FileConversionApi.Domain.ValueObjects;

namespace FileConversionApi.Application.Interfaces;

/// <summary>
/// Service interface for managing usage quotas.
/// </summary>
public interface IUsageQuotaService
{
    /// <summary>
    /// Checks if the user can perform a conversion based on their quota.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result indicating success or the quota error.</returns>
    Task<Result> CheckQuotaAsync(UserId userId, CancellationToken cancellationToken);

    /// <summary>
    /// Records a conversion usage for a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="bytesProcessed">The bytes processed in the conversion.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the operation.</returns>
    Task RecordUsageAsync(UserId userId, long bytesProcessed, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the current quota for a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The user's current quota.</returns>
    Task<Result<UsageQuota>> GetCurrentQuotaAsync(UserId userId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the quota history for a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="months">The number of months to retrieve.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The list of quota records for the user.</returns>
    Task<Result<IReadOnlyList<UsageQuota>>> GetQuotaHistoryAsync(
        UserId userId,
        int months,
        CancellationToken cancellationToken);

    /// <summary>
    /// Updates the quota limits for a user's current month.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="conversionsLimit">The new conversions limit.</param>
    /// <param name="bytesLimit">The new bytes limit.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated quota.</returns>
    Task<Result<UsageQuota>> UpdateQuotaLimitsAsync(
        UserId userId,
        int conversionsLimit,
        long bytesLimit,
        CancellationToken cancellationToken);
}
