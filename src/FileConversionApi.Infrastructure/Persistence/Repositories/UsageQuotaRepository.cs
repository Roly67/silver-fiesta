// <copyright file="UsageQuotaRepository.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Application.Interfaces;
using FileConversionApi.Domain.Entities;
using FileConversionApi.Domain.ValueObjects;

using Microsoft.EntityFrameworkCore;

namespace FileConversionApi.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for usage quota data access.
/// </summary>
public class UsageQuotaRepository : IUsageQuotaRepository
{
    private readonly AppDbContext context;

    /// <summary>
    /// Initializes a new instance of the <see cref="UsageQuotaRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public UsageQuotaRepository(AppDbContext context)
    {
        this.context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc/>
    public async Task<UsageQuota?> GetByIdAsync(UsageQuotaId id, CancellationToken cancellationToken)
    {
        return await this.context.UsageQuotas
            .FirstOrDefaultAsync(q => q.Id == id, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<UsageQuota?> GetByUserAndMonthAsync(
        UserId userId,
        int year,
        int month,
        CancellationToken cancellationToken)
    {
        return await this.context.UsageQuotas
            .FirstOrDefaultAsync(
                q => q.UserId == userId && q.Year == year && q.Month == month,
                cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<UsageQuota>> GetByUserAsync(
        UserId userId,
        CancellationToken cancellationToken)
    {
        return await this.context.UsageQuotas
            .Where(q => q.UserId == userId)
            .OrderByDescending(q => q.Year)
            .ThenByDescending(q => q.Month)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task AddAsync(UsageQuota quota, CancellationToken cancellationToken)
    {
        await this.context.UsageQuotas.AddAsync(quota, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public void Update(UsageQuota quota)
    {
        this.context.UsageQuotas.Update(quota);
    }
}
