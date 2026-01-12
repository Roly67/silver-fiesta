// <copyright file="JobCleanupServiceTests.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Domain.Entities;
using FileConversionApi.Domain.Enums;
using FileConversionApi.Domain.ValueObjects;
using FileConversionApi.Infrastructure.Options;
using FileConversionApi.Infrastructure.Persistence;
using FileConversionApi.Infrastructure.Services;

using FluentAssertions;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Moq;

namespace FileConversionApi.UnitTests.Infrastructure;

/// <summary>
/// Unit tests for the <see cref="JobCleanupService"/> class.
/// </summary>
public class JobCleanupServiceTests : IDisposable
{
    private readonly Mock<ILogger<JobCleanupService>> loggerMock;
    private readonly ServiceProvider serviceProvider;
    private readonly string databaseName;
    private readonly UserId testUserId;
    private bool disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="JobCleanupServiceTests"/> class.
    /// </summary>
    public JobCleanupServiceTests()
    {
        this.loggerMock = new Mock<ILogger<JobCleanupService>>();
        this.testUserId = UserId.New();
        this.databaseName = Guid.NewGuid().ToString();

        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase(databaseName: this.databaseName));

        this.serviceProvider = services.BuildServiceProvider();
    }

    /// <summary>
    /// Tests that constructor throws ArgumentNullException when scopeFactory is null.
    /// </summary>
    [Fact]
    public void Constructor_WhenScopeFactoryIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        var settings = Options.Create(new JobCleanupSettings());

        // Act
        var act = () => new JobCleanupService(null!, settings, this.loggerMock.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("scopeFactory");
    }

    /// <summary>
    /// Tests that constructor throws ArgumentNullException when settings is null.
    /// </summary>
    [Fact]
    public void Constructor_WhenSettingsIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        var scopeFactory = this.serviceProvider.GetRequiredService<IServiceScopeFactory>();

        // Act
        var act = () => new JobCleanupService(scopeFactory, null!, this.loggerMock.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("settings");
    }

    /// <summary>
    /// Tests that constructor throws ArgumentNullException when logger is null.
    /// </summary>
    [Fact]
    public void Constructor_WhenLoggerIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        var scopeFactory = this.serviceProvider.GetRequiredService<IServiceScopeFactory>();
        var settings = Options.Create(new JobCleanupSettings());

        // Act
        var act = () => new JobCleanupService(scopeFactory, settings, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    /// <summary>
    /// Tests that CleanupExpiredJobsAsync deletes expired completed jobs.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task CleanupExpiredJobsAsync_DeletesExpiredCompletedJobs()
    {
        // Arrange
        var settings = new JobCleanupSettings
        {
            Enabled = true,
            CompletedJobRetentionDays = 7,
            FailedJobRetentionDays = 30,
            BatchSize = 100,
        };

        var expiredJob = this.CreateCompletedJob(daysAgo: 10);
        var recentJob = this.CreateCompletedJob(daysAgo: 3);

        using (var setupScope = this.serviceProvider.CreateScope())
        {
            var setupContext = setupScope.ServiceProvider.GetRequiredService<AppDbContext>();
            await setupContext.ConversionJobs.AddRangeAsync(expiredJob, recentJob);
            await setupContext.SaveChangesAsync();
        }

        var service = this.CreateService(settings);

        // Act
        await service.CleanupExpiredJobsAsync(CancellationToken.None);

        // Assert
        using var verifyScope = this.serviceProvider.CreateScope();
        var verifyContext = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var remainingJobs = await verifyContext.ConversionJobs.ToListAsync();
        remainingJobs.Should().HaveCount(1);
        remainingJobs.Should().Contain(j => j.Id == recentJob.Id);
    }

    /// <summary>
    /// Tests that CleanupExpiredJobsAsync deletes expired failed jobs.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task CleanupExpiredJobsAsync_DeletesExpiredFailedJobs()
    {
        // Arrange
        var settings = new JobCleanupSettings
        {
            Enabled = true,
            CompletedJobRetentionDays = 7,
            FailedJobRetentionDays = 30,
            BatchSize = 100,
        };

        var expiredJob = this.CreateFailedJob(daysAgo: 35);
        var recentJob = this.CreateFailedJob(daysAgo: 10);

        using (var setupScope = this.serviceProvider.CreateScope())
        {
            var setupContext = setupScope.ServiceProvider.GetRequiredService<AppDbContext>();
            await setupContext.ConversionJobs.AddRangeAsync(expiredJob, recentJob);
            await setupContext.SaveChangesAsync();
        }

        var service = this.CreateService(settings);

        // Act
        await service.CleanupExpiredJobsAsync(CancellationToken.None);

        // Assert
        using var verifyScope = this.serviceProvider.CreateScope();
        var verifyContext = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var remainingJobs = await verifyContext.ConversionJobs.ToListAsync();
        remainingJobs.Should().HaveCount(1);
        remainingJobs.Should().Contain(j => j.Id == recentJob.Id);
    }

    /// <summary>
    /// Tests that CleanupExpiredJobsAsync keeps pending jobs.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task CleanupExpiredJobsAsync_KeepsPendingJobs()
    {
        // Arrange
        var settings = new JobCleanupSettings
        {
            Enabled = true,
            CompletedJobRetentionDays = 7,
            FailedJobRetentionDays = 30,
            BatchSize = 100,
        };

        var pendingJob = ConversionJob.Create(this.testUserId, "html", "pdf", "test.html", null);

        using (var setupScope = this.serviceProvider.CreateScope())
        {
            var setupContext = setupScope.ServiceProvider.GetRequiredService<AppDbContext>();
            await setupContext.ConversionJobs.AddAsync(pendingJob);
            await setupContext.SaveChangesAsync();
        }

        var service = this.CreateService(settings);

        // Act
        await service.CleanupExpiredJobsAsync(CancellationToken.None);

        // Assert
        using var verifyScope = this.serviceProvider.CreateScope();
        var verifyContext = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var remainingJobs = await verifyContext.ConversionJobs.ToListAsync();
        remainingJobs.Should().HaveCount(1);
        remainingJobs.Should().Contain(j => j.Id == pendingJob.Id);
    }

    /// <summary>
    /// Tests that CleanupExpiredJobsAsync keeps processing jobs.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task CleanupExpiredJobsAsync_KeepsProcessingJobs()
    {
        // Arrange
        var settings = new JobCleanupSettings
        {
            Enabled = true,
            CompletedJobRetentionDays = 7,
            FailedJobRetentionDays = 30,
            BatchSize = 100,
        };

        var processingJob = ConversionJob.Create(this.testUserId, "html", "pdf", "test.html", null);
        processingJob.MarkAsProcessing();

        using (var setupScope = this.serviceProvider.CreateScope())
        {
            var setupContext = setupScope.ServiceProvider.GetRequiredService<AppDbContext>();
            await setupContext.ConversionJobs.AddAsync(processingJob);
            await setupContext.SaveChangesAsync();
        }

        var service = this.CreateService(settings);

        // Act
        await service.CleanupExpiredJobsAsync(CancellationToken.None);

        // Assert
        using var verifyScope = this.serviceProvider.CreateScope();
        var verifyContext = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var remainingJobs = await verifyContext.ConversionJobs.ToListAsync();
        remainingJobs.Should().HaveCount(1);
        remainingJobs.Should().Contain(j => j.Id == processingJob.Id);
    }

    /// <summary>
    /// Tests that CleanupExpiredJobsAsync handles empty database gracefully.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task CleanupExpiredJobsAsync_WhenNoJobs_HandlesGracefully()
    {
        // Arrange
        var settings = new JobCleanupSettings
        {
            Enabled = true,
            CompletedJobRetentionDays = 7,
            FailedJobRetentionDays = 30,
            BatchSize = 100,
        };

        var service = this.CreateService(settings);

        // Act
        var act = () => service.CleanupExpiredJobsAsync(CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
    }

    /// <summary>
    /// Tests that CleanupExpiredJobsAsync uses different retention periods for completed and failed jobs.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task CleanupExpiredJobsAsync_UsesDifferentRetentionPeriods()
    {
        // Arrange
        var settings = new JobCleanupSettings
        {
            Enabled = true,
            CompletedJobRetentionDays = 7,
            FailedJobRetentionDays = 30,
            BatchSize = 100,
        };

        // 15 days old completed job (expired, > 7 days)
        var expiredCompletedJob = this.CreateCompletedJob(daysAgo: 15);

        // 15 days old failed job (not expired, < 30 days)
        var recentFailedJob = this.CreateFailedJob(daysAgo: 15);

        using (var setupScope = this.serviceProvider.CreateScope())
        {
            var setupContext = setupScope.ServiceProvider.GetRequiredService<AppDbContext>();
            await setupContext.ConversionJobs.AddRangeAsync(expiredCompletedJob, recentFailedJob);
            await setupContext.SaveChangesAsync();
        }

        var service = this.CreateService(settings);

        // Act
        await service.CleanupExpiredJobsAsync(CancellationToken.None);

        // Assert
        using var verifyScope = this.serviceProvider.CreateScope();
        var verifyContext = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var remainingJobs = await verifyContext.ConversionJobs.ToListAsync();
        remainingJobs.Should().HaveCount(1);
        remainingJobs.Should().Contain(j => j.Id == recentFailedJob.Id);
    }

    /// <summary>
    /// Tests that CleanupExpiredJobsAsync respects batch size limit.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task CleanupExpiredJobsAsync_RespectsBatchSizeLimit()
    {
        // Arrange
        var settings = new JobCleanupSettings
        {
            Enabled = true,
            CompletedJobRetentionDays = 7,
            FailedJobRetentionDays = 30,
            BatchSize = 2,
        };

        // Create 5 expired completed jobs
        var expiredJobs = new List<ConversionJob>();
        for (var i = 0; i < 5; i++)
        {
            expiredJobs.Add(this.CreateCompletedJob(daysAgo: 10));
        }

        using (var setupScope = this.serviceProvider.CreateScope())
        {
            var setupContext = setupScope.ServiceProvider.GetRequiredService<AppDbContext>();
            await setupContext.ConversionJobs.AddRangeAsync(expiredJobs);
            await setupContext.SaveChangesAsync();
        }

        var service = this.CreateService(settings);

        // Act
        await service.CleanupExpiredJobsAsync(CancellationToken.None);

        // Assert - only 2 jobs should be deleted due to batch size
        using var verifyScope = this.serviceProvider.CreateScope();
        var verifyContext = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var remainingJobs = await verifyContext.ConversionJobs.ToListAsync();
        remainingJobs.Should().HaveCount(3);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the test resources.
    /// </summary>
    /// <param name="disposing">Whether to dispose managed resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!this.disposed)
        {
            if (disposing)
            {
                this.serviceProvider.Dispose();
            }

            this.disposed = true;
        }
    }

    private ConversionJob CreateCompletedJob(int daysAgo)
    {
        var job = ConversionJob.Create(this.testUserId, "html", "pdf", "test.html", null);
        job.MarkAsProcessing();
        job.MarkAsCompleted("output.pdf", [0x25, 0x50, 0x44, 0x46]);

        // Use reflection to set CompletedAt to a past date
        var completedAtProperty = typeof(ConversionJob).GetProperty("CompletedAt");
        completedAtProperty?.SetValue(job, DateTimeOffset.UtcNow.AddDays(-daysAgo));

        return job;
    }

    private ConversionJob CreateFailedJob(int daysAgo)
    {
        var job = ConversionJob.Create(this.testUserId, "html", "pdf", "test.html", null);
        job.MarkAsProcessing();
        job.MarkAsFailed("Test failure");

        // Use reflection to set CompletedAt to a past date
        var completedAtProperty = typeof(ConversionJob).GetProperty("CompletedAt");
        completedAtProperty?.SetValue(job, DateTimeOffset.UtcNow.AddDays(-daysAgo));

        return job;
    }

    private JobCleanupService CreateService(JobCleanupSettings settings)
    {
        var scopeFactory = this.serviceProvider.GetRequiredService<IServiceScopeFactory>();
        var options = Options.Create(settings);
        return new JobCleanupService(scopeFactory, options, this.loggerMock.Object);
    }
}
