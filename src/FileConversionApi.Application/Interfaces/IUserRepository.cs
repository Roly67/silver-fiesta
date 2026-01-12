// <copyright file="IUserRepository.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Domain.Entities;
using FileConversionApi.Domain.ValueObjects;

namespace FileConversionApi.Application.Interfaces;

/// <summary>
/// Repository interface for user data access.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Gets a user by identifier.
    /// </summary>
    /// <param name="id">The user identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The user if found; otherwise, null.</returns>
    Task<User?> GetByIdAsync(UserId id, CancellationToken cancellationToken);

    /// <summary>
    /// Gets a user by email address.
    /// </summary>
    /// <param name="email">The email address.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The user if found; otherwise, null.</returns>
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken);

    /// <summary>
    /// Gets a user by API key.
    /// </summary>
    /// <param name="apiKey">The API key.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The user if found; otherwise, null.</returns>
    Task<User?> GetByApiKeyAsync(string apiKey, CancellationToken cancellationToken);

    /// <summary>
    /// Checks if an email address is already in use.
    /// </summary>
    /// <param name="email">The email address.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the email exists; otherwise, false.</returns>
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken);

    /// <summary>
    /// Adds a new user.
    /// </summary>
    /// <param name="user">The user to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the operation.</returns>
    Task AddAsync(User user, CancellationToken cancellationToken);

    /// <summary>
    /// Updates an existing user.
    /// </summary>
    /// <param name="user">The user to update.</param>
    void Update(User user);

    /// <summary>
    /// Gets all users with pagination.
    /// </summary>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A tuple of users and total count.</returns>
    Task<(IReadOnlyList<User> Users, int TotalCount)> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the total number of users.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The total count of users.</returns>
    Task<int> GetTotalCountAsync(CancellationToken cancellationToken);
}
