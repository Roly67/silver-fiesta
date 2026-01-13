// <copyright file="InputValidationServiceTests.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Infrastructure.Options;
using FileConversionApi.Infrastructure.Services;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Moq;

namespace FileConversionApi.UnitTests.Infrastructure.Services;

/// <summary>
/// Unit tests for the <see cref="InputValidationService"/> class.
/// </summary>
public class InputValidationServiceTests
{
    private readonly Mock<ILogger<InputValidationService>> loggerMock;

    /// <summary>
    /// Initializes a new instance of the <see cref="InputValidationServiceTests"/> class.
    /// </summary>
    public InputValidationServiceTests()
    {
        this.loggerMock = new Mock<ILogger<InputValidationService>>();
    }

    /// <summary>
    /// Tests that ValidateUrl returns success for public URLs.
    /// </summary>
    [Fact]
    public void ValidateUrl_PublicUrl_ReturnsSuccess()
    {
        // Arrange
        var settings = CreateDefaultSettings();
        var service = this.CreateService(settings);

        // Act
        var result = service.ValidateUrl("https://example.com/page");

        // Assert
        Assert.True(result.IsSuccess);
    }

    /// <summary>
    /// Tests that ValidateUrl blocks localhost URLs.
    /// </summary>
    /// <param name="url">The localhost URL to test.</param>
    [Theory]
    [InlineData("http://localhost/test")]
    [InlineData("http://localhost:8080/test")]
    [InlineData("https://127.0.0.1/test")]
    [InlineData("http://127.0.0.1:3000/api")]
    public void ValidateUrl_LocalhostUrl_ReturnsFailure(string url)
    {
        // Arrange
        var settings = CreateDefaultSettings();
        var service = this.CreateService(settings);

        // Act
        var result = service.ValidateUrl(url);

        // Assert
        Assert.True(result.IsFailure);
        Assert.StartsWith("InputValidation.", result.Error.Code);
    }

    /// <summary>
    /// Tests that ValidateUrl blocks private IP addresses.
    /// </summary>
    /// <param name="url">The private IP URL to test.</param>
    [Theory]
    [InlineData("http://10.0.0.1/test")]
    [InlineData("http://192.168.1.1/test")]
    [InlineData("http://172.16.0.1/test")]
    [InlineData("http://169.254.169.254/metadata")]
    public void ValidateUrl_PrivateIpUrl_ReturnsFailure(string url)
    {
        // Arrange
        var settings = CreateDefaultSettings();
        var service = this.CreateService(settings);

        // Act
        var result = service.ValidateUrl(url);

        // Assert
        Assert.True(result.IsFailure);
    }

    /// <summary>
    /// Tests that ValidateUrl allows URLs when validation is disabled.
    /// </summary>
    [Fact]
    public void ValidateUrl_ValidationDisabled_ReturnsSuccess()
    {
        // Arrange
        var settings = CreateDefaultSettings();
        settings.Enabled = false;
        var service = this.CreateService(settings);

        // Act
        var result = service.ValidateUrl("http://localhost/test");

        // Assert
        Assert.True(result.IsSuccess);
    }

    /// <summary>
    /// Tests that ValidateUrl returns success for empty URLs.
    /// </summary>
    /// <param name="url">The empty or null URL to test.</param>
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void ValidateUrl_EmptyUrl_ReturnsSuccess(string? url)
    {
        // Arrange
        var settings = CreateDefaultSettings();
        var service = this.CreateService(settings);

        // Act
        var result = service.ValidateUrl(url!);

        // Assert
        Assert.True(result.IsSuccess);
    }

    /// <summary>
    /// Tests that ValidateUrl works with allowlist mode.
    /// </summary>
    [Fact]
    public void ValidateUrl_AllowlistMode_AllowedDomain_ReturnsSuccess()
    {
        // Arrange
        var settings = CreateDefaultSettings();
        settings.UrlValidation.UseAllowlist = true;
        settings.UrlValidation.Allowlist = ["example.com", "*.github.com"];
        var service = this.CreateService(settings);

        // Act
        var result = service.ValidateUrl("https://example.com/page");

        // Assert
        Assert.True(result.IsSuccess);
    }

    /// <summary>
    /// Tests that ValidateUrl rejects URLs not in allowlist.
    /// </summary>
    [Fact]
    public void ValidateUrl_AllowlistMode_NotAllowedDomain_ReturnsFailure()
    {
        // Arrange
        var settings = CreateDefaultSettings();
        settings.UrlValidation.UseAllowlist = true;
        settings.UrlValidation.Allowlist = ["example.com"];
        var service = this.CreateService(settings);

        // Act
        var result = service.ValidateUrl("https://other.com/page");

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("not in the allowed list", result.Error.Message);
    }

