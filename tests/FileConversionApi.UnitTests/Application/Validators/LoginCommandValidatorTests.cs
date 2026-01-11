// <copyright file="LoginCommandValidatorTests.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Application.Commands.Auth;

using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace FileConversionApi.UnitTests.Application.Validators;

/// <summary>
/// Unit tests for <see cref="LoginCommandValidator"/>.
/// </summary>
public class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoginCommandValidatorTests"/> class.
    /// </summary>
    public LoginCommandValidatorTests()
    {
        this.validator = new LoginCommandValidator();
    }

    /// <summary>
    /// Tests that validation succeeds when all fields are valid.
    /// </summary>
    [Fact]
    public void Validate_WhenAllFieldsAreValid_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var command = new LoginCommand("user@example.com", "password123");

        // Act
        var result = this.validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Tests that validation fails when email is empty.
    /// </summary>
    [Fact]
    public void Validate_WhenEmailIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        var command = new LoginCommand(string.Empty, "password123");

        // Act
        var result = this.validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email is required.");
    }

    /// <summary>
    /// Tests that validation fails when email is null.
    /// </summary>
    [Fact]
    public void Validate_WhenEmailIsNull_ShouldHaveValidationError()
    {
        // Arrange
        var command = new LoginCommand(null!, "password123");

        // Act
        var result = this.validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email is required.");
    }

    /// <summary>
    /// Tests that validation fails when email is whitespace only.
    /// </summary>
    [Fact]
    public void Validate_WhenEmailIsWhitespace_ShouldHaveValidationError()
    {
        // Arrange
        var command = new LoginCommand("   ", "password123");

        // Act
        var result = this.validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email is required.");
    }

    /// <summary>
    /// Tests that validation fails when email is invalid format.
    /// </summary>
    /// <param name="invalidEmail">The invalid email to test.</param>
    [Theory]
    [InlineData("notanemail")]
    [InlineData("@nodomain.com")]
    [InlineData("double@@at.com")]
    public void Validate_WhenEmailIsInvalidFormat_ShouldHaveValidationError(string invalidEmail)
    {
        // Arrange
        var command = new LoginCommand(invalidEmail, "password123");

        // Act
        var result = this.validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("A valid email address is required.");
    }

    /// <summary>
    /// Tests that validation succeeds with various valid email formats.
    /// </summary>
    /// <param name="validEmail">The valid email to test.</param>
    [Theory]
    [InlineData("simple@example.com")]
    [InlineData("user.name@example.com")]
    [InlineData("user+tag@example.com")]
    [InlineData("user@subdomain.example.com")]
    public void Validate_WhenEmailIsValidFormat_ShouldNotHaveValidationErrorForEmail(string validEmail)
    {
        // Arrange
        var command = new LoginCommand(validEmail, "password123");

        // Act
        var result = this.validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    /// <summary>
    /// Tests that validation fails when password is empty.
    /// </summary>
    [Fact]
    public void Validate_WhenPasswordIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        var command = new LoginCommand("user@example.com", string.Empty);

        // Act
        var result = this.validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password is required.");
    }

    /// <summary>
    /// Tests that validation fails when password is null.
    /// </summary>
    [Fact]
    public void Validate_WhenPasswordIsNull_ShouldHaveValidationError()
    {
        // Arrange
        var command = new LoginCommand("user@example.com", null!);

        // Act
        var result = this.validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password is required.");
    }

    /// <summary>
    /// Tests that validation fails when password is whitespace only.
    /// </summary>
    [Fact]
    public void Validate_WhenPasswordIsWhitespace_ShouldHaveValidationError()
    {
        // Arrange
        var command = new LoginCommand("user@example.com", "   ");

        // Act
        var result = this.validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password is required.");
    }

    /// <summary>
    /// Tests that validation fails with multiple errors when both email and password are invalid.
    /// </summary>
    [Fact]
    public void Validate_WhenBothEmailAndPasswordAreInvalid_ShouldHaveMultipleValidationErrors()
    {
        // Arrange
        var command = new LoginCommand(string.Empty, string.Empty);

        // Act
        var result = this.validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
        result.ShouldHaveValidationErrorFor(x => x.Password);
        result.Errors.Should().HaveCountGreaterThanOrEqualTo(2);
    }

    /// <summary>
    /// Tests that validation succeeds with any non-empty password (no complexity requirements for login).
    /// </summary>
    /// <param name="password">The password to test.</param>
    [Theory]
    [InlineData("a")]
    [InlineData("12345")]
    [InlineData("simple")]
    [InlineData("very long password with spaces and special chars !@#$%")]
    public void Validate_WhenPasswordIsAnyNonEmptyValue_ShouldNotHaveValidationErrorForPassword(string password)
    {
        // Arrange
        var command = new LoginCommand("user@example.com", password);

        // Act
        var result = this.validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }
}
