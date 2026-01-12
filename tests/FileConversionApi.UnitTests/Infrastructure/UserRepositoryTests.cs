// <copyright file="UserRepositoryTests.cs" company="FileConversionApi">
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
/// Unit tests for the <see cref="UserRepository"/> class.
/// </summary>
public class UserRepositoryTests : IDisposable
{
    private readonly AppDbContext context;
    private readonly UserRepository repository;
    private bool disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserRepositoryTests"/> class.
    /// </summary>
    public UserRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        this.context = new AppDbContext(options);
        this.repository = new UserRepository(this.context);
    }

    /// <summary>
    /// Tests that constructor throws ArgumentNullException when context is null.
    /// </summary>
    [Fact]
    public void Constructor_WhenContextIsNull_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new UserRepository(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("context");
    }

    /// <summary>
    /// Tests that GetByIdAsync returns user when user exists.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetByIdAsync_WhenUserExists_ReturnsUser()
    {
        // Arrange
        var user = User.Create("test@example.com", "hashedPassword");
        await this.context.Users.AddAsync(user);
        await this.context.SaveChangesAsync();

        // Act
        var result = await this.repository.GetByIdAsync(user.Id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
        result.Email.Should().Be("test@example.com");
    }

    /// <summary>
    /// Tests that GetByIdAsync returns null when user does not exist.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetByIdAsync_WhenUserDoesNotExist_ReturnsNull()
    {
        // Arrange
        var nonExistentId = UserId.New();

        // Act
        var result = await this.repository.GetByIdAsync(nonExistentId, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    /// Tests that GetByEmailAsync returns user when user exists.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetByEmailAsync_WhenUserExists_ReturnsUser()
    {
        // Arrange
        var user = User.Create("findme@example.com", "hashedPassword");
        await this.context.Users.AddAsync(user);
        await this.context.SaveChangesAsync();

        // Act
        var result = await this.repository.GetByEmailAsync("findme@example.com", CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be("findme@example.com");
    }

    /// <summary>
    /// Tests that GetByEmailAsync returns null when user does not exist.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetByEmailAsync_WhenUserDoesNotExist_ReturnsNull()
    {
        // Act
        var result = await this.repository.GetByEmailAsync("nonexistent@example.com", CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    /// Tests that GetByApiKeyAsync returns user when user exists.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetByApiKeyAsync_WhenUserExists_ReturnsUser()
    {
        // Arrange
        var user = User.Create("apikey@example.com", "hashedPassword");
        await this.context.Users.AddAsync(user);
        await this.context.SaveChangesAsync();

        // Act
        var result = await this.repository.GetByApiKeyAsync(user.ApiKey, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.ApiKey.Should().Be(user.ApiKey);
    }

    /// <summary>
    /// Tests that GetByApiKeyAsync returns null when user does not exist.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetByApiKeyAsync_WhenUserDoesNotExist_ReturnsNull()
    {
        // Act
        var result = await this.repository.GetByApiKeyAsync("nonexistent-api-key", CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    /// Tests that EmailExistsAsync returns true when email exists.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task EmailExistsAsync_WhenEmailExists_ReturnsTrue()
    {
        // Arrange
        var user = User.Create("exists@example.com", "hashedPassword");
        await this.context.Users.AddAsync(user);
        await this.context.SaveChangesAsync();

        // Act
        var result = await this.repository.EmailExistsAsync("exists@example.com", CancellationToken.None);

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Tests that EmailExistsAsync returns false when email does not exist.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task EmailExistsAsync_WhenEmailDoesNotExist_ReturnsFalse()
    {
        // Act
        var result = await this.repository.EmailExistsAsync("notexists@example.com", CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Tests that AddAsync adds user to context.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task AddAsync_WhenCalled_AddsUserToContext()
    {
        // Arrange
        var user = User.Create("newuser@example.com", "hashedPassword");

        // Act
        await this.repository.AddAsync(user, CancellationToken.None);
        await this.context.SaveChangesAsync();

        // Assert
        var savedUser = await this.context.Users.FirstOrDefaultAsync(u => u.Email == "newuser@example.com");
        savedUser.Should().NotBeNull();
        savedUser!.Id.Should().Be(user.Id);
    }

    /// <summary>
    /// Tests that Update updates user in context.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Update_WhenCalled_UpdatesUserInContext()
    {
        // Arrange
        var user = User.Create("update@example.com", "hashedPassword");
        await this.context.Users.AddAsync(user);
        await this.context.SaveChangesAsync();
        this.context.Entry(user).State = EntityState.Detached;

        // Act
        user.RegenerateApiKey();
        this.repository.Update(user);
        await this.context.SaveChangesAsync();

        // Assert
        var updatedUser = await this.context.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
        updatedUser.Should().NotBeNull();
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
