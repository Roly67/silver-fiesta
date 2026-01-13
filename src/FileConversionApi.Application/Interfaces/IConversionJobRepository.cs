// <copyright file="IConversionJobRepository.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Domain.Entities;
using FileConversionApi.Domain.ValueObjects;

namespace FileConversionApi.Application.Interfaces;

/// <summary>
/// Repository interface for conversion job data access.
/// </summary>
public interface IConversionJobRepository
{
    /// <summary>
    /// Gets a conversion job by identifier.
    /// </summary>
    /// <param name="id">The job identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The conversion job if found; otherwise, null.</returns>
    Task<ConversionJob?> GetByIdAsync(ConversionJobId id, CancellationToken cancellationToken);

    /// <summary>
    /// Gets a conversion job by identifier for a specific user.
    /// </summary>
    /// <param name="id">The job identifier.</param>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The conversion job if found; otherwise, null.</returns>
    Task<ConversionJob?> GetByIdForUserAsync(ConversionJobId id, UserId userId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets conversion jobs for a user with pagination.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of conversion jobs.</returns>
    Task<IReadOnlyList<ConversionJob>> GetByUserIdAsync(
        UserId userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    /// <summary>
    /// Gets the total count of conversion jobs for a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The total count.</returns>
    Task<int> GetCountByUserIdAsync(UserId userId, CancellationToken cancellationToken);

    /// <summary>
    /// Adds a new conversion job.
    /// </summary>
    /// <param name="job">The conversion job to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the operation.</returns>
    Task AddAsync(ConversionJob job, CancellationToken cancellationToken);

    /// <summary>
    /// Updates an existing conversion job.
    /// </summary>
    /// <param name="job">The conversion job to update.</param>
    void Update(ConversionJob job);

    /// <summary>
    /// Gets job statistics for all users.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A tuple containing total, completed, failed, and pending job counts.</returns>
    Task<(int Total, int Completed, int Failed, int Pending)> GetStatisticsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Gets the total count of all conversion jobs.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The total count.</returns>
    Task<int> GetTotalCountAsync(CancellationToken cancellationToken);
}
