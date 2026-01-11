// <copyright file="ResultTests.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using System;

using FileConversionApi.Domain.Primitives;

using FluentAssertions;

using Xunit;

namespace FileConversionApi.UnitTests.Domain;

/// <summary>
/// Unit tests for the <see cref="Result"/> class.
/// </summary>
public class ResultTests
{
    /// <summary>
    /// Tests that Success creates a result with IsSuccess true.
    /// </summary>
    [Fact]
    public void Success_ShouldCreateResultWithIsSuccessTrue()
    {
        // Act
        var result = Result.Success();

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    /// <summary>
    /// Tests that Success creates a result with IsFailure false.
    /// </summary>
    [Fact]
    public void Success_ShouldCreateResultWithIsFailureFalse()
    {
        // Act
        var result = Result.Success();

        // Assert
        result.IsFailure.Should().BeFalse();
    }

    /// <summary>
    /// Tests that Success creates a result with Error.None.
    /// </summary>
    [Fact]
    public void Success_ShouldCreateResultWithErrorNone()
    {
        // Act
        var result = Result.Success();

        // Assert
        result.Error.Should().Be(Error.None);
    }

    /// <summary>
    /// Tests that Failure creates a result with IsSuccess false.
    /// </summary>
    [Fact]
    public void Failure_ShouldCreateResultWithIsSuccessFalse()
    {
        // Arrange
        var error = new Error("Test.Error", "Test error message");

        // Act
        var result = Result.Failure(error);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    /// <summary>
    /// Tests that Failure creates a result with IsFailure true.
    /// </summary>
    [Fact]
    public void Failure_ShouldCreateResultWithIsFailureTrue()
    {
        // Arrange
        var error = new Error("Test.Error", "Test error message");

        // Act
        var result = Result.Failure(error);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    /// <summary>
    /// Tests that Failure creates a result with the specified error.
    /// </summary>
    [Fact]
    public void Failure_ShouldCreateResultWithSpecifiedError()
    {
        // Arrange
        var error = new Error("Test.Error", "Test error message");

        // Act
        var result = Result.Failure(error);

        // Assert
        result.Error.Should().Be(error);
    }

    /// <summary>
    /// Tests that generic Success creates a result with the value.
    /// </summary>
    [Fact]
    public void SuccessGeneric_ShouldCreateResultWithValue()
    {
        // Arrange
        var value = "test value";

        // Act
        var result = Result.Success(value);

        // Assert
        result.Value.Should().Be(value);
    }

    /// <summary>
    /// Tests that generic Success creates a result with IsSuccess true.
    /// </summary>
    [Fact]
    public void SuccessGeneric_ShouldCreateResultWithIsSuccessTrue()
    {
        // Arrange
        var value = 42;

        // Act
        var result = Result.Success(value);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    /// <summary>
    /// Tests that generic Failure creates a result with IsSuccess false.
    /// </summary>
    [Fact]
    public void FailureGeneric_ShouldCreateResultWithIsSuccessFalse()
    {
        // Arrange
        var error = new Error("Test.Error", "Test error message");

        // Act
        var result = Result.Failure<string>(error);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    /// <summary>
    /// Tests that generic Failure creates a result with the specified error.
    /// </summary>
    [Fact]
    public void FailureGeneric_ShouldCreateResultWithSpecifiedError()
    {
        // Arrange
        var error = new Error("Test.Error", "Test error message");

        // Act
        var result = Result.Failure<int>(error);

        // Assert
        result.Error.Should().Be(error);
    }

    /// <summary>
    /// Tests that implicit conversion from Error to Result creates a failure result.
    /// </summary>
    [Fact]
    public void ImplicitConversionFromError_ShouldCreateFailureResult()
    {
        // Arrange
        var error = new Error("Test.Error", "Test error message");

        // Act
        Result result = error;

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    /// <summary>
    /// Tests that IsFailure is the opposite of IsSuccess.
    /// </summary>
    [Fact]
    public void IsFailure_ShouldBeOppositeOfIsSuccess()
    {
        // Arrange
        var successResult = Result.Success();
        var failureResult = Result.Failure(new Error("Test.Error", "Test"));

        // Assert
        successResult.IsFailure.Should().Be(!successResult.IsSuccess);
        failureResult.IsFailure.Should().Be(!failureResult.IsSuccess);
    }
}
