// <copyright file="ConvertHtmlToPdfCommandValidatorTests.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Application.Commands.Conversion;
using FileConversionApi.Application.DTOs;

using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace FileConversionApi.UnitTests.Application.Validators;

/// <summary>
/// Unit tests for <see cref="ConvertHtmlToPdfCommandValidator"/>.
/// </summary>
public class ConvertHtmlToPdfCommandValidatorTests
{
    private readonly ConvertHtmlToPdfCommandValidator validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConvertHtmlToPdfCommandValidatorTests"/> class.
    /// </summary>
    public ConvertHtmlToPdfCommandValidatorTests()
    {
        this.validator = new ConvertHtmlToPdfCommandValidator();
    }

    /// <summary>
    /// Tests that validation succeeds when HtmlContent is provided.
    /// </summary>
    [Fact]
    public void Validate_WhenHtmlContentIsProvided_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var command = new ConvertHtmlToPdfCommand
        {
            HtmlContent = "<html><body>Hello World</body></html>",
        };

        // Act
        var result = this.validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Tests that validation succeeds when Url is provided.
    /// </summary>
    [Fact]
    public void Validate_WhenUrlIsProvided_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var command = new ConvertHtmlToPdfCommand
        {
            Url = "https://example.com",
        };

        // Act
        var result = this.validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Tests that validation succeeds when both HtmlContent and Url are provided.
    /// </summary>
    [Fact]
    public void Validate_WhenBothHtmlContentAndUrlAreProvided_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var command = new ConvertHtmlToPdfCommand
        {
            HtmlContent = "<html><body>Hello</body></html>",
            Url = "https://example.com",
        };

        // Act
        var result = this.validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Tests that validation fails when neither HtmlContent nor Url is provided.
    /// </summary>
    [Fact]
    public void Validate_WhenNeitherHtmlContentNorUrlIsProvided_ShouldHaveValidationError()
    {
        // Arrange
        var command = new ConvertHtmlToPdfCommand();

        // Act
        var result = this.validator.TestValidate(command);

        // Assert
        result.Errors.Should().Contain(e => e.ErrorMessage == "Either HtmlContent or Url must be provided.");
    }

    /// <summary>
    /// Tests that validation fails when both HtmlContent and Url are null.
    /// </summary>
    [Fact]
    public void Validate_WhenBothHtmlContentAndUrlAreNull_ShouldHaveValidationError()
    {
        // Arrange
        var command = new ConvertHtmlToPdfCommand
        {
            HtmlContent = null,
            Url = null,
        };

        // Act
        var result = this.validator.TestValidate(command);

        // Assert
        result.Errors.Should().Contain(e => e.ErrorMessage == "Either HtmlContent or Url must be provided.");
    }

    /// <summary>
    /// Tests that validation fails when both HtmlContent and Url are whitespace.
    /// </summary>
    [Fact]
    public void Validate_WhenBothHtmlContentAndUrlAreWhitespace_ShouldHaveValidationError()
    {
        // Arrange
        var command = new ConvertHtmlToPdfCommand
        {
            HtmlContent = "   ",
            Url = "   ",
        };

        // Act
        var result = this.validator.TestValidate(command);

        // Assert
        result.Errors.Should().Contain(e => e.ErrorMessage == "Either HtmlContent or Url must be provided.");
    }

