// <copyright file="UserRateLimitSettingsTests.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using System;

using FileConversionApi.Domain.Entities;
using FileConversionApi.Domain.Enums;
using FileConversionApi.Domain.ValueObjects;

using FluentAssertions;

using Xunit;

namespace FileConversionApi.UnitTests.Domain;

/// <summary>
/// Unit tests for <see cref="UserRateLimitSettings"/>.
/// </summary>
public class UserRateLimitSettingsTests
{
    /// <summary>
    /// Tests that Create creates settings with default tier.
    /// </summary>
    [Fact]
    public void Create_CreatesSettingsWithDefaultTier()
    {
        // Arrange
        var userId = new UserId(Guid.NewGuid());

        // Act
        var settings = UserRateLimitSettings.Create(userId);

        // Assert
        settings.UserId.Should().Be(userId);
        settings.Tier.Should().Be(RateLimitTier.Free);
        settings.HasAnyOverride.Should().BeFalse();
    }

    /// <summary>
    /// Tests that UpdateTier changes the tier correctly.
    /// </summary>
    [Fact]
    public void UpdateTier_ChangesTier()
    {
        // Arrange
        var userId = new UserId(Guid.NewGuid());
        var settings = UserRateLimitSettings.Create(userId);

        // Act
        settings.UpdateTier(RateLimitTier.Premium);

        // Assert
        settings.Tier.Should().Be(RateLimitTier.Premium);
    }

    /// <summary>
    /// Tests that SetStandardPolicyOverride sets override values.
    /// </summary>
    [Fact]
    public void SetStandardPolicyOverride_SetsOverrideValues()
    {
        // Arrange
        var userId = new UserId(Guid.NewGuid());
        var settings = UserRateLimitSettings.Create(userId);

        // Act
        settings.SetStandardPolicyOverride(500, 30);

        // Assert
        settings.StandardPolicyPermitLimit.Should().Be(500);
        settings.StandardPolicyWindowMinutes.Should().Be(30);
        settings.HasStandardPolicyOverride.Should().BeTrue();
        settings.HasAnyOverride.Should().BeTrue();
    }

    /// <summary>
    /// Tests that SetConversionPolicyOverride sets override values.
    /// </summary>
    [Fact]
    public void SetConversionPolicyOverride_SetsOverrideValues()
    {
        // Arrange
        var userId = new UserId(Guid.NewGuid());
        var settings = UserRateLimitSettings.Create(userId);

        // Act
        settings.SetConversionPolicyOverride(100, 15);

        // Assert
        settings.ConversionPolicyPermitLimit.Should().Be(100);
        settings.ConversionPolicyWindowMinutes.Should().Be(15);
        settings.HasConversionPolicyOverride.Should().BeTrue();
        settings.HasAnyOverride.Should().BeTrue();
    }

    /// <summary>
    /// Tests that ClearAllOverrides removes all overrides.
    /// </summary>
    [Fact]
    public void ClearAllOverrides_RemovesAllOverrides()
    {
        // Arrange
        var userId = new UserId(Guid.NewGuid());
        var settings = UserRateLimitSettings.Create(userId);
        settings.SetStandardPolicyOverride(500, 30);
        settings.SetConversionPolicyOverride(100, 15);

        // Act
        settings.ClearAllOverrides();

        // Assert
        settings.StandardPolicyPermitLimit.Should().BeNull();
        settings.StandardPolicyWindowMinutes.Should().BeNull();
        settings.ConversionPolicyPermitLimit.Should().BeNull();
        settings.ConversionPolicyWindowMinutes.Should().BeNull();
        settings.HasAnyOverride.Should().BeFalse();
    }

    /// <summary>
    /// Tests that HasStandardPolicyOverride requires both values set.
    /// </summary>
    [Fact]
    public void HasStandardPolicyOverride_RequiresBothValues()
    {
        // Arrange
        var userId = new UserId(Guid.NewGuid());
        var settings = UserRateLimitSettings.Create(userId);

        // Act & Assert
        settings.HasStandardPolicyOverride.Should().BeFalse();

        settings.SetStandardPolicyOverride(500, 30);
        settings.HasStandardPolicyOverride.Should().BeTrue();
    }

    /// <summary>
    /// Tests that HasConversionPolicyOverride requires both values set.
    /// </summary>
    [Fact]
    public void HasConversionPolicyOverride_RequiresBothValues()
    {
        // Arrange
        var userId = new UserId(Guid.NewGuid());
        var settings = UserRateLimitSettings.Create(userId);

        // Act & Assert
        settings.HasConversionPolicyOverride.Should().BeFalse();

        settings.SetConversionPolicyOverride(100, 15);
        settings.HasConversionPolicyOverride.Should().BeTrue();
    }
}
