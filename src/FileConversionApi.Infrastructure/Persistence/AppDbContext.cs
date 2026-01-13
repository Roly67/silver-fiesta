// <copyright file="AppDbContext.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FileConversionApi.Infrastructure.Persistence;

/// <summary>
/// The application database context.
/// </summary>
public class AppDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AppDbContext"/> class.
    /// </summary>
    /// <param name="options">The database context options.</param>
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Gets the users.
    /// </summary>
    public DbSet<User> Users => this.Set<User>();

    /// <summary>
    /// Gets the conversion jobs.
    /// </summary>
    public DbSet<ConversionJob> ConversionJobs => this.Set<ConversionJob>();

    /// <summary>
    /// Gets the conversion templates.
    /// </summary>
    public DbSet<ConversionTemplate> ConversionTemplates => this.Set<ConversionTemplate>();

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
