// <copyright file="UnitOfWorkTests.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Domain.Entities;
using FileConversionApi.Domain.ValueObjects;
using FileConversionApi.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace FileConversionApi.UnitTests.Infrastructure;

/// <summary>
/// Unit tests for the <see cref="UnitOfWork"/> class.
/// </summary>
public class UnitOfWorkTests : IDisposable
{
    private readonly AppDbContext context;
    private readonly UnitOfWork unitOfWork;
    private bool disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="UnitOfWorkTests"/> class.
    /// </summary>
    public UnitOfWorkTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        this.context = new AppDbContext(options);
        this.unitOfWork = new UnitOfWork(this.context);
    }

    /// <summary>
    /// Tests that constructor throws ArgumentNullException when context is null.
    /// </summary>
    [Fact]
    public void Constructor_WhenContextIsNull_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new UnitOfWork(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("context");
    }

    /// <summary>
    /// Tests that SaveChangesAsync saves pending changes.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task SaveChangesAsync_WhenCalled_SavesPendingChanges()
    {
        // Arrange
        var user = User.Create("test@example.com", "hashedPassword");
        await this.context.Users.AddAsync(user);

        // Act
        var result = await this.unitOfWork.SaveChangesAsync(CancellationToken.None);

        // Assert
        result.Should().Be(1);
        var savedUser = await this.context.Users.FirstOrDefaultAsync(u => u.Email == "test@example.com");
        savedUser.Should().NotBeNull();
    }

    /// <summary>
    /// Tests that SaveChangesAsync returns zero when no changes pending.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task SaveChangesAsync_WhenNoChangesPending_ReturnsZero()
    {
        // Act
        var result = await this.unitOfWork.SaveChangesAsync(CancellationToken.None);

        // Assert
        result.Should().Be(0);
    }

    /// <summary>
    /// Tests that SaveChangesAsync saves multiple entities.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task SaveChangesAsync_WhenMultipleEntitiesAdded_SavesAllEntities()
    {
        // Arrange
        var user1 = User.Create("user1@example.com", "hashedPassword");
        var user2 = User.Create("user2@example.com", "hashedPassword");
        var job = ConversionJob.Create(user1.Id, "html", "pdf", "test.html", null);

        await this.context.Users.AddAsync(user1);
        await this.context.Users.AddAsync(user2);
        await this.context.ConversionJobs.AddAsync(job);

        // Act
        var result = await this.unitOfWork.SaveChangesAsync(CancellationToken.None);

        // Assert
        result.Should().Be(3);
    }

    /// <summary>
    /// Tests that SaveChangesAsync tracks updates correctly.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task SaveChangesAsync_WhenEntityUpdated_SavesUpdate()
    {
        // Arrange
        var user = User.Create("update@example.com", "hashedPassword");
        await this.context.Users.AddAsync(user);
        await this.context.SaveChangesAsync();

        user.RegenerateApiKey();

        // Act
        var result = await this.unitOfWork.SaveChangesAsync(CancellationToken.None);

        // Assert
        result.Should().Be(1);
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