    /// <summary>
    /// Tests that ValidateUrl supports wildcard patterns.
    /// </summary>
    [Fact]
    public void ValidateUrl_AllowlistWithWildcard_MatchesSubdomain()
    {
        // Arrange
        var settings = CreateDefaultSettings();
        settings.UrlValidation.UseAllowlist = true;
        settings.UrlValidation.Allowlist = ["*.github.com"];
        var service = this.CreateService(settings);

        // Act
        var result = service.ValidateUrl("https://api.github.com/repos");

        // Assert
        Assert.True(result.IsSuccess);
    }

    /// <summary>
    /// Tests that ValidateFileSize returns success for files within limit.
    /// </summary>
    [Fact]
    public void ValidateFileSize_WithinLimit_ReturnsSuccess()
    {
        // Arrange
        var settings = CreateDefaultSettings();
        settings.MaxFileSizeBytes = 10 * 1024 * 1024; // 10MB
        var service = this.CreateService(settings);

        // Act
        var result = service.ValidateFileSize(5 * 1024 * 1024); // 5MB

        // Assert
        Assert.True(result.IsSuccess);
    }

    /// <summary>
    /// Tests that ValidateFileSize returns failure for files exceeding limit.
    /// </summary>
    [Fact]
    public void ValidateFileSize_ExceedsLimit_ReturnsFailure()
    {
        // Arrange
        var settings = CreateDefaultSettings();
        settings.MaxFileSizeBytes = 10 * 1024 * 1024; // 10MB
        var service = this.CreateService(settings);

        // Act
        var result = service.ValidateFileSize(15 * 1024 * 1024); // 15MB

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("InputValidation.FileTooLarge", result.Error.Code);
    }

    /// <summary>
    /// Tests that ValidateHtmlContentSize returns success for content within limit.
    /// </summary>
    [Fact]
    public void ValidateHtmlContentSize_WithinLimit_ReturnsSuccess()
    {
        // Arrange
        var settings = CreateDefaultSettings();
        settings.MaxHtmlContentBytes = 1024; // 1KB
        var service = this.CreateService(settings);
        var content = new string('a', 500); // 500 bytes

        // Act
        var result = service.ValidateHtmlContentSize(content);

        // Assert
        Assert.True(result.IsSuccess);
    }

    /// <summary>
    /// Tests that ValidateHtmlContentSize returns failure for content exceeding limit.
    /// </summary>
    [Fact]
    public void ValidateHtmlContentSize_ExceedsLimit_ReturnsFailure()
    {
        // Arrange
        var settings = CreateDefaultSettings();
        settings.MaxHtmlContentBytes = 1024; // 1KB
        var service = this.CreateService(settings);
        var content = new string('a', 2000); // 2000 bytes

        // Act
        var result = service.ValidateHtmlContentSize(content);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("InputValidation.HtmlContentTooLarge", result.Error.Code);
    }

    /// <summary>
    /// Tests that ValidateMarkdownContentSize returns success for content within limit.
    /// </summary>
    [Fact]
    public void ValidateMarkdownContentSize_WithinLimit_ReturnsSuccess()
    {
        // Arrange
        var settings = CreateDefaultSettings();
        settings.MaxMarkdownContentBytes = 1024; // 1KB
        var service = this.CreateService(settings);
        var content = new string('a', 500); // 500 bytes

        // Act
        var result = service.ValidateMarkdownContentSize(content);

        // Assert
        Assert.True(result.IsSuccess);
    }

    /// <summary>
    /// Tests that ValidateMarkdownContentSize returns failure for content exceeding limit.
    /// </summary>
    [Fact]
    public void ValidateMarkdownContentSize_ExceedsLimit_ReturnsFailure()
    {
        // Arrange
        var settings = CreateDefaultSettings();
        settings.MaxMarkdownContentBytes = 1024; // 1KB
        var service = this.CreateService(settings);
        var content = new string('a', 2000); // 2000 bytes

        // Act
        var result = service.ValidateMarkdownContentSize(content);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("InputValidation.MarkdownContentTooLarge", result.Error.Code);
    }

