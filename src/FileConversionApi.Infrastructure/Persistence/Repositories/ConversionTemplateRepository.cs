// <copyright file="ConversionTemplateRepository.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Application.Interfaces;
using FileConversionApi.Domain.Entities;
using FileConversionApi.Domain.ValueObjects;

using Microsoft.EntityFrameworkCore;

namespace FileConversionApi.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository for conversion template data access.
/// </summary>
public class ConversionTemplateRepository : IConversionTemplateRepository
{
    private readonly AppDbContext context;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConversionTemplateRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public ConversionTemplateRepository(AppDbContext context)
    {
        this.context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc/>
    public async Task<ConversionTemplate?> GetByIdAsync(
        ConversionTemplateId id,
        CancellationToken cancellationToken)
    {
        return await this.context.ConversionTemplates
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ConversionTemplate>> GetByUserIdAsync(
        UserId userId,
        CancellationToken cancellationToken)
    {
        return await this.context.ConversionTemplates
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ConversionTemplate>> GetByUserIdAndFormatAsync(
        UserId userId,
        string targetFormat,
        CancellationToken cancellationToken)
    {
        return await this.context.ConversionTemplates
            .Where(t => t.UserId == userId && t.TargetFormat == targetFormat)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<bool> NameExistsForUserAsync(
        UserId userId,
        string name,
        CancellationToken cancellationToken)
    {
        return await this.context.ConversionTemplates
            .AnyAsync(
                t => t.UserId == userId && t.Name.ToLower() == name.ToLower(),
                cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<bool> NameExistsForUserAsync(
        UserId userId,
        string name,
        ConversionTemplateId excludeTemplateId,
        CancellationToken cancellationToken)
    {
        return await this.context.ConversionTemplates
            .AnyAsync(
                t => t.UserId == userId && t.Name.ToLower() == name.ToLower() && t.Id != excludeTemplateId,
                cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task AddAsync(ConversionTemplate template, CancellationToken cancellationToken)
    {
        await this.context.ConversionTemplates.AddAsync(template, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public void Update(ConversionTemplate template)
    {
        this.context.ConversionTemplates.Update(template);
    }

    /// <inheritdoc/>
    public void Delete(ConversionTemplate template)
    {
        this.context.ConversionTemplates.Remove(template);
    }
}
