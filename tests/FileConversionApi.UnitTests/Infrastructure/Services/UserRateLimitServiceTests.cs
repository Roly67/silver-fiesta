// <copyright file="UserRateLimitServiceTests.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using System;
using System.Threading;
using System.Threading.Tasks;

using FileConversionApi.Application.Interfaces;
using FileConversionApi.Domain.Entities;
using FileConversionApi.Domain.Enums;
using FileConversionApi.Domain.ValueObjects;
using FileConversionApi.Infrastructure.Options;
using FileConversionApi.Infrastructure.Services;

using FluentAssertions;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Moq;

using Xunit;

namespace FileConversionApi.UnitTests.Infrastructure.Services;

/// <summary>
/// Unit tests for <see cref="UserRateLimitService"/>.
/// </summary>
public class UserRateLimitServiceTests
{
    private readonly Mock<IUserRateLimitSettingsRepository> repositoryMock;
    private readonly Mock<IUserRepository> userRepositoryMock;
    private readonly Mock<IUnitOfWork> unitOfWorkMock;
    private readonly Mock<ILogger<UserRateLimitService>> loggerMock;
    private readonly IMemoryCache memoryCache;
    private readonly RateLimitingSettings settings;
    private readonly UserRateLimitService service;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserRateLimitServiceTests"/> class.
    /// </summary>
    public UserRateLimitServiceTests()
    {
        this.repositoryMock = new Mock<IUserRateLimitSettingsRepository>();
        this.userRepositoryMock = new Mock<IUserRepository>();
        this.unitOfWorkMock = new Mock<IUnitOfWork>();
        this.loggerMock = new Mock<ILogger<UserRateLimitService>>();
        this.memoryCache = new MemoryCache(new MemoryCacheOptions());
        this.settings = new RateLimitingSettings
        {
            EnableRateLimiting = true,
            ExemptAdmins = true,
            UserSettingsCacheSeconds = 300,
            Tiers = new()
            {
                ["Free"] = new RateLimitTierSettings
                {
                    StandardPolicy = new RateLimitPolicySettings { PermitLimit = 100, WindowMinutes = 60 },
                    ConversionPolicy = new RateLimitPolicySettings { PermitLimit = 20, WindowMinutes = 60 },
                },
                ["Premium"] = new RateLimitTierSettings
                {
                    StandardPolicy = new RateLimitPolicySettings { PermitLimit = 2000, WindowMinutes = 60 },
                    ConversionPolicy = new RateLimitPolicySettings { PermitLimit = 500, WindowMinutes = 60 },
                },
            },
        };

        var optionsMock = new Mock<IOptions<RateLimitingSettings>>();
        optionsMock.Setup(x => x.Value).Returns(this.settings);

        this.service = new UserRateLimitService(
            this.repositoryMock.Object,
            this.userRepositoryMock.Object,
            this.unitOfWorkMock.Object,
            this.memoryCache,
            optionsMock.Object,
            this.loggerMock.Object);
    }

    /// <summary>
    /// Tests that GetEffectiveLimitsAsync returns tier defaults when no overrides.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetEffectiveLimitsAsync_ReturnsDefaultTierLimits_WhenNoOverrides()
    {
        // Arrange
        var userId = new UserId(Guid.NewGuid());
        var userSettings = UserRateLimitSettings.Create(userId);

        this.repositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userSettings);

        // Act
        var result = await this.service.GetEffectiveLimitsAsync(userId, "standard", CancellationToken.None);