    /// <summary>
    /// Tests that ValidateContentType returns success for allowed HTML content types.
    /// </summary>
    /// <param name="contentType">The content type to test.</param>
    [Theory]
    [InlineData("text/html")]
    [InlineData("text/plain")]
    [InlineData("application/xhtml+xml")]
    [InlineData("text/html; charset=utf-8")]
    public void ValidateContentType_AllowedHtmlType_ReturnsSuccess(string contentType)
    {
        // Arrange
        var settings = CreateDefaultSettings();
        var service = this.CreateService(settings);

        // Act
        var result = service.ValidateContentType(contentType, "html");

        // Assert
        Assert.True(result.IsSuccess);
    }

    /// <summary>
    /// Tests that ValidateContentType returns failure for disallowed content types.
    /// </summary>
    [Fact]
    public void ValidateContentType_DisallowedType_ReturnsFailure()
    {
        // Arrange
        var settings = CreateDefaultSettings();
        var service = this.CreateService(settings);

        // Act
        var result = service.ValidateContentType("application/octet-stream", "html");

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("InputValidation.InvalidContentType", result.Error.Code);
    }

    /// <summary>
    /// Tests that ValidateContentType returns success for allowed image content types.
    /// </summary>
    /// <param name="contentType">The content type to test.</param>
    [Theory]
    [InlineData("image/jpeg")]
    [InlineData("image/png")]
    [InlineData("image/gif")]
    [InlineData("image/webp")]
    public void ValidateContentType_AllowedImageType_ReturnsSuccess(string contentType)
    {
        // Arrange
        var settings = CreateDefaultSettings();
        var service = this.CreateService(settings);

        // Act
        var result = service.ValidateContentType(contentType, "image");

        // Assert
        Assert.True(result.IsSuccess);
    }

    /// <summary>
    /// Tests that ValidateContentType returns success when validation is disabled.
    /// </summary>
    [Fact]
    public void ValidateContentType_ValidationDisabled_ReturnsSuccess()
    {
        // Arrange
        var settings = CreateDefaultSettings();
        settings.ContentTypeValidation.Enabled = false;
        var service = this.CreateService(settings);

        // Act
        var result = service.ValidateContentType("application/octet-stream", "html");

        // Assert
        Assert.True(result.IsSuccess);
    }

    /// <summary>
    /// Tests that GetMaxFileSizeBytes returns configured value.
    /// </summary>
    [Fact]
    public void GetMaxFileSizeBytes_ReturnsConfiguredValue()
    {
        // Arrange
        var settings = CreateDefaultSettings();
        settings.MaxFileSizeBytes = 25 * 1024 * 1024;
        var service = this.CreateService(settings);

        // Act
        var result = service.GetMaxFileSizeBytes();

        // Assert
        Assert.Equal(25 * 1024 * 1024, result);
    }

    /// <summary>
    /// Tests that GetMaxHtmlContentBytes returns configured value.
    /// </summary>
    [Fact]
    public void GetMaxHtmlContentBytes_ReturnsConfiguredValue()
    {
        // Arrange
        var settings = CreateDefaultSettings();
        settings.MaxHtmlContentBytes = 5 * 1024 * 1024;
        var service = this.CreateService(settings);

        // Act
        var result = service.GetMaxHtmlContentBytes();

        // Assert
        Assert.Equal(5 * 1024 * 1024, result);
    }

    /// <summary>
    /// Tests that GetMaxMarkdownContentBytes returns configured value.
    /// </summary>
    [Fact]
    public void GetMaxMarkdownContentBytes_ReturnsConfiguredValue()
    {
        // Arrange
        var settings = CreateDefaultSettings();
        settings.MaxMarkdownContentBytes = 2 * 1024 * 1024;
        var service = this.CreateService(settings);

        // Act
        var result = service.GetMaxMarkdownContentBytes();

        // Assert
        Assert.Equal(2 * 1024 * 1024, result);
    }

    private static InputValidationSettings CreateDefaultSettings()
    {
        return new InputValidationSettings
        {
            Enabled = true,
            MaxFileSizeBytes = 50 * 1024 * 1024,
            MaxHtmlContentBytes = 10 * 1024 * 1024,
            MaxMarkdownContentBytes = 5 * 1024 * 1024,
            UrlValidation = new UrlValidationSettings
            {
                Enabled = true,
                UseAllowlist = false,
                BlockPrivateIpAddresses = true,
                Blocklist =
                [
                    "localhost",
                    "127.0.0.1",
                    "::1",
                    "10.*",
                    "192.168.*",
                    "172.16.*",
                    "169.254.*",
                ],
            },
            ContentTypeValidation = new ContentTypeValidationSettings
            {
                Enabled = true,
            },
        };
    }

    private InputValidationService CreateService(InputValidationSettings settings)
    {
        var options = Options.Create(settings);
        return new InputValidationService(options, this.loggerMock.Object);
    }
}