    /// <summary>
    /// Tests that validation fails when Url is not a valid HTTP or HTTPS URL.
    /// </summary>
    /// <param name="invalidUrl">The invalid URL to test.</param>
    [Theory]
    [InlineData("ftp://example.com")]
    [InlineData("file:///path/to/file")]
    [InlineData("mailto:user@example.com")]
    [InlineData("javascript:alert('hi')")]
    [InlineData("data:text/html,<h1>Hello</h1>")]
    public void Validate_WhenUrlIsNotHttpOrHttps_ShouldHaveValidationError(string invalidUrl)
    {
        // Arrange
        var command = new ConvertHtmlToPdfCommand
        {
            Url = invalidUrl,
        };

        // Act
        var result = this.validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Url)
            .WithErrorMessage("Url must be a valid HTTP or HTTPS URL.");
    }

    /// <summary>
    /// Tests that validation fails when Url is not a valid URI.
    /// </summary>
    /// <param name="invalidUrl">The invalid URL to test.</param>
    [Theory]
    [InlineData("not-a-url")]
    [InlineData("example.com")]
    [InlineData("//example.com")]
    public void Validate_WhenUrlIsNotValidUri_ShouldHaveValidationError(string invalidUrl)
    {
        // Arrange
        var command = new ConvertHtmlToPdfCommand
        {
            Url = invalidUrl,
        };

        // Act
        var result = this.validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Url)
            .WithErrorMessage("Url must be a valid HTTP or HTTPS URL.");
    }

    /// <summary>
    /// Tests that validation succeeds with valid HTTP and HTTPS URLs.
    /// </summary>
    /// <param name="validUrl">The valid URL to test.</param>
    [Theory]
    [InlineData("http://example.com")]
    [InlineData("https://example.com")]
    [InlineData("http://example.com/path")]
    [InlineData("https://example.com/path/to/page.html")]
    [InlineData("http://example.com:8080")]
    [InlineData("https://subdomain.example.com")]
    [InlineData("http://example.com?query=value")]
    [InlineData("https://example.com/path?query=value#anchor")]
    public void Validate_WhenUrlIsValidHttpOrHttps_ShouldNotHaveValidationErrorForUrl(string validUrl)
    {
        // Arrange
        var command = new ConvertHtmlToPdfCommand
        {
            Url = validUrl,
        };

        // Act
        var result = this.validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Url);
    }

    /// <summary>
    /// Tests that validation fails when HtmlContent exceeds 10MB.
    /// </summary>
    [Fact]
    public void Validate_WhenHtmlContentExceeds10MB_ShouldHaveValidationError()
    {
        // Arrange
        var largeContent = new string('x', (10 * 1024 * 1024) + 1); // 10MB + 1 byte
        var command = new ConvertHtmlToPdfCommand
        {
            HtmlContent = largeContent,
        };

        // Act
        var result = this.validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.HtmlContent)
            .WithErrorMessage("HtmlContent must not exceed 10MB.");
    }

    /// <summary>
    /// Tests that validation succeeds when HtmlContent is exactly 10MB.
    /// </summary>
    [Fact]
    public void Validate_WhenHtmlContentIsExactly10MB_ShouldNotHaveValidationError()
    {
        // Arrange
        var exactContent = new string('x', 10 * 1024 * 1024); // Exactly 10MB
        var command = new ConvertHtmlToPdfCommand
        {
            HtmlContent = exactContent,
        };

        // Act
        var result = this.validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.HtmlContent);
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
    [InlineData("LeTtEr")]
    public void Validate_WhenPageSizeIsValid_ShouldNotHaveValidationError(string pageSize)
    {
        // Arrange
        var command = new ConvertHtmlToPdfCommand
        {
            HtmlContent = "<html></html>",
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
    [InlineData("Executive")]
    [InlineData("InvalidSize")]
    public void Validate_WhenPageSizeIsInvalid_ShouldHaveValidationError(string invalidPageSize)
    {
        // Arrange
        var command = new ConvertHtmlToPdfCommand
        {
            HtmlContent = "<html></html>",
            Options = new ConversionOptions
            {
                PageSize = invalidPageSize,
            },
        };

        // Act
        var result = this.validator.TestValidate(command);

        // Assert
        result.Errors.Should().Contain(e =>
            e.ErrorMessage == "PageSize must be A4, Letter, Legal, Tabloid, or Ledger.");
    }

    /// <summary>
    /// Tests that validation succeeds when page size is null or empty.
    /// </summary>
    /// <param name="pageSize">The null or empty page size.</param>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WhenPageSizeIsNullOrEmpty_ShouldNotHaveValidationError(string? pageSize)
    {
        // Arrange
        var command = new ConvertHtmlToPdfCommand
        {
            HtmlContent = "<html></html>",
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
    /// Tests that validation fails when JavaScript timeout is less than 1000ms.
    /// </summary>
    /// <param name="timeout">The invalid timeout value.</param>
    [Theory]
    [InlineData(0)]
    [InlineData(500)]
    [InlineData(999)]
    [InlineData(-1)]
    public void Validate_WhenJavaScriptTimeoutIsLessThan1000_ShouldHaveValidationError(int timeout)
    {
        // Arrange
        var command = new ConvertHtmlToPdfCommand
        {
            HtmlContent = "<html></html>",
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
    /// Tests that validation fails when JavaScript timeout exceeds 120000ms.
    /// </summary>
    /// <param name="timeout">The invalid timeout value.</param>
    [Theory]
    [InlineData(120001)]
    [InlineData(150000)]
    [InlineData(200000)]
    public void Validate_WhenJavaScriptTimeoutExceeds120000_ShouldHaveValidationError(int timeout)
    {
        // Arrange
        var command = new ConvertHtmlToPdfCommand
        {
            HtmlContent = "<html></html>",
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
    [InlineData(5000)]
    [InlineData(30000)]
    [InlineData(60000)]
    [InlineData(120000)]
    public void Validate_WhenJavaScriptTimeoutIsValid_ShouldNotHaveValidationError(int timeout)
    {
        // Arrange
        var command = new ConvertHtmlToPdfCommand
        {
            HtmlContent = "<html></html>",
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
        var command = new ConvertHtmlToPdfCommand
        {
            HtmlContent = "<html></html>",
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
        var command = new ConvertHtmlToPdfCommand
        {
            HtmlContent = "<html><body>Test</body></html>",
            FileName = "output.pdf",
            Options = new ConversionOptions
            {
                PageSize = "A4",
                Landscape = true,
                MarginTop = 30,
                MarginBottom = 30,
                MarginLeft = 25,
                MarginRight = 25,
                HeaderTemplate = "<div>Header</div>",
                FooterTemplate = "<div>Footer</div>",
                WaitForJavaScript = true,
                JavaScriptTimeout = 30000,
            },
        };

        // Act
        var result = this.validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Tests that validation fails with multiple invalid options.
    /// </summary>
    [Fact]
    public void Validate_WhenMultipleOptionsAreInvalid_ShouldHaveMultipleValidationErrors()
    {
        // Arrange
        var command = new ConvertHtmlToPdfCommand
        {
            HtmlContent = "<html></html>",
            Options = new ConversionOptions
            {
                PageSize = "InvalidSize",
                JavaScriptTimeout = 500,
            },
        };

        // Act
        var result = this.validator.TestValidate(command);

        // Assert
        result.Errors.Should().HaveCountGreaterThanOrEqualTo(2);
    }
}
