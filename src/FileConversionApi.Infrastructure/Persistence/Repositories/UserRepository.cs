// <copyright file="UserRepository.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Application.Interfaces;
using FileConversionApi.Domain.Entities;
using FileConversionApi.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FileConversionApi.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository for user data access.
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly AppDbContext context;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public UserRepository(AppDbContext context)
    {
        this.context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc/>
    public async Task<User?> GetByIdAsync(UserId id, CancellationToken cancellationToken)
    {
        return await this.context.Users
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        return await this.context.Users
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<User?> GetByApiKeyAsync(string apiKey, CancellationToken cancellationToken)
    {
        return await this.context.Users
            .FirstOrDefaultAsync(u => u.ApiKey == apiKey, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken)
    {
        return await this.context.Users
            .AnyAsync(u => u.Email == email, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task AddAsync(User user, CancellationToken cancellationToken)
    {
        await this.context.Users.AddAsync(user, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public void Update(User user)
    {
        this.context.Users.Update(user);
    }

    /// <inheritdoc/>
    public async Task<(IReadOnlyList<User> Users, int TotalCount)> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        var totalCount = await this.context.Users.CountAsync(cancellationToken).ConfigureAwait(false);

        var users = await this.context.Users
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return (users, totalCount);
    }

    /// <inheritdoc/>
    public async Task<int> GetTotalCountAsync(CancellationToken cancellationToken)
    {
        return await this.context.Users.CountAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<bool> AnyAdminExistsAsync(CancellationToken cancellationToken)
    {
        return await this.context.Users
            .AnyAsync(u => u.IsAdmin, cancellationToken)
            .ConfigureAwait(false);
    }
}
