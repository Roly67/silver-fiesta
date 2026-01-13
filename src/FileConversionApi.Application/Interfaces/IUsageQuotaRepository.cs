// <copyright file="IUsageQuotaRepository.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Domain.Entities;
using FileConversionApi.Domain.ValueObjects;

namespace FileConversionApi.Application.Interfaces;

/// <summary>
/// Repository interface for usage quota data access.
/// </summary>
public interface IUsageQuotaRepository
{
    /// <summary>
    /// Gets a usage quota by identifier.
    /// </summary>
    /// <param name="id">The quota identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The quota if found; otherwise, null.</returns>
    Task<UsageQuota?> GetByIdAsync(UsageQuotaId id, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the usage quota for a user for a specific month.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="year">The year.</param>
    /// <param name="month">The month (1-12).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The quota if found; otherwise, null.</returns>
    Task<UsageQuota?> GetByUserAndMonthAsync(
        UserId userId,
        int year,
        int month,
        CancellationToken cancellationToken);

    /// <summary>
    /// Gets all usage quotas for a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The list of quotas for the user.</returns>
    Task<IReadOnlyList<UsageQuota>> GetByUserAsync(UserId userId, CancellationToken cancellationToken);

    /// <summary>
    /// Adds a new usage quota.
    /// </summary>
    /// <param name="quota">The quota to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the operation.</returns>
    Task AddAsync(UsageQuota quota, CancellationToken cancellationToken);

    /// <summary>
    /// Updates an existing usage quota.
    /// </summary>
    /// <param name="quota">The quota to update.</param>
    void Update(UsageQuota quota);
}
