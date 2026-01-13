// <copyright file="IUserRateLimitSettingsRepository.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Domain.Entities;
using FileConversionApi.Domain.ValueObjects;

namespace FileConversionApi.Application.Interfaces;

/// <summary>
/// Repository interface for user rate limit settings data access.
/// </summary>
public interface IUserRateLimitSettingsRepository
{
    /// <summary>
    /// Gets the rate limit settings for a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The settings if found; otherwise, null.</returns>
    Task<UserRateLimitSettings?> GetByUserIdAsync(UserId userId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets all user rate limit settings.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The list of all settings.</returns>
    Task<IReadOnlyList<UserRateLimitSettings>> GetAllAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Adds new rate limit settings.
    /// </summary>
    /// <param name="settings">The settings to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the operation.</returns>
    Task AddAsync(UserRateLimitSettings settings, CancellationToken cancellationToken);

    /// <summary>
    /// Updates existing rate limit settings.
    /// </summary>
    /// <param name="settings">The settings to update.</param>
    void Update(UserRateLimitSettings settings);

    /// <summary>
    /// Deletes rate limit settings.
    /// </summary>
    /// <param name="settings">The settings to delete.</param>
    void Delete(UserRateLimitSettings settings);
}
