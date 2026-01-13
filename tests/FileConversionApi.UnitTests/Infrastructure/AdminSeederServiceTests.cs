// <copyright file="AdminSeederServiceTests.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Application.Interfaces;
using FileConversionApi.Domain.Entities;
using FileConversionApi.Infrastructure.Options;
using FileConversionApi.Infrastructure.Services;

using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Moq;

namespace FileConversionApi.UnitTests.Infrastructure;

/// <summary>
/// Unit tests for the <see cref="AdminSeederService"/> class.
/// </summary>
public class AdminSeederServiceTests : IDisposable
{
    private readonly Mock<IUserRepository> userRepositoryMock;
    private readonly Mock<IPasswordHasher> passwordHasherMock;
    private readonly Mock<IUnitOfWork> unitOfWorkMock;
    private readonly Mock<ILogger<AdminSeederService>> loggerMock;
    private readonly ServiceProvider serviceProvider;
    private bool disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminSeederServiceTests"/> class.
    /// </summary>
    public AdminSeederServiceTests()
    {
        this.userRepositoryMock = new Mock<IUserRepository>();
        this.passwordHasherMock = new Mock<IPasswordHasher>();
        this.unitOfWorkMock = new Mock<IUnitOfWork>();
        this.loggerMock = new Mock<ILogger<AdminSeederService>>();

        var services = new ServiceCollection();
        services.AddScoped(_ => this.userRepositoryMock.Object);
        services.AddScoped(_ => this.passwordHasherMock.Object);
        services.AddScoped(_ => this.unitOfWorkMock.Object);

        this.serviceProvider = services.BuildServiceProvider();
    }

    /// <summary>
    /// Tests that constructor throws ArgumentNullException when scopeFactory is null.
    /// </summary>
    [Fact]
    public void Constructor_WhenScopeFactoryIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        var settings = Options.Create(new AdminSeedSettings());

        // Act
        var act = () => new AdminSeederService(null!, settings, this.loggerMock.Object);

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
        var act = () => new AdminSeederService(scopeFactory, null!, this.loggerMock.Object);

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
        var settings = Options.Create(new AdminSeedSettings());

