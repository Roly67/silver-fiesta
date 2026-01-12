// <copyright file="MarkdownToHtmlConverterTests.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using System.Text;

using FileConversionApi.Application.DTOs;
using FileConversionApi.Infrastructure.Converters;

using FluentAssertions;

using Microsoft.Extensions.Logging;

using Moq;

using Xunit;

namespace FileConversionApi.UnitTests.Infrastructure.Converters;

/// <summary>
/// Unit tests for <see cref="MarkdownToHtmlConverter"/>.
/// </summary>
public class MarkdownToHtmlConverterTests
{
    private readonly Mock<ILogger<MarkdownToHtmlConverter>> loggerMock;
    private readonly MarkdownToHtmlConverter converter;

    /// <summary>
    /// Initializes a new instance of the <see cref="MarkdownToHtmlConverterTests"/> class.
    /// </summary>
    public MarkdownToHtmlConverterTests()
    {
        this.loggerMock = new Mock<ILogger<MarkdownToHtmlConverter>>();
        this.converter = new MarkdownToHtmlConverter(this.loggerMock.Object);
    }

    /// <summary>
    /// Tests that SourceFormat returns markdown.
    /// </summary>
    [Fact]
    public void SourceFormat_ReturnsMarkdown()
    {
        // Assert
        this.converter.SourceFormat.Should().Be("markdown");
    }

    /// <summary>
    /// Tests that TargetFormat returns html.
    /// </summary>
    [Fact]
    public void TargetFormat_ReturnsHtml()
    {
        // Assert
        this.converter.TargetFormat.Should().Be("html");
    }

    /// <summary>
    /// Tests that ConvertAsync converts markdown to HTML successfully.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task ConvertAsync_WhenMarkdownIsValid_ReturnsHtmlContent()
    {
        // Arrange
        var markdown = "# Hello World\n\nThis is a test.";
        using var inputStream = new MemoryStream(Encoding.UTF8.GetBytes(markdown));
        var options = new ConversionOptions();

        // Act
        var result = await this.converter.ConvertAsync(inputStream, options, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var html = Encoding.UTF8.GetString(result.Value);
        html.Should().Contain("Hello World</h1>");
        html.Should().Contain("<p>This is a test.</p>");
        html.Should().Contain("<!DOCTYPE html>");
        html.Should().Contain("<html lang=\"en\">");
    }

    /// <summary>
    /// Tests that ConvertAsync handles empty markdown.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task ConvertAsync_WhenMarkdownIsEmpty_ReturnsHtmlWithEmptyBody()
    {
        // Arrange
        var markdown = string.Empty;
        using var inputStream = new MemoryStream(Encoding.UTF8.GetBytes(markdown));
        var options = new ConversionOptions();

        // Act
        var result = await this.converter.ConvertAsync(inputStream, options, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var html = Encoding.UTF8.GetString(result.Value);
        html.Should().Contain("<!DOCTYPE html>");
        html.Should().Contain("<article class=\"markdown-body\">");
    }

    /// <summary>
    /// Tests that ConvertAsync handles markdown with code blocks.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task ConvertAsync_WhenMarkdownContainsCodeBlock_ReturnsHtmlWithPreTag()
    {
        // Arrange
        var markdown = "```csharp\nvar x = 1;\n```";
        using var inputStream = new MemoryStream(Encoding.UTF8.GetBytes(markdown));
        var options = new ConversionOptions();

        // Act
        var result = await this.converter.ConvertAsync(inputStream, options, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var html = Encoding.UTF8.GetString(result.Value);
        html.Should().Contain("<pre>");
        html.Should().Contain("<code");
        html.Should().Contain("var x = 1;");
    }

    /// <summary>
    /// Tests that ConvertAsync handles markdown with links.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task ConvertAsync_WhenMarkdownContainsLinks_ReturnsHtmlWithAnchorTags()
    {
        // Arrange
        var markdown = "[Example](https://example.com)";
        using var inputStream = new MemoryStream(Encoding.UTF8.GetBytes(markdown));
        var options = new ConversionOptions();

        // Act
        var result = await this.converter.ConvertAsync(inputStream, options, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var html = Encoding.UTF8.GetString(result.Value);
        html.Should().Contain("<a href=\"https://example.com\"");
        html.Should().Contain(">Example</a>");
    }

    /// <summary>
    /// Tests that ConvertAsync handles markdown with tables.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task ConvertAsync_WhenMarkdownContainsTables_ReturnsHtmlWithTableTags()
    {
        // Arrange
        var markdown = "| Header 1 | Header 2 |\n|----------|----------|\n| Cell 1   | Cell 2   |";
        using var inputStream = new MemoryStream(Encoding.UTF8.GetBytes(markdown));
        var options = new ConversionOptions();

        // Act
        var result = await this.converter.ConvertAsync(inputStream, options, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var html = Encoding.UTF8.GetString(result.Value);
        html.Should().Contain("<table>");
        html.Should().Contain("<th>Header 1</th>");
        html.Should().Contain("<td>Cell 1</td>");
    }

    /// <summary>
    /// Tests that ConvertAsync handles markdown with task lists.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task ConvertAsync_WhenMarkdownContainsTaskLists_ReturnsHtmlWithCheckboxes()
    {
        // Arrange
        var markdown = "- [x] Done\n- [ ] Not done";
        using var inputStream = new MemoryStream(Encoding.UTF8.GetBytes(markdown));
        var options = new ConversionOptions();

        // Act
        var result = await this.converter.ConvertAsync(inputStream, options, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var html = Encoding.UTF8.GetString(result.Value);
        html.Should().Contain("type=\"checkbox\"");
        html.Should().Contain("checked");
    }

    /// <summary>
    /// Tests that ConvertAsync includes CSS styling.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task ConvertAsync_ReturnsHtmlWithStyling()
    {
        // Arrange
        var markdown = "# Test";
        using var inputStream = new MemoryStream(Encoding.UTF8.GetBytes(markdown));
        var options = new ConversionOptions();

        // Act
        var result = await this.converter.ConvertAsync(inputStream, options, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var html = Encoding.UTF8.GetString(result.Value);
        html.Should().Contain("<style>");
        html.Should().Contain("font-family:");
        html.Should().Contain(".markdown-body");
    }

    /// <summary>
    /// Tests that constructor throws ArgumentNullException when logger is null.
    /// </summary>
    [Fact]
    public void Constructor_WhenLoggerIsNull_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new MarkdownToHtmlConverter(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }
}
