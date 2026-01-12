// <copyright file="ConversionJobRepositoryTests.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Domain.Entities;
using FileConversionApi.Domain.ValueObjects;
using FileConversionApi.Infrastructure.Persistence;
using FileConversionApi.Infrastructure.Persistence.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace FileConversionApi.UnitTests.Infrastructure;

/// <summary>
/// Unit tests for the <see cref="ConversionJobRepository"/> class.
/// </summary>
public class ConversionJobRepositoryTests : IDisposable
{
    private readonly AppDbContext context;
    private readonly ConversionJobRepository repository;
    private readonly UserId testUserId;
    private bool disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConversionJobRepositoryTests"/> class.
    /// </summary>
    public ConversionJobRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        this.context = new AppDbContext(options);
        this.repository = new ConversionJobRepository(this.context);
        this.testUserId = UserId.New();
    }

    /// <summary>
    /// Tests that constructor throws ArgumentNullException when context is null.
    /// </summary>
    [Fact]
    public void Constructor_WhenContextIsNull_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new ConversionJobRepository(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("context");
    }

    /// <summary>
    /// Tests that GetByIdAsync returns job when job exists.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetByIdAsync_WhenJobExists_ReturnsJob()
    {
        // Arrange
        var job = ConversionJob.Create(this.testUserId, "html", "pdf", "test.html", null);
        await this.context.ConversionJobs.AddAsync(job);
        await this.context.SaveChangesAsync();

        // Act
        var result = await this.repository.GetByIdAsync(job.Id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(job.Id);
        result.SourceFormat.Should().Be("html");
    }

    /// <summary>
    /// Tests that GetByIdAsync returns null when job does not exist.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetByIdAsync_WhenJobDoesNotExist_ReturnsNull()
    {
        // Arrange
        var nonExistentId = ConversionJobId.New();

        // Act
        var result = await this.repository.GetByIdAsync(nonExistentId, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    /// Tests that GetByIdForUserAsync returns job when job exists and belongs to user.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetByIdForUserAsync_WhenJobExistsAndBelongsToUser_ReturnsJob()
    {
        // Arrange
        var job = ConversionJob.Create(this.testUserId, "html", "pdf", "test.html", null);
        await this.context.ConversionJobs.AddAsync(job);
        await this.context.SaveChangesAsync();

        // Act
        var result = await this.repository.GetByIdForUserAsync(job.Id, this.testUserId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(job.Id);
        result.UserId.Should().Be(this.testUserId);
    }

    /// <summary>
    /// Tests that GetByIdForUserAsync returns null when job belongs to different user.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetByIdForUserAsync_WhenJobBelongsToDifferentUser_ReturnsNull()
    {
        // Arrange
        var job = ConversionJob.Create(this.testUserId, "html", "pdf", "test.html", null);
        await this.context.ConversionJobs.AddAsync(job);
        await this.context.SaveChangesAsync();

        var differentUserId = UserId.New();

        // Act
        var result = await this.repository.GetByIdForUserAsync(job.Id, differentUserId, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    /// Tests that GetByIdForUserAsync returns null when job does not exist.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetByIdForUserAsync_WhenJobDoesNotExist_ReturnsNull()
    {
        // Arrange
        var nonExistentId = ConversionJobId.New();

        // Act
        var result = await this.repository.GetByIdForUserAsync(nonExistentId, this.testUserId, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    /// Tests that GetByUserIdAsync returns jobs ordered by creation date descending.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetByUserIdAsync_WhenJobsExist_ReturnsJobsOrderedByCreatedAtDescending()
    {
        // Arrange
        var job1 = ConversionJob.Create(this.testUserId, "html", "pdf", "first.html", null);
        var job2 = ConversionJob.Create(this.testUserId, "markdown", "pdf", "second.md", null);
        var job3 = ConversionJob.Create(this.testUserId, "html", "pdf", "third.html", null);

        await this.context.ConversionJobs.AddAsync(job1);
        await this.context.SaveChangesAsync();
        await Task.Delay(10); // Ensure different timestamps

        await this.context.ConversionJobs.AddAsync(job2);
        await this.context.SaveChangesAsync();
        await Task.Delay(10);

        await this.context.ConversionJobs.AddAsync(job3);
        await this.context.SaveChangesAsync();

        // Act
        var result = await this.repository.GetByUserIdAsync(this.testUserId, 1, 10, CancellationToken.None);

        // Assert
        result.Should().HaveCount(3);
        result[0].InputFileName.Should().Be("third.html");
        result[1].InputFileName.Should().Be("second.md");
        result[2].InputFileName.Should().Be("first.html");
    }

    /// <summary>
    /// Tests that GetByUserIdAsync returns empty list when no jobs exist.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetByUserIdAsync_WhenNoJobsExist_ReturnsEmptyList()
    {
        // Act
        var result = await this.repository.GetByUserIdAsync(this.testUserId, 1, 10, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that GetByUserIdAsync returns only jobs for specified user.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetByUserIdAsync_WhenMultipleUsersHaveJobs_ReturnsOnlyUserJobs()
    {
        // Arrange
        var otherUserId = UserId.New();
        var userJob = ConversionJob.Create(this.testUserId, "html", "pdf", "user.html", null);
        var otherJob = ConversionJob.Create(otherUserId, "html", "pdf", "other.html", null);

        await this.context.ConversionJobs.AddAsync(userJob);
        await this.context.ConversionJobs.AddAsync(otherJob);
        await this.context.SaveChangesAsync();

        // Act
        var result = await this.repository.GetByUserIdAsync(this.testUserId, 1, 10, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result[0].InputFileName.Should().Be("user.html");
    }

    /// <summary>
    /// Tests that GetByUserIdAsync applies pagination correctly.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetByUserIdAsync_WhenPaginationApplied_ReturnsCorrectPage()
    {
        // Arrange
        for (int i = 0; i < 5; i++)
        {
            var job = ConversionJob.Create(this.testUserId, "html", "pdf", $"file{i}.html", null);
            await this.context.ConversionJobs.AddAsync(job);
            await this.context.SaveChangesAsync();
            await Task.Delay(10);
        }

        // Act
        var page1 = await this.repository.GetByUserIdAsync(this.testUserId, 1, 2, CancellationToken.None);
        var page2 = await this.repository.GetByUserIdAsync(this.testUserId, 2, 2, CancellationToken.None);
        var page3 = await this.repository.GetByUserIdAsync(this.testUserId, 3, 2, CancellationToken.None);

        // Assert
        page1.Should().HaveCount(2);
        page2.Should().HaveCount(2);
        page3.Should().HaveCount(1);
    }

    /// <summary>
    /// Tests that GetCountByUserIdAsync returns correct count.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetCountByUserIdAsync_WhenJobsExist_ReturnsCorrectCount()
    {
        // Arrange
        var job1 = ConversionJob.Create(this.testUserId, "html", "pdf", "test1.html", null);
        var job2 = ConversionJob.Create(this.testUserId, "html", "pdf", "test2.html", null);
        var job3 = ConversionJob.Create(this.testUserId, "html", "pdf", "test3.html", null);

        await this.context.ConversionJobs.AddRangeAsync(job1, job2, job3);
        await this.context.SaveChangesAsync();

        // Act
        var result = await this.repository.GetCountByUserIdAsync(this.testUserId, CancellationToken.None);

        // Assert
        result.Should().Be(3);
    }

    /// <summary>
    /// Tests that GetCountByUserIdAsync returns zero when no jobs exist.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetCountByUserIdAsync_WhenNoJobsExist_ReturnsZero()
    {
        // Act
        var result = await this.repository.GetCountByUserIdAsync(this.testUserId, CancellationToken.None);

        // Assert
        result.Should().Be(0);
    }

    /// <summary>
    /// Tests that GetCountByUserIdAsync counts only jobs for specified user.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetCountByUserIdAsync_WhenMultipleUsersHaveJobs_CountsOnlyUserJobs()
    {
        // Arrange
        var otherUserId = UserId.New();
        var userJob1 = ConversionJob.Create(this.testUserId, "html", "pdf", "user1.html", null);
        var userJob2 = ConversionJob.Create(this.testUserId, "html", "pdf", "user2.html", null);
        var otherJob = ConversionJob.Create(otherUserId, "html", "pdf", "other.html", null);

        await this.context.ConversionJobs.AddRangeAsync(userJob1, userJob2, otherJob);
        await this.context.SaveChangesAsync();

        // Act
        var result = await this.repository.GetCountByUserIdAsync(this.testUserId, CancellationToken.None);

        // Assert
        result.Should().Be(2);
    }

    /// <summary>
    /// Tests that AddAsync adds job to context.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task AddAsync_WhenCalled_AddsJobToContext()
    {
        // Arrange
        var job = ConversionJob.Create(this.testUserId, "html", "pdf", "new.html", null);

        // Act
        await this.repository.AddAsync(job, CancellationToken.None);
        await this.context.SaveChangesAsync();

        // Assert
        var savedJob = await this.context.ConversionJobs.FirstOrDefaultAsync(j => j.Id == job.Id);
        savedJob.Should().NotBeNull();
        savedJob!.InputFileName.Should().Be("new.html");
    }

    /// <summary>
    /// Tests that Update updates job in context.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Update_WhenCalled_UpdatesJobInContext()
    {
        // Arrange
        var job = ConversionJob.Create(this.testUserId, "html", "pdf", "update.html", null);
        await this.context.ConversionJobs.AddAsync(job);
        await this.context.SaveChangesAsync();
        this.context.Entry(job).State = EntityState.Detached;

        // Act
        job.MarkAsCompleted("output.pdf", new byte[] { 1, 2, 3 });
        this.repository.Update(job);
        await this.context.SaveChangesAsync();

        // Assert
        var updatedJob = await this.context.ConversionJobs.FirstOrDefaultAsync(j => j.Id == job.Id);
        updatedJob.Should().NotBeNull();
        updatedJob!.OutputFileName.Should().Be("output.pdf");
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
                this.context.Dispose();
            }

            this.disposed = true;
        }
    }
}
