// <copyright file="UserRateLimitSettingsRepository.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Application.Interfaces;
using FileConversionApi.Domain.Entities;
using FileConversionApi.Domain.ValueObjects;

using Microsoft.EntityFrameworkCore;

namespace FileConversionApi.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for user rate limit settings data access.
/// </summary>
public class UserRateLimitSettingsRepository : IUserRateLimitSettingsRepository
{
    private readonly AppDbContext context;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserRateLimitSettingsRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public UserRateLimitSettingsRepository(AppDbContext context)
    {
        this.context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc/>
    public async Task<UserRateLimitSettings?> GetByUserIdAsync(
        UserId userId,
        CancellationToken cancellationToken)
    {
        return await this.context.UserRateLimitSettings
            .FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<UserRateLimitSettings>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await this.context.UserRateLimitSettings
            .OrderBy(s => s.CreatedAt)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task AddAsync(UserRateLimitSettings settings, CancellationToken cancellationToken)
    {
        await this.context.UserRateLimitSettings.AddAsync(settings, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public void Update(UserRateLimitSettings settings)
    {
        this.context.UserRateLimitSettings.Update(settings);
    }

    /// <inheritdoc/>
    public void Delete(UserRateLimitSettings settings)
    {
        this.context.UserRateLimitSettings.Remove(settings);
    }
}
