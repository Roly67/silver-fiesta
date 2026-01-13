// <copyright file="ConvertMarkdownToPdfCommandValidatorTests.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Application.Commands.Conversion;
using FileConversionApi.Application.DTOs;

using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace FileConversionApi.UnitTests.Application.Validators;

/// <summary>
/// Unit tests for <see cref="ConvertMarkdownToPdfCommandValidator"/>.
/// </summary>
public class ConvertMarkdownToPdfCommandValidatorTests
{
    private readonly ConvertMarkdownToPdfCommandValidator validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConvertMarkdownToPdfCommandValidatorTests"/> class.
    /// </summary>
    public ConvertMarkdownToPdfCommandValidatorTests()
    {
        this.validator = new ConvertMarkdownToPdfCommandValidator();
    }

    /// <summary>
    /// Tests that validation succeeds when Markdown content is provided.
    /// </summary>
    [Fact]
    public void Validate_WhenMarkdownIsProvided_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var command = new ConvertMarkdownToPdfCommand
        {
            Markdown = "# Hello World\n\nThis is a test.",
        };

        // Act
        var result = this.validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Tests that validation fails when Markdown content is null.
    /// </summary>
    [Fact]
    public void Validate_WhenMarkdownIsNull_ShouldHaveValidationError()
    {
        // Arrange
        var command = new ConvertMarkdownToPdfCommand
        {
            Markdown = null,
        };

        // Act
        var result = this.validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Markdown)
            .WithErrorMessage("Markdown content is required.");
    }

    /// <summary>
    /// Tests that validation fails when Markdown content is empty.
    /// </summary>
    [Fact]
    public void Validate_WhenMarkdownIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        var command = new ConvertMarkdownToPdfCommand
        {
            Markdown = string.Empty,
        };

        // Act
        var result = this.validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Markdown)
            .WithErrorMessage("Markdown content is required.");
    }

    /// <summary>
    /// Tests that validation fails when Markdown content is whitespace only.
    /// </summary>
    [Fact]
    public void Validate_WhenMarkdownIsWhitespace_ShouldHaveValidationError()
    {
        // Arrange
        var command = new ConvertMarkdownToPdfCommand
        {
            Markdown = "   ",
        };

        // Act
        var result = this.validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Markdown)
            .WithErrorMessage("Markdown content is required.");
    }

    /// <summary>
    /// Tests that validation fails when Markdown content exceeds 5MB.
    /// </summary>
    [Fact]
    public void Validate_WhenMarkdownExceeds5MB_ShouldHaveValidationError()
    {
        // Arrange
        var largeContent = new string('x', (5 * 1024 * 1024) + 1); // 5MB + 1 byte
        var command = new ConvertMarkdownToPdfCommand
        {
            Markdown = largeContent,
        };

        // Act
        var result = this.validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Markdown)
            .WithErrorMessage("Markdown content must not exceed 5MB.");
    }

    /// <summary>
    /// Tests that validation succeeds when Markdown content is exactly 5MB.
    /// </summary>
    [Fact]
    public void Validate_WhenMarkdownIsExactly5MB_ShouldNotHaveValidationError()
    {
        // Arrange
        var exactContent = new string('x', 5 * 1024 * 1024); // Exactly 5MB
        var command = new ConvertMarkdownToPdfCommand
        {
            Markdown = exactContent,
        };

        // Act
        var result = this.validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Markdown);
    }

    /// <summary>
    /// Tests that validation succeeds with valid page sizes.
    /// </summary>
    /// <param name="pageSize">The valid page size to test.</param>
    [Theory]
    [InlineData("A4")]
    [InlineData("Letter")]
    [InlineData("Legal")]
    [InlineData("Tabloid")]
    [InlineData("Ledger")]
    [InlineData("A3")]
    [InlineData("A5")]
    [InlineData("a4")]
    [InlineData("LETTER")]
    public void Validate_WhenPageSizeIsValid_ShouldNotHaveValidationError(string pageSize)
    {
        // Arrange
        var command = new ConvertMarkdownToPdfCommand
        {
            Markdown = "# Test",
            Options = new ConversionOptions
            {
                PageSize = pageSize,
            },
        };

        // Act
        var result = this.validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Tests that validation fails when page size is invalid.
    /// </summary>
    /// <param name="invalidPageSize">The invalid page size to test.</param>
    [Theory]
    [InlineData("B4")]
    [InlineData("A6")]
    [InlineData("Custom")]
    [InlineData("InvalidSize")]
    public void Validate_WhenPageSizeIsInvalid_ShouldHaveValidationError(string invalidPageSize)
    {
        // Arrange
        var command = new ConvertMarkdownToPdfCommand
        {
            Markdown = "# Test",
            Options = new ConversionOptions
            {
                PageSize = invalidPageSize,
            },
        };

        // Act
        var result = this.validator.TestValidate(command);

        // Assert
        result.Errors.Should().Contain(e =>
            e.ErrorMessage == "PageSize must be A4, Letter, Legal, Tabloid, Ledger, A3, or A5.");
    }

    /// <summary>
    /// Tests that validation fails when JavaScript timeout is out of range.
    /// </summary>
    /// <param name="timeout">The invalid timeout value.</param>
    [Theory]
    [InlineData(0)]
    [InlineData(500)]
    [InlineData(999)]
    [InlineData(120001)]
    [InlineData(200000)]
    public void Validate_WhenJavaScriptTimeoutIsOutOfRange_ShouldHaveValidationError(int timeout)
    {
        // Arrange
        var command = new ConvertMarkdownToPdfCommand
        {
            Markdown = "# Test",
            Options = new ConversionOptions
            {
                JavaScriptTimeout = timeout,
            },
        };

        // Act
        var result = this.validator.TestValidate(command);

        // Assert
        result.Errors.Should().Contain(e =>
            e.ErrorMessage == "JavaScriptTimeout must be between 1000 and 120000 milliseconds.");
    }

    /// <summary>
    /// Tests that validation succeeds with valid JavaScript timeout values.
    /// </summary>
    /// <param name="timeout">The valid timeout value.</param>
    [Theory]
    [InlineData(1000)]
    [InlineData(30000)]
    [InlineData(120000)]
    public void Validate_WhenJavaScriptTimeoutIsValid_ShouldNotHaveValidationError(int timeout)
    {
        // Arrange
        var command = new ConvertMarkdownToPdfCommand
        {
            Markdown = "# Test",
            Options = new ConversionOptions
            {
                JavaScriptTimeout = timeout,
            },
        };

        // Act
        var result = this.validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Tests that validation succeeds when Options is null.
    /// </summary>
    [Fact]
    public void Validate_WhenOptionsIsNull_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var command = new ConvertMarkdownToPdfCommand
        {
            Markdown = "# Test",
            Options = null,
        };

        // Act
        var result = this.validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Tests that validation succeeds with all valid options.
    /// </summary>
    [Fact]
    public void Validate_WhenAllOptionsAreValid_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var command = new ConvertMarkdownToPdfCommand
        {
            Markdown = "# Hello World\n\n**Bold text** and *italic text*.",
            FileName = "output.pdf",
            Options = new ConversionOptions
            {
                PageSize = "A4",
                Landscape = false,
                MarginTop = 25,
                MarginBottom = 25,
                MarginLeft = 20,
                MarginRight = 20,
                WaitForJavaScript = true,
                JavaScriptTimeout = 30000,
            },
        };

        // Act
        var result = this.validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
