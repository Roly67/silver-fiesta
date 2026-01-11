// <copyright file="ResultGenericTests.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using System;

using FileConversionApi.Domain.Primitives;

using FluentAssertions;

using Xunit;

namespace FileConversionApi.UnitTests.Domain;

/// <summary>
/// Unit tests for the <see cref="Result{TValue}"/> class.
/// </summary>
public class ResultGenericTests
{
    /// <summary>
    /// Tests that Value returns the value when result is success.
    /// </summary>
    [Fact]
    public void Value_WhenSuccess_ShouldReturnValue()
    {
        // Arrange
        var expectedValue = "test value";
        var result = Result.Success(expectedValue);

        // Act
        var actualValue = result.Value;

        // Assert
        actualValue.Should().Be(expectedValue);
    }

    /// <summary>
    /// Tests that Value throws InvalidOperationException when result is failure.
    /// </summary>
    [Fact]
    public void Value_WhenFailure_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var error = new Error("Test.Error", "Test error message");
        var result = Result.Failure<string>(error);

        // Act
        Action act = () => _ = result.Value;

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot access value of a failed result.");
    }

    /// <summary>
    /// Tests that IsSuccess is true when result is success.
    /// </summary>
    [Fact]
    public void IsSuccess_WhenSuccess_ShouldBeTrue()
    {
        // Arrange
        var result = Result.Success(42);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    /// <summary>
    /// Tests that IsSuccess is false when result is failure.
    /// </summary>
    [Fact]
    public void IsSuccess_WhenFailure_ShouldBeFalse()
    {
        // Arrange
        var error = new Error("Test.Error", "Test error message");
        var result = Result.Failure<int>(error);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    /// <summary>
    /// Tests that IsFailure is false when result is success.
    /// </summary>
    [Fact]
    public void IsFailure_WhenSuccess_ShouldBeFalse()
    {
        // Arrange
        var result = Result.Success("test");

        // Assert
        result.IsFailure.Should().BeFalse();
    }

    /// <summary>
    /// Tests that IsFailure is true when result is failure.
    /// </summary>
    [Fact]
    public void IsFailure_WhenFailure_ShouldBeTrue()
    {
        // Arrange
        var error = new Error("Test.Error", "Test error message");
        var result = Result.Failure<string>(error);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    /// <summary>
    /// Tests that Error is Error.None when result is success.
    /// </summary>
    [Fact]
    public void Error_WhenSuccess_ShouldBeErrorNone()
    {
        // Arrange
        var result = Result.Success(100);

        // Assert
        result.Error.Should().Be(Error.None);
    }

    /// <summary>
    /// Tests that Error returns the error when result is failure.
    /// </summary>
    [Fact]
    public void Error_WhenFailure_ShouldReturnError()
    {
        // Arrange
        var error = new Error("Test.Error", "Test error message");
        var result = Result.Failure<double>(error);

        // Assert
        result.Error.Should().Be(error);
    }

    /// <summary>
    /// Tests implicit conversion from value to success result.
    /// </summary>
    [Fact]
    public void ImplicitConversionFromValue_ShouldCreateSuccessResult()
    {
        // Arrange
        string value = "test value";

        // Act
        Result<string> result = value;

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(value);
    }

    /// <summary>
    /// Tests implicit conversion from error to failure result.
    /// </summary>
    [Fact]
    public void ImplicitConversionFromError_ShouldCreateFailureResult()
    {
        // Arrange
        var error = new Error("Test.Error", "Test error message");

        // Act
        Result<string> result = error;

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    /// <summary>
    /// Tests that implicit conversion from int value creates success result.
    /// </summary>
    [Fact]
    public void ImplicitConversionFromInt_ShouldCreateSuccessResult()
    {
        // Arrange
        int value = 42;

        // Act
        Result<int> result = value;

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    /// <summary>
    /// Tests that Result inherits from base Result class.
    /// </summary>
    [Fact]
    public void GenericResult_ShouldInheritFromResult()
    {
        // Arrange
        var result = Result.Success("test");

        // Assert
        result.Should().BeAssignableTo<Result>();
    }

    /// <summary>
    /// Tests that value type result works correctly.
    /// </summary>
    [Fact]
    public void ValueTypeResult_ShouldWorkCorrectly()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var result = Result.Success(guid);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(guid);
    }

    /// <summary>
    /// Tests that nullable reference type result works correctly.
    /// </summary>
    [Fact]
    public void NullableReferenceTypeResult_ShouldWorkCorrectly()
    {
        // Arrange
        string? value = null;

        // Act
        var result = Result.Success(value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    /// <summary>
    /// Tests that complex type result works correctly.
    /// </summary>
    [Fact]
    public void ComplexTypeResult_ShouldWorkCorrectly()
    {
        // Arrange
        var complexObject = new { Name = "Test", Value = 123 };

        // Act
        var result = Result.Success(complexObject);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Test");
        result.Value.Value.Should().Be(123);
    }
}
