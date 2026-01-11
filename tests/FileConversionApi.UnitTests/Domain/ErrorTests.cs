// <copyright file="ErrorTests.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using System;

using FileConversionApi.Domain.Errors;
using FileConversionApi.Domain.Primitives;

using FluentAssertions;

using Xunit;

namespace FileConversionApi.UnitTests.Domain;

/// <summary>
/// Unit tests for the <see cref="Error"/> record and domain error definitions.
/// </summary>
public class ErrorTests
{
    /// <summary>
    /// Tests that Error.None has empty code.
    /// </summary>
    [Fact]
    public void ErrorNone_ShouldHaveEmptyCode()
    {
        // Assert
        Error.None.Code.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that Error.None has empty message.
    /// </summary>
    [Fact]
    public void ErrorNone_ShouldHaveEmptyMessage()
    {
        // Assert
        Error.None.Message.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that Error.NullValue has correct code.
    /// </summary>
    [Fact]
    public void ErrorNullValue_ShouldHaveCorrectCode()
    {
        // Assert
        Error.NullValue.Code.Should().Be("Error.NullValue");
    }

    /// <summary>
    /// Tests that Error.NullValue has correct message.
    /// </summary>
    [Fact]
    public void ErrorNullValue_ShouldHaveCorrectMessage()
    {
        // Assert
        Error.NullValue.Message.Should().Be("A null value was provided.");
    }

    /// <summary>
    /// Tests that Error can be created with custom code and message.
    /// </summary>
    [Fact]
    public void Error_ShouldBeCreatedWithCustomCodeAndMessage()
    {
        // Arrange
        var code = "Custom.Error";
        var message = "Custom error message";

        // Act
        var error = new Error(code, message);

        // Assert
        error.Code.Should().Be(code);
        error.Message.Should().Be(message);
    }

    /// <summary>
    /// Tests that Error equality works correctly for equal values.
    /// </summary>
    [Fact]
    public void Error_Equality_ShouldReturnTrueForEqualValues()
    {
        // Arrange
        var error1 = new Error("Test.Error", "Test message");
        var error2 = new Error("Test.Error", "Test message");

        // Assert
        error1.Should().Be(error2);
        (error1 == error2).Should().BeTrue();
    }

    /// <summary>
    /// Tests that Error equality works correctly for different values.
    /// </summary>
    [Fact]
    public void Error_Equality_ShouldReturnFalseForDifferentValues()
    {
        // Arrange
        var error1 = new Error("Test.Error1", "Test message");
        var error2 = new Error("Test.Error2", "Test message");

        // Assert
        error1.Should().NotBe(error2);
        (error1 != error2).Should().BeTrue();
    }

    /// <summary>
    /// Tests that implicit conversion from Error to Result creates failure result.
    /// </summary>
    [Fact]
    public void Error_ImplicitConversionToResult_ShouldCreateFailureResult()
    {
        // Arrange
        var error = new Error("Test.Error", "Test message");

        // Act
        Result result = error;

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    /// <summary>
    /// Tests UserErrors.EmailAlreadyExists has correct code.
    /// </summary>
    [Fact]
    public void UserErrors_EmailAlreadyExists_ShouldHaveCorrectCode()
    {
        // Assert
        UserErrors.EmailAlreadyExists.Code.Should().Be("User.EmailAlreadyExists");
    }

    /// <summary>
    /// Tests UserErrors.EmailAlreadyExists has correct message.
    /// </summary>
    [Fact]
    public void UserErrors_EmailAlreadyExists_ShouldHaveCorrectMessage()
    {
        // Assert
        UserErrors.EmailAlreadyExists.Message.Should().Be("A user with this email address already exists.");
    }

    /// <summary>
    /// Tests UserErrors.InvalidCredentials has correct code.
    /// </summary>
    [Fact]
    public void UserErrors_InvalidCredentials_ShouldHaveCorrectCode()
    {
        // Assert
        UserErrors.InvalidCredentials.Code.Should().Be("User.InvalidCredentials");
    }

    /// <summary>
    /// Tests UserErrors.InvalidCredentials has correct message.
    /// </summary>
    [Fact]
    public void UserErrors_InvalidCredentials_ShouldHaveCorrectMessage()
    {
        // Assert
        UserErrors.InvalidCredentials.Message.Should().Be("The provided credentials are invalid.");
    }

    /// <summary>
    /// Tests UserErrors.Inactive has correct code.
    /// </summary>
    [Fact]
    public void UserErrors_Inactive_ShouldHaveCorrectCode()
    {
        // Assert
        UserErrors.Inactive.Code.Should().Be("User.Inactive");
    }

    /// <summary>
    /// Tests UserErrors.Inactive has correct message.
    /// </summary>
    [Fact]
    public void UserErrors_Inactive_ShouldHaveCorrectMessage()
    {
        // Assert
        UserErrors.Inactive.Message.Should().Be("The user account is inactive.");
    }

    /// <summary>
    /// Tests UserErrors.InvalidApiKey has correct code.
    /// </summary>
    [Fact]
    public void UserErrors_InvalidApiKey_ShouldHaveCorrectCode()
    {
        // Assert
        UserErrors.InvalidApiKey.Code.Should().Be("User.InvalidApiKey");
    }

    /// <summary>
    /// Tests UserErrors.InvalidApiKey has correct message.
    /// </summary>
    [Fact]
    public void UserErrors_InvalidApiKey_ShouldHaveCorrectMessage()
    {
        // Assert
        UserErrors.InvalidApiKey.Message.Should().Be("The provided API key is invalid.");
    }

    /// <summary>
    /// Tests UserErrors.NotFound returns correct error with ID.
    /// </summary>
    [Fact]
    public void UserErrors_NotFound_ShouldReturnCorrectErrorWithId()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var error = UserErrors.NotFound(id);

        // Assert
        error.Code.Should().Be("User.NotFound");
        error.Message.Should().Be($"User with ID '{id}' was not found.");
    }

    /// <summary>
    /// Tests UserErrors.NotFoundByEmail returns correct error with email.
    /// </summary>
    [Fact]
    public void UserErrors_NotFoundByEmail_ShouldReturnCorrectErrorWithEmail()
    {
        // Arrange
        var email = "test@example.com";

        // Act
        var error = UserErrors.NotFoundByEmail(email);

        // Assert
        error.Code.Should().Be("User.NotFoundByEmail");
        error.Message.Should().Be($"User with email '{email}' was not found.");
    }

    /// <summary>
    /// Tests ConversionJobErrors.NotCompleted has correct code.
    /// </summary>
    [Fact]
    public void ConversionJobErrors_NotCompleted_ShouldHaveCorrectCode()
    {
        // Assert
        ConversionJobErrors.NotCompleted.Code.Should().Be("ConversionJob.NotCompleted");
    }

    /// <summary>
    /// Tests ConversionJobErrors.NotCompleted has correct message.
    /// </summary>
    [Fact]
    public void ConversionJobErrors_NotCompleted_ShouldHaveCorrectMessage()
    {
        // Assert
        ConversionJobErrors.NotCompleted.Message.Should().Be("The conversion job has not completed yet.");
    }

    /// <summary>
    /// Tests ConversionJobErrors.AlreadyProcessing has correct code.
    /// </summary>
    [Fact]
    public void ConversionJobErrors_AlreadyProcessing_ShouldHaveCorrectCode()
    {
        // Assert
        ConversionJobErrors.AlreadyProcessing.Code.Should().Be("ConversionJob.AlreadyProcessing");
    }

    /// <summary>
    /// Tests ConversionJobErrors.AlreadyProcessing has correct message.
    /// </summary>
    [Fact]
    public void ConversionJobErrors_AlreadyProcessing_ShouldHaveCorrectMessage()
    {
        // Assert
        ConversionJobErrors.AlreadyProcessing.Message.Should().Be("The conversion job is already being processed.");
    }

    /// <summary>
    /// Tests ConversionJobErrors.NoOutputAvailable has correct code.
    /// </summary>
    [Fact]
    public void ConversionJobErrors_NoOutputAvailable_ShouldHaveCorrectCode()
    {
        // Assert
        ConversionJobErrors.NoOutputAvailable.Code.Should().Be("ConversionJob.NoOutputAvailable");
    }

    /// <summary>
    /// Tests ConversionJobErrors.NoOutputAvailable has correct message.
    /// </summary>
    [Fact]
    public void ConversionJobErrors_NoOutputAvailable_ShouldHaveCorrectMessage()
    {
        // Assert
        ConversionJobErrors.NoOutputAvailable.Message.Should().Be("No output is available for this conversion job.");
    }

    /// <summary>
    /// Tests ConversionJobErrors.NotFound returns correct error with ID.
    /// </summary>
    [Fact]
    public void ConversionJobErrors_NotFound_ShouldReturnCorrectErrorWithId()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var error = ConversionJobErrors.NotFound(id);

        // Assert
        error.Code.Should().Be("ConversionJob.NotFound");
        error.Message.Should().Be($"Conversion job with ID '{id}' was not found.");
    }

    /// <summary>
    /// Tests ConversionJobErrors.ConversionFailed returns correct error with message.
    /// </summary>
    [Fact]
    public void ConversionJobErrors_ConversionFailed_ShouldReturnCorrectErrorWithMessage()
    {
        // Arrange
        var message = "File corrupted";

        // Act
        var error = ConversionJobErrors.ConversionFailed(message);

        // Assert
        error.Code.Should().Be("ConversionJob.ConversionFailed");
        error.Message.Should().Be($"Conversion failed: {message}");
    }

    /// <summary>
    /// Tests ConversionJobErrors.UnsupportedConversion returns correct error with formats.
    /// </summary>
    [Fact]
    public void ConversionJobErrors_UnsupportedConversion_ShouldReturnCorrectErrorWithFormats()
    {
        // Arrange
        var sourceFormat = "xyz";
        var targetFormat = "abc";

        // Act
        var error = ConversionJobErrors.UnsupportedConversion(sourceFormat, targetFormat);

        // Assert
        error.Code.Should().Be("ConversionJob.UnsupportedConversion");
        error.Message.Should().Be($"Conversion from '{sourceFormat}' to '{targetFormat}' is not supported.");
    }

    /// <summary>
    /// Tests that Error GetHashCode returns same value for equal instances.
    /// </summary>
    [Fact]
    public void Error_GetHashCode_ShouldReturnSameValueForEqualInstances()
    {
        // Arrange
        var error1 = new Error("Test.Error", "Test message");
        var error2 = new Error("Test.Error", "Test message");

        // Assert
        error1.GetHashCode().Should().Be(error2.GetHashCode());
    }

    /// <summary>
    /// Tests that Error can be used as a dictionary key.
    /// </summary>
    [Fact]
    public void Error_CanBeUsedAsDictionaryKey()
    {
        // Arrange
        var error = new Error("Test.Error", "Test message");
        var dictionary = new System.Collections.Generic.Dictionary<Error, int>();

        // Act
        dictionary[error] = 42;

        // Assert
        dictionary[error].Should().Be(42);
    }
}
