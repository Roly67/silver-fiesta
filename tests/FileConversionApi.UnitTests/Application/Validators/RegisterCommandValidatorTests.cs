// <copyright file="RegisterCommandValidatorTests.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Application.Commands.Auth;

using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace FileConversionApi.UnitTests.Application.Validators;

/// <summary>
/// Unit tests for <see cref="RegisterCommandValidator"/>.
/// </summary>
public class RegisterCommandValidatorTests
{
    private readonly RegisterCommandValidator validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterCommandValidatorTests"/> class.
    /// </summary>
    public RegisterCommandValidatorTests()
    {
        this.validator = new RegisterCommandValidator();
    }

    /// <summary>
    /// Tests that validation succeeds when all fields are valid.
    /// </summary>
    [Fact]
    public void Validate_WhenAllFieldsAreValid_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var command = new RegisterCommand("user@example.com", "Password1");

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
        var command = new RegisterCommand(string.Empty, "Password1");

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
        var command = new RegisterCommand(null!, "Password1");

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
        var command = new RegisterCommand("   ", "Password1");

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
        var command = new RegisterCommand(invalidEmail, "Password1");

        // Act
        var result = this.validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("A valid email address is required.");
    }

    /// <summary>
    /// Tests that validation fails when email exceeds 256 characters.
    /// </summary>
    [Fact]
    public void Validate_WhenEmailExceeds256Characters_ShouldHaveValidationError()
    {
        // Arrange - create email that exceeds 256 characters (265 total)
        var longEmail = new string('a', 250) + "@" + new string('b', 10) + ".com";
        var command = new RegisterCommand(longEmail, "Password1");

        // Act
        var result = this.validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email must not exceed 256 characters.");
    }

    /// <summary>
    /// Tests that validation succeeds when email is exactly 256 characters.
    /// </summary>
    [Fact]
    public void Validate_WhenEmailIsExactly256Characters_ShouldNotHaveValidationErrorForEmail()
    {
        // Arrange
        var email = new string('a', 249) + "@b.com"; // 256 characters
        var command = new RegisterCommand(email, "Password1");

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
        var command = new RegisterCommand("user@example.com", string.Empty);

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
        var command = new RegisterCommand("user@example.com", null!);

        // Act
        var result = this.validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password is required.");
    }

    /// <summary>
    /// Tests that validation fails when password is less than 8 characters.
    /// </summary>
    /// <param name="shortPassword">The short password to test.</param>
    [Theory]
    [InlineData("Pass1")]
    [InlineData("Pa1")]
    [InlineData("Abcde1")]
    [InlineData("Abcdef1")]
    public void Validate_WhenPasswordIsLessThan8Characters_ShouldHaveValidationError(string shortPassword)
    {
        // Arrange
        var command = new RegisterCommand("user@example.com", shortPassword);

        // Act
        var result = this.validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password must be at least 8 characters.");
    }

    /// <summary>
    /// Tests that validation succeeds when password is exactly 8 characters.
    /// </summary>
    [Fact]
    public void Validate_WhenPasswordIsExactly8Characters_ShouldNotHaveMinLengthError()
    {
        // Arrange
        var command = new RegisterCommand("user@example.com", "Abcdef1g");

        // Act
        var result = this.validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }

    /// <summary>
    /// Tests that validation fails when password exceeds 128 characters.
    /// </summary>
    [Fact]
    public void Validate_WhenPasswordExceeds128Characters_ShouldHaveValidationError()
    {
        // Arrange
        var longPassword = "A" + new string('a', 127) + "1"; // 129 characters
        var command = new RegisterCommand("user@example.com", longPassword);

        // Act
        var result = this.validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password must not exceed 128 characters.");
    }

    /// <summary>
    /// Tests that validation succeeds when password is exactly 128 characters.
    /// </summary>
    [Fact]
    public void Validate_WhenPasswordIsExactly128Characters_ShouldNotHaveMaxLengthError()
    {
        // Arrange
        var password = "A" + new string('a', 126) + "1"; // 128 characters
        var command = new RegisterCommand("user@example.com", password);

        // Act
        var result = this.validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }

    /// <summary>
    /// Tests that validation fails when password has no uppercase letter.
    /// </summary>
    /// <param name="passwordWithoutUppercase">The password without uppercase to test.</param>
    [Theory]
    [InlineData("password1")]
    [InlineData("alllowercase1")]
    [InlineData("12345678a")]
    public void Validate_WhenPasswordHasNoUppercase_ShouldHaveValidationError(string passwordWithoutUppercase)
    {
        // Arrange
        var command = new RegisterCommand("user@example.com", passwordWithoutUppercase);

        // Act
        var result = this.validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password must contain at least one uppercase letter.");
    }

    /// <summary>
    /// Tests that validation fails when password has no lowercase letter.
    /// </summary>
    /// <param name="passwordWithoutLowercase">The password without lowercase to test.</param>
    [Theory]
    [InlineData("PASSWORD1")]
    [InlineData("ALLUPPERCASE1")]
    [InlineData("12345678A")]
    public void Validate_WhenPasswordHasNoLowercase_ShouldHaveValidationError(string passwordWithoutLowercase)
    {
        // Arrange
        var command = new RegisterCommand("user@example.com", passwordWithoutLowercase);

        // Act
        var result = this.validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password must contain at least one lowercase letter.");
    }

    /// <summary>
    /// Tests that validation fails when password has no digit.
    /// </summary>
    /// <param name="passwordWithoutDigit">The password without digit to test.</param>
    [Theory]
    [InlineData("Password")]
    [InlineData("NoDigitsHere")]
    [InlineData("ALLUPPERCASE")]
    public void Validate_WhenPasswordHasNoDigit_ShouldHaveValidationError(string passwordWithoutDigit)
    {
        // Arrange
        var command = new RegisterCommand("user@example.com", passwordWithoutDigit);

        // Act
        var result = this.validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password must contain at least one digit.");
    }

    /// <summary>
    /// Tests that validation succeeds with various valid passwords.
    /// </summary>
    /// <param name="validPassword">The valid password to test.</param>
    [Theory]
    [InlineData("Password1")]
    [InlineData("Abcdefg1")]
    [InlineData("MySecure123Password")]
    [InlineData("Test1234")]
    [InlineData("Complex1Password!@#")]
    public void Validate_WhenPasswordMeetsAllRequirements_ShouldNotHaveValidationErrors(string validPassword)
    {
        // Arrange
        var command = new RegisterCommand("user@example.com", validPassword);

        // Act
        var result = this.validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Tests that validation fails with multiple errors when password violates multiple rules.
    /// </summary>
    [Fact]
    public void Validate_WhenPasswordViolatesMultipleRules_ShouldHaveMultipleValidationErrors()
    {
        // Arrange - all lowercase, no digit, too short
        var command = new RegisterCommand("user@example.com", "short");

        // Act
        var result = this.validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password);
        result.Errors.Should().HaveCountGreaterThan(1);
    }

    /// <summary>
    /// Tests that validation fails with multiple errors when both email and password are invalid.
    /// </summary>
    [Fact]
    public void Validate_WhenBothEmailAndPasswordAreInvalid_ShouldHaveMultipleValidationErrors()
    {
        // Arrange
        var command = new RegisterCommand("invalid-email", "weak");

        // Act
        var result = this.validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }
}