        // Act
        var act = () => new AdminSeederService(scopeFactory, settings, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    /// <summary>
    /// Tests that StartAsync does nothing when seeding is disabled.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task StartAsync_WhenDisabled_DoesNothing()
    {
        // Arrange
        var settings = new AdminSeedSettings { Enabled = false };
        var service = this.CreateService(settings);

        // Act
        await service.StartAsync(CancellationToken.None);

        // Assert
        this.userRepositoryMock.Verify(
            r => r.AnyAdminExistsAsync(It.IsAny<CancellationToken>()),
            Times.Never);
        this.userRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    /// <summary>
    /// Tests that SeedAdminUserAsync skips seeding when admin already exists.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task SeedAdminUserAsync_WhenAdminExistsAndSkipEnabled_SkipsSeeding()
    {
        // Arrange
        var settings = new AdminSeedSettings
        {
            Enabled = true,
            SkipIfAdminExists = true,
            Email = "admin@test.local",
            Password = "TestPassword123!",
        };

        this.userRepositoryMock
            .Setup(r => r.AnyAdminExistsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var service = this.CreateService(settings);

        // Act
        await service.SeedAdminUserAsync(CancellationToken.None);

        // Assert
        this.userRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Never);
        this.unitOfWorkMock.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    /// <summary>
    /// Tests that SeedAdminUserAsync creates admin when no admin exists.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task SeedAdminUserAsync_WhenNoAdminExists_CreatesAdmin()
    {
        // Arrange
        var settings = new AdminSeedSettings
        {
            Enabled = true,
            SkipIfAdminExists = true,
            Email = "admin@test.local",
            Password = "TestPassword123!",
        };

        this.userRepositoryMock
            .Setup(r => r.AnyAdminExistsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        this.userRepositoryMock
            .Setup(r => r.EmailExistsAsync(settings.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        this.passwordHasherMock
            .Setup(p => p.Hash(settings.Password))
            .Returns("hashed_password");

        User? capturedUser = null;
        this.userRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((user, _) => capturedUser = user)
            .Returns(Task.CompletedTask);

        var service = this.CreateService(settings);

        // Act
        await service.SeedAdminUserAsync(CancellationToken.None);

        // Assert
        this.userRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Once);
        this.unitOfWorkMock.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);

        capturedUser.Should().NotBeNull();
        capturedUser!.Email.Should().Be(settings.Email);
        capturedUser.IsAdmin.Should().BeTrue();
    }

    /// <summary>
    /// Tests that SeedAdminUserAsync skips when email already exists.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task SeedAdminUserAsync_WhenEmailAlreadyExists_SkipsSeeding()
    {
        // Arrange
        var settings = new AdminSeedSettings
        {
            Enabled = true,
            SkipIfAdminExists = true,
            Email = "admin@test.local",
            Password = "TestPassword123!",
        };

        this.userRepositoryMock
            .Setup(r => r.AnyAdminExistsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        this.userRepositoryMock
            .Setup(r => r.EmailExistsAsync(settings.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var service = this.CreateService(settings);

        // Act
        await service.SeedAdminUserAsync(CancellationToken.None);

        // Assert
        this.userRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Never);
        this.unitOfWorkMock.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    /// <summary>
    /// Tests that SeedAdminUserAsync creates admin when SkipIfAdminExists is false.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task SeedAdminUserAsync_WhenSkipIfAdminExistsFalse_AlwaysAttemptsSeeding()
    {
        // Arrange
        var settings = new AdminSeedSettings
        {
            Enabled = true,
            SkipIfAdminExists = false,
            Email = "admin@test.local",
            Password = "TestPassword123!",
        };

        this.userRepositoryMock
            .Setup(r => r.EmailExistsAsync(settings.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        this.passwordHasherMock
            .Setup(p => p.Hash(settings.Password))
            .Returns("hashed_password");

        var service = this.CreateService(settings);

        // Act
        await service.SeedAdminUserAsync(CancellationToken.None);

        // Assert
        this.userRepositoryMock.Verify(
            r => r.AnyAdminExistsAsync(It.IsAny<CancellationToken>()),
            Times.Never);
        this.userRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that SeedAdminUserAsync handles exceptions gracefully.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task SeedAdminUserAsync_WhenExceptionOccurs_HandlesGracefully()
    {
        // Arrange
        var settings = new AdminSeedSettings
        {
            Enabled = true,
            SkipIfAdminExists = true,
            Email = "admin@test.local",
            Password = "TestPassword123!",
        };

        this.userRepositoryMock
            .Setup(r => r.AnyAdminExistsAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        var service = this.CreateService(settings);

        // Act
        var act = () => service.SeedAdminUserAsync(CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
    }

    /// <summary>
    /// Tests that StopAsync completes immediately.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task StopAsync_CompletesImmediately()
    {
        // Arrange
        var settings = new AdminSeedSettings { Enabled = true };
        var service = this.CreateService(settings);

        // Act
        var act = () => service.StopAsync(CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
    }

    /// <summary>
    /// Tests that SeedAdminUserAsync uses configured email.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task SeedAdminUserAsync_UsesConfiguredEmail()
    {
        // Arrange
        var customEmail = "custom-admin@example.com";
        var settings = new AdminSeedSettings
        {
            Enabled = true,
            SkipIfAdminExists = false,
            Email = customEmail,
            Password = "TestPassword123!",
        };

        this.userRepositoryMock
            .Setup(r => r.EmailExistsAsync(customEmail, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        this.passwordHasherMock
            .Setup(p => p.Hash(It.IsAny<string>()))
            .Returns("hashed");

        User? capturedUser = null;
        this.userRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((user, _) => capturedUser = user)
            .Returns(Task.CompletedTask);

        var service = this.CreateService(settings);

        // Act
        await service.SeedAdminUserAsync(CancellationToken.None);

        // Assert
        capturedUser.Should().NotBeNull();
        capturedUser!.Email.Should().Be(customEmail);
    }

    /// <summary>
    /// Tests that SeedAdminUserAsync hashes configured password.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task SeedAdminUserAsync_HashesConfiguredPassword()
    {
        // Arrange
        var customPassword = "SuperSecretP@ssw0rd!";
        var settings = new AdminSeedSettings
        {
            Enabled = true,
            SkipIfAdminExists = false,
            Email = "admin@test.local",
            Password = customPassword,
        };

        this.userRepositoryMock
            .Setup(r => r.EmailExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        this.passwordHasherMock
            .Setup(p => p.Hash(customPassword))
            .Returns("hashed_custom_password");

        var service = this.CreateService(settings);

        // Act
        await service.SeedAdminUserAsync(CancellationToken.None);

        // Assert
        this.passwordHasherMock.Verify(
            p => p.Hash(customPassword),
            Times.Once);
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

    private AdminSeederService CreateService(AdminSeedSettings settings)
    {
        var scopeFactory = this.serviceProvider.GetRequiredService<IServiceScopeFactory>();
        var options = Options.Create(settings);
        return new AdminSeederService(scopeFactory, options, this.loggerMock.Object);
    }
}
