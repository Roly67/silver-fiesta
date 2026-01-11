// <copyright file="ConversionJobRepository.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Application.Interfaces;
using FileConversionApi.Domain.Entities;
using FileConversionApi.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FileConversionApi.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository for conversion job data access.
/// </summary>
public class ConversionJobRepository : IConversionJobRepository
{
    private readonly AppDbContext context;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConversionJobRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public ConversionJobRepository(AppDbContext context)
    {
        this.context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc/>
    public async Task<ConversionJob?> GetByIdAsync(ConversionJobId id, CancellationToken cancellationToken)
    {
        return await this.context.ConversionJobs
            .FirstOrDefaultAsync(j => j.Id == id, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<ConversionJob?> GetByIdForUserAsync(
        ConversionJobId id,
        UserId userId,
        CancellationToken cancellationToken)
    {
        return await this.context.ConversionJobs
            .FirstOrDefaultAsync(j => j.Id == id && j.UserId == userId, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ConversionJob>> GetByUserIdAsync(
        UserId userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        return await this.context.ConversionJobs
            .Where(j => j.UserId == userId)
            .OrderByDescending(j => j.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<int> GetCountByUserIdAsync(UserId userId, CancellationToken cancellationToken)
    {
        return await this.context.ConversionJobs
            .CountAsync(j => j.UserId == userId, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task AddAsync(ConversionJob job, CancellationToken cancellationToken)
    {
        await this.context.ConversionJobs.AddAsync(job, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public void Update(ConversionJob job)
    {
        this.context.ConversionJobs.Update(job);
    }
}