        // Assert
        result.PermitLimit.Should().Be(100);
        result.Window.TotalMinutes.Should().Be(60);
    }

    /// <summary>
    /// Tests that GetEffectiveLimitsAsync returns override values when set.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetEffectiveLimitsAsync_ReturnsOverrideLimits_WhenOverrideSet()
    {
        // Arrange
        var userId = new UserId(Guid.NewGuid());
        var userSettings = UserRateLimitSettings.Create(userId);
        userSettings.SetStandardPolicyOverride(999, 30);

        this.repositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userSettings);

        // Act
        var result = await this.service.GetEffectiveLimitsAsync(userId, "standard", CancellationToken.None);

        // Assert
        result.PermitLimit.Should().Be(999);
        result.Window.TotalMinutes.Should().Be(30);
    }

    /// <summary>
    /// Tests that GetEffectiveLimitsAsync returns premium tier limits.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetEffectiveLimitsAsync_ReturnsPremiumTierLimits_WhenTierIsPremium()
    {
        // Arrange
        var userId = new UserId(Guid.NewGuid());
        var userSettings = UserRateLimitSettings.Create(userId);
        userSettings.UpdateTier(RateLimitTier.Premium);

        this.repositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userSettings);

        // Act
        var result = await this.service.GetEffectiveLimitsAsync(userId, "standard", CancellationToken.None);

        // Assert
        result.PermitLimit.Should().Be(2000);
        result.Window.TotalMinutes.Should().Be(60);
    }

    /// <summary>
    /// Tests that ShouldBypassRateLimitAsync returns true for admin users.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task ShouldBypassRateLimitAsync_ReturnsTrue_WhenUserIsAdmin()
    {
        // Arrange
        var user = User.Create("admin@test.com", "hash");
        user.GrantAdmin();

        this.userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await this.service.ShouldBypassRateLimitAsync(user.Id, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Tests that ShouldBypassRateLimitAsync returns false for non-admin users.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task ShouldBypassRateLimitAsync_ReturnsFalse_WhenUserIsNotAdmin()
    {
        // Arrange
        var user = User.Create("user@test.com", "hash");

        this.userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await this.service.ShouldBypassRateLimitAsync(user.Id, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Tests that GetOrCreateSettingsAsync creates settings for new user.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetOrCreateSettingsAsync_CreatesNewSettings_WhenNotExists()
    {
        // Arrange
        var userId = new UserId(Guid.NewGuid());

        this.repositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(null as UserRateLimitSettings);

        this.unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await this.service.GetOrCreateSettingsAsync(userId, CancellationToken.None);

        // Assert
        result.UserId.Should().Be(userId);
        result.Tier.Should().Be(RateLimitTier.Free);
        this.repositoryMock.Verify(x => x.AddAsync(It.IsAny<UserRateLimitSettings>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that UpdateTierAsync updates the tier correctly.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task UpdateTierAsync_UpdatesTier_Successfully()
    {
        // Arrange
        var userId = new UserId(Guid.NewGuid());
        var userSettings = UserRateLimitSettings.Create(userId);

        this.repositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userSettings);

        this.unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await this.service.UpdateTierAsync(userId, RateLimitTier.Premium, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        userSettings.Tier.Should().Be(RateLimitTier.Premium);
        this.repositoryMock.Verify(x => x.Update(userSettings), Times.Once);
    }

    /// <summary>
    /// Tests that SetPolicyOverrideAsync sets the override correctly.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task SetPolicyOverrideAsync_SetsOverride_ForStandardPolicy()
    {
        // Arrange
        var userId = new UserId(Guid.NewGuid());
        var userSettings = UserRateLimitSettings.Create(userId);

        this.repositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userSettings);

        this.unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await this.service.SetPolicyOverrideAsync(userId, "standard", 500, 30, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        userSettings.StandardPolicyPermitLimit.Should().Be(500);
        userSettings.StandardPolicyWindowMinutes.Should().Be(30);
    }

    /// <summary>
    /// Tests that SetPolicyOverrideAsync returns error for invalid policy name.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task SetPolicyOverrideAsync_ReturnsError_ForInvalidPolicyName()
    {
        // Arrange
        var userId = new UserId(Guid.NewGuid());
        var userSettings = UserRateLimitSettings.Create(userId);

        this.repositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userSettings);

        // Act
        var result = await this.service.SetPolicyOverrideAsync(userId, "invalid", 500, 30, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("RateLimit.InvalidPolicyName");
    }

    /// <summary>
    /// Tests that ClearOverridesAsync clears all overrides.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task ClearOverridesAsync_ClearsAllOverrides_Successfully()
    {
        // Arrange
        var userId = new UserId(Guid.NewGuid());
        var userSettings = UserRateLimitSettings.Create(userId);
        userSettings.SetStandardPolicyOverride(500, 30);
        userSettings.SetConversionPolicyOverride(100, 15);

        this.repositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userSettings);

        this.unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await this.service.ClearOverridesAsync(userId, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        userSettings.HasAnyOverride.Should().BeFalse();
    }
}
