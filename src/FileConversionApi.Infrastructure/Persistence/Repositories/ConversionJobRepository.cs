// <copyright file="ConversionJobRepository.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Application.Interfaces;
using FileConversionApi.Domain.Entities;
using FileConversionApi.Domain.Enums;
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

    /// <inheritdoc/>
    public async Task<(int Total, int Completed, int Failed, int Pending)> GetStatisticsAsync(CancellationToken cancellationToken)
    {
        var total = await this.context.ConversionJobs
            .CountAsync(cancellationToken)
            .ConfigureAwait(false);

        var completed = await this.context.ConversionJobs
            .CountAsync(j => j.Status == ConversionStatus.Completed, cancellationToken)
            .ConfigureAwait(false);

        var failed = await this.context.ConversionJobs
            .CountAsync(j => j.Status == ConversionStatus.Failed, cancellationToken)
            .ConfigureAwait(false);

        var pending = await this.context.ConversionJobs
            .CountAsync(j => j.Status == ConversionStatus.Pending || j.Status == ConversionStatus.Processing, cancellationToken)
            .ConfigureAwait(false);

        return (total, completed, failed, pending);
    }

    /// <inheritdoc/>
    public async Task<int> GetTotalCountAsync(CancellationToken cancellationToken)
    {
        return await this.context.ConversionJobs
            .CountAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
