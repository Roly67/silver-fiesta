// <copyright file="UserTests.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using System;

using FileConversionApi.Domain.Entities;
using FileConversionApi.Domain.ValueObjects;

using FluentAssertions;

using Xunit;

namespace FileConversionApi.UnitTests.Domain;

/// <summary>
/// Unit tests for the <see cref="User"/> entity.
/// </summary>
public class UserTests
{
    private const string ValidEmail = "test@example.com";
    private const string ValidPasswordHash = "hashedpassword123";

    /// <summary>
    /// Tests that Create returns a user with a valid ID.
    /// </summary>
    [Fact]
    public void Create_ShouldReturnUserWithValidId()
    {
        // Act
        var user = User.Create(ValidEmail, ValidPasswordHash);

        // Assert
        user.Id.Value.Should().NotBeEmpty();
    }

    /// <summary>
    /// Tests that Create returns a user with the specified email.
    /// </summary>
    [Fact]
    public void Create_ShouldReturnUserWithSpecifiedEmail()
    {
        // Act
        var user = User.Create(ValidEmail, ValidPasswordHash);

        // Assert
        user.Email.Should().Be(ValidEmail);
    }

    /// <summary>
    /// Tests that Create returns a user with the specified password hash.
    /// </summary>
    [Fact]
    public void Create_ShouldReturnUserWithSpecifiedPasswordHash()
    {
        // Act
        var user = User.Create(ValidEmail, ValidPasswordHash);

        // Assert
        user.PasswordHash.Should().Be(ValidPasswordHash);
    }

    /// <summary>
    /// Tests that Create returns a user with a generated API key.
    /// </summary>
    [Fact]
    public void Create_ShouldReturnUserWithGeneratedApiKey()
    {
        // Act
        var user = User.Create(ValidEmail, ValidPasswordHash);

        // Assert
        user.ApiKey.Should().NotBeNullOrEmpty();
        user.ApiKey.Should().StartWith("fca_");
    }

    /// <summary>
    /// Tests that Create returns a user with IsActive set to true.
    /// </summary>
    [Fact]
    public void Create_ShouldReturnUserWithIsActiveTrue()
    {
        // Act
        var user = User.Create(ValidEmail, ValidPasswordHash);

        // Assert
        user.IsActive.Should().BeTrue();
    }

    /// <summary>
    /// Tests that Create sets CreatedAt to current UTC time.
    /// </summary>
    [Fact]
    public void Create_ShouldSetCreatedAtToCurrentUtcTime()
    {
        // Arrange
        var before = DateTimeOffset.UtcNow;

        // Act
        var user = User.Create(ValidEmail, ValidPasswordHash);

        // Assert
        var after = DateTimeOffset.UtcNow;
        user.CreatedAt.Should().BeOnOrAfter(before);
        user.CreatedAt.Should().BeOnOrBefore(after);
    }

    /// <summary>
    /// Tests that Create returns a user with empty ConversionJobs collection.
    /// </summary>
    [Fact]
    public void Create_ShouldReturnUserWithEmptyConversionJobs()
    {
        // Act
        var user = User.Create(ValidEmail, ValidPasswordHash);

        // Assert
        user.ConversionJobs.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that UpdatePassword changes the password hash.
    /// </summary>
    [Fact]
    public void UpdatePassword_ShouldChangePasswordHash()
    {
        // Arrange
        var user = User.Create(ValidEmail, ValidPasswordHash);
        var newPasswordHash = "newhash456";

        // Act
        user.UpdatePassword(newPasswordHash);

        // Assert
        user.PasswordHash.Should().Be(newPasswordHash);
    }

    /// <summary>
    /// Tests that RegenerateApiKey returns a new API key.
    /// </summary>
    [Fact]
    public void RegenerateApiKey_ShouldReturnNewApiKey()
    {
        // Arrange
        var user = User.Create(ValidEmail, ValidPasswordHash);
        var originalApiKey = user.ApiKey;

        // Act
        var newApiKey = user.RegenerateApiKey();

        // Assert
        newApiKey.Should().NotBeNullOrEmpty();
        newApiKey.Should().StartWith("fca_");
        newApiKey.Should().NotBe(originalApiKey);
    }

    /// <summary>
    /// Tests that RegenerateApiKey updates the user's API key property.
    /// </summary>
    [Fact]
    public void RegenerateApiKey_ShouldUpdateApiKeyProperty()
    {
        // Arrange
        var user = User.Create(ValidEmail, ValidPasswordHash);
        var originalApiKey = user.ApiKey;

        // Act
        var newApiKey = user.RegenerateApiKey();

        // Assert
        user.ApiKey.Should().Be(newApiKey);
        user.ApiKey.Should().NotBe(originalApiKey);
    }

    /// <summary>
    /// Tests that Deactivate sets IsActive to false.
    /// </summary>
    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var user = User.Create(ValidEmail, ValidPasswordHash);

        // Act
        user.Deactivate();

        // Assert
        user.IsActive.Should().BeFalse();
    }

    /// <summary>
    /// Tests that Activate sets IsActive to true.
    /// </summary>
    [Fact]
    public void Activate_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var user = User.Create(ValidEmail, ValidPasswordHash);
        user.Deactivate();

        // Act
        user.Activate();

        // Assert
        user.IsActive.Should().BeTrue();
    }

    /// <summary>
    /// Tests that multiple users have unique IDs.
    /// </summary>
    [Fact]
    public void Create_MultipleUsers_ShouldHaveUniqueIds()
    {
        // Act
        var user1 = User.Create("user1@example.com", ValidPasswordHash);
        var user2 = User.Create("user2@example.com", ValidPasswordHash);

        // Assert
        user1.Id.Should().NotBe(user2.Id);
    }

    /// <summary>
    /// Tests that multiple users have unique API keys.
    /// </summary>
    [Fact]
    public void Create_MultipleUsers_ShouldHaveUniqueApiKeys()
    {
        // Act
        var user1 = User.Create("user1@example.com", ValidPasswordHash);
        var user2 = User.Create("user2@example.com", ValidPasswordHash);

        // Assert
        user1.ApiKey.Should().NotBe(user2.ApiKey);
    }

    /// <summary>
    /// Tests that API key format is valid.
    /// </summary>
    [Fact]
    public void Create_ApiKey_ShouldHaveValidFormat()
    {
        // Act
        var user = User.Create(ValidEmail, ValidPasswordHash);

        // Assert
        user.ApiKey.Should().StartWith("fca_");
        user.ApiKey.Should().NotContain("=");
        user.ApiKey.Should().NotContain("+");
        user.ApiKey.Should().NotContain("/");
    }

    /// <summary>
    /// Tests that Deactivate can be called multiple times.
    /// </summary>
    [Fact]
    public void Deactivate_CalledMultipleTimes_ShouldRemainInactive()
    {
        // Arrange
        var user = User.Create(ValidEmail, ValidPasswordHash);

        // Act
        user.Deactivate();
        user.Deactivate();

        // Assert
        user.IsActive.Should().BeFalse();
    }

    /// <summary>
    /// Tests that Activate can be called on already active user.
    /// </summary>
    [Fact]
    public void Activate_CalledOnActiveUser_ShouldRemainActive()
    {
        // Arrange
        var user = User.Create(ValidEmail, ValidPasswordHash);

        // Act
        user.Activate();

        // Assert
        user.IsActive.Should().BeTrue();
    }
}
