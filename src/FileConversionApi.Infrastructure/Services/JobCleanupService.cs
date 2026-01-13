// <copyright file="JobCleanupService.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Domain.Enums;
using FileConversionApi.Infrastructure.Options;
using FileConversionApi.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FileConversionApi.Infrastructure.Services;

/// <summary>
/// Background service that periodically cleans up expired conversion jobs.
/// </summary>
public class JobCleanupService : BackgroundService
{
    private readonly IServiceScopeFactory scopeFactory;
    private readonly JobCleanupSettings settings;
    private readonly ILogger<JobCleanupService> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="JobCleanupService"/> class.
    /// </summary>
    /// <param name="scopeFactory">The service scope factory.</param>
    /// <param name="settings">The job cleanup settings.</param>
    /// <param name="logger">The logger.</param>
    public JobCleanupService(
        IServiceScopeFactory scopeFactory,
        IOptions<JobCleanupSettings> settings,
        ILogger<JobCleanupService> logger)
    {
        this.scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        this.settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Cleans up expired conversion jobs from the database.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    internal async Task CleanupExpiredJobsAsync(CancellationToken cancellationToken)
    {
        using var scope = this.scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var completedCutoff = DateTimeOffset.UtcNow.AddDays(-this.settings.CompletedJobRetentionDays);
        var failedCutoff = DateTimeOffset.UtcNow.AddDays(-this.settings.FailedJobRetentionDays);

        // Find and delete expired completed jobs
        var expiredCompletedJobs = await dbContext.ConversionJobs
            .Where(j => j.Status == ConversionStatus.Completed
                && j.CompletedAt.HasValue
                && j.CompletedAt.Value < completedCutoff)
            .Take(this.settings.BatchSize)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        dbContext.ConversionJobs.RemoveRange(expiredCompletedJobs);

        // Find and delete expired failed jobs
        var expiredFailedJobs = await dbContext.ConversionJobs
            .Where(j => j.Status == ConversionStatus.Failed
                && j.CompletedAt.HasValue
                && j.CompletedAt.Value < failedCutoff)
            .Take(this.settings.BatchSize)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        dbContext.ConversionJobs.RemoveRange(expiredFailedJobs);

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var completedDeleted = expiredCompletedJobs.Count;
        var failedDeleted = expiredFailedJobs.Count;

        if (completedDeleted > 0 || failedDeleted > 0)
        {
            this.logger.LogInformation(
                "Job cleanup completed: {CompletedDeleted} completed jobs, {FailedDeleted} failed jobs deleted",
                completedDeleted,
                failedDeleted);
        }
        else
        {
            this.logger.LogDebug("Job cleanup completed: no expired jobs found");
        }
    }

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!this.settings.Enabled)
        {
            this.logger.LogInformation("Job cleanup service is disabled");
            return;
        }

        this.logger.LogInformation(
            "Job cleanup service started. Running every {IntervalMinutes} minutes. " +
            "Completed job retention: {CompletedDays} days, Failed job retention: {FailedDays} days",
            this.settings.RunIntervalMinutes,
            this.settings.CompletedJobRetentionDays,
            this.settings.FailedJobRetentionDays);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await this.CleanupExpiredJobsAsync(stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Graceful shutdown, don't log as error
                break;
            }
            catch (Exception ex) when (ex is not (OutOfMemoryException or StackOverflowException))
            {
                this.logger.LogError(ex, "Error occurred during job cleanup");
            }

            try
            {
                await Task.Delay(
                    TimeSpan.FromMinutes(this.settings.RunIntervalMinutes),
                    stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Graceful shutdown
                break;
            }
        }

        this.logger.LogInformation("Job cleanup service stopped");
    }
}
