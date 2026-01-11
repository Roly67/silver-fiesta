// <copyright file="ValueObjectTests.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using System;

using FileConversionApi.Domain.ValueObjects;

using FluentAssertions;

using Xunit;

namespace FileConversionApi.UnitTests.Domain;

/// <summary>
/// Unit tests for value objects in the Domain layer.
/// </summary>
public class ValueObjectTests
{
    /// <summary>
    /// Tests that UserId.New creates a user ID with a non-empty GUID.
    /// </summary>
    [Fact]
    public void UserId_New_ShouldCreateWithNonEmptyGuid()
    {
        // Act
        var userId = UserId.New();

        // Assert
        userId.Value.Should().NotBeEmpty();
    }

    /// <summary>
    /// Tests that UserId.New creates unique IDs.
    /// </summary>
    [Fact]
    public void UserId_New_ShouldCreateUniqueIds()
    {
        // Act
        var userId1 = UserId.New();
        var userId2 = UserId.New();

        // Assert
        userId1.Should().NotBe(userId2);
    }

    /// <summary>
    /// Tests that UserId.From creates a user ID with the specified GUID.
    /// </summary>
    [Fact]
    public void UserId_From_ShouldCreateWithSpecifiedGuid()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var userId = UserId.From(guid);

        // Assert
        userId.Value.Should().Be(guid);
    }

    /// <summary>
    /// Tests that UserId.ToString returns the GUID as a string.
    /// </summary>
    [Fact]
    public void UserId_ToString_ShouldReturnGuidAsString()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var userId = UserId.From(guid);

        // Act
        var result = userId.ToString();

        // Assert
        result.Should().Be(guid.ToString());
    }

    /// <summary>
    /// Tests that UserId equality works correctly for equal values.
    /// </summary>
    [Fact]
    public void UserId_Equality_ShouldReturnTrueForEqualValues()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var userId1 = UserId.From(guid);
        var userId2 = UserId.From(guid);

        // Assert
        userId1.Should().Be(userId2);
        (userId1 == userId2).Should().BeTrue();
    }

    /// <summary>
    /// Tests that UserId equality works correctly for different values.
    /// </summary>
    [Fact]
    public void UserId_Equality_ShouldReturnFalseForDifferentValues()
    {
        // Arrange
        var userId1 = UserId.New();
        var userId2 = UserId.New();

        // Assert
        userId1.Should().NotBe(userId2);
        (userId1 != userId2).Should().BeTrue();
    }

    /// <summary>
    /// Tests that UserId GetHashCode returns same value for equal instances.
    /// </summary>
    [Fact]
    public void UserId_GetHashCode_ShouldReturnSameValueForEqualInstances()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var userId1 = UserId.From(guid);
        var userId2 = UserId.From(guid);

        // Assert
        userId1.GetHashCode().Should().Be(userId2.GetHashCode());
    }

    /// <summary>
    /// Tests that UserId can be used as a dictionary key.
    /// </summary>
    [Fact]
    public void UserId_CanBeUsedAsDictionaryKey()
    {
        // Arrange
        var userId = UserId.New();
        var dictionary = new System.Collections.Generic.Dictionary<UserId, string>();

        // Act
        dictionary[userId] = "test";

        // Assert
        dictionary[userId].Should().Be("test");
    }

    /// <summary>
    /// Tests that ConversionJobId.New creates a job ID with a non-empty GUID.
    /// </summary>
    [Fact]
    public void ConversionJobId_New_ShouldCreateWithNonEmptyGuid()
    {
        // Act
        var jobId = ConversionJobId.New();

        // Assert
        jobId.Value.Should().NotBeEmpty();
    }

    /// <summary>
    /// Tests that ConversionJobId.New creates unique IDs.
    /// </summary>
    [Fact]
    public void ConversionJobId_New_ShouldCreateUniqueIds()
    {
        // Act
        var jobId1 = ConversionJobId.New();
        var jobId2 = ConversionJobId.New();

        // Assert
        jobId1.Should().NotBe(jobId2);
    }

    /// <summary>
    /// Tests that ConversionJobId.From creates a job ID with the specified GUID.
    /// </summary>
    [Fact]
    public void ConversionJobId_From_ShouldCreateWithSpecifiedGuid()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var jobId = ConversionJobId.From(guid);

        // Assert
        jobId.Value.Should().Be(guid);
    }

    /// <summary>
    /// Tests that ConversionJobId.ToString returns the GUID as a string.
    /// </summary>
    [Fact]
    public void ConversionJobId_ToString_ShouldReturnGuidAsString()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var jobId = ConversionJobId.From(guid);

        // Act
        var result = jobId.ToString();

        // Assert
        result.Should().Be(guid.ToString());
    }

    /// <summary>
    /// Tests that ConversionJobId equality works correctly for equal values.
    /// </summary>
    [Fact]
    public void ConversionJobId_Equality_ShouldReturnTrueForEqualValues()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var jobId1 = ConversionJobId.From(guid);
        var jobId2 = ConversionJobId.From(guid);

        // Assert
        jobId1.Should().Be(jobId2);
        (jobId1 == jobId2).Should().BeTrue();
    }

    /// <summary>
    /// Tests that ConversionJobId equality works correctly for different values.
    /// </summary>
    [Fact]
    public void ConversionJobId_Equality_ShouldReturnFalseForDifferentValues()
    {
        // Arrange
        var jobId1 = ConversionJobId.New();
        var jobId2 = ConversionJobId.New();

        // Assert
        jobId1.Should().NotBe(jobId2);
        (jobId1 != jobId2).Should().BeTrue();
    }

    /// <summary>
    /// Tests that ConversionJobId GetHashCode returns same value for equal instances.
    /// </summary>
    [Fact]
    public void ConversionJobId_GetHashCode_ShouldReturnSameValueForEqualInstances()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var jobId1 = ConversionJobId.From(guid);
        var jobId2 = ConversionJobId.From(guid);

        // Assert
        jobId1.GetHashCode().Should().Be(jobId2.GetHashCode());
    }

    /// <summary>
    /// Tests that ConversionJobId can be used as a dictionary key.
    /// </summary>
    [Fact]
    public void ConversionJobId_CanBeUsedAsDictionaryKey()
    {
        // Arrange
        var jobId = ConversionJobId.New();
        var dictionary = new System.Collections.Generic.Dictionary<ConversionJobId, string>();

        // Act
        dictionary[jobId] = "test";

        // Assert
        dictionary[jobId].Should().Be("test");
    }

    /// <summary>
    /// Tests that UserId default value has empty GUID.
    /// </summary>
    [Fact]
    public void UserId_Default_ShouldHaveEmptyGuid()
    {
        // Act
        var userId = default(UserId);

        // Assert
        userId.Value.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that ConversionJobId default value has empty GUID.
    /// </summary>
    [Fact]
    public void ConversionJobId_Default_ShouldHaveEmptyGuid()
    {
        // Act
        var jobId = default(ConversionJobId);

        // Assert
        jobId.Value.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that UserId with empty GUID can be created.
    /// </summary>
    [Fact]
    public void UserId_FromEmptyGuid_ShouldBeCreated()
    {
        // Act
        var userId = UserId.From(Guid.Empty);

        // Assert
        userId.Value.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that ConversionJobId with empty GUID can be created.
    /// </summary>
    [Fact]
    public void ConversionJobId_FromEmptyGuid_ShouldBeCreated()
    {
        // Act
        var jobId = ConversionJobId.From(Guid.Empty);

        // Assert
        jobId.Value.Should().BeEmpty();
    }
}
