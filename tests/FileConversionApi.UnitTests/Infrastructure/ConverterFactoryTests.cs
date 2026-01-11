// <copyright file="ConverterFactoryTests.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Application.DTOs;
using FileConversionApi.Application.Interfaces;
using FileConversionApi.Domain.Primitives;
using FileConversionApi.Infrastructure.Converters;
using FluentAssertions;
using Moq;

namespace FileConversionApi.UnitTests.Infrastructure;

/// <summary>
/// Unit tests for the <see cref="ConverterFactory"/> class.
/// </summary>
public class ConverterFactoryTests
{
    /// <summary>
    /// Tests that the constructor throws ArgumentNullException when converters is null.
    /// </summary>
    [Fact]
    public void Constructor_WhenConvertersIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        IEnumerable<IFileConverter>? nullConverters = null;

        // Act
        var act = () => new ConverterFactory(nullConverters!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("converters");
    }

    /// <summary>
    /// Tests that the constructor accepts an empty collection.
    /// </summary>
    [Fact]
    public void Constructor_WhenConvertersIsEmpty_DoesNotThrow()
    {
        // Arrange
        var emptyConverters = Enumerable.Empty<IFileConverter>();

        // Act
        var act = () => new ConverterFactory(emptyConverters);

        // Assert
        act.Should().NotThrow();
    }

    /// <summary>
    /// Tests that GetConverter returns the correct converter for matching formats.
    /// </summary>
    [Fact]
    public void GetConverter_WhenConverterExists_ReturnsCorrectConverter()
    {
        // Arrange
        var mockConverter = CreateMockConverter("html", "pdf");
        var factory = new ConverterFactory(new[] { mockConverter.Object });

        // Act
        var result = factory.GetConverter("html", "pdf");

        // Assert
        result.Should().Be(mockConverter.Object);
    }

    /// <summary>
    /// Tests that GetConverter returns null when no matching converter exists.
    /// </summary>
    [Fact]
    public void GetConverter_WhenNoMatchingConverter_ReturnsNull()
    {
        // Arrange
        var mockConverter = CreateMockConverter("html", "pdf");
        var factory = new ConverterFactory(new[] { mockConverter.Object });

        // Act
        var result = factory.GetConverter("docx", "pdf");

        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    /// Tests that GetConverter returns null when source format does not match.
    /// </summary>
    [Fact]
    public void GetConverter_WhenSourceFormatDoesNotMatch_ReturnsNull()
    {
        // Arrange
        var mockConverter = CreateMockConverter("html", "pdf");
        var factory = new ConverterFactory(new[] { mockConverter.Object });

        // Act
        var result = factory.GetConverter("xml", "pdf");

        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    /// Tests that GetConverter returns null when target format does not match.
    /// </summary>
    [Fact]
    public void GetConverter_WhenTargetFormatDoesNotMatch_ReturnsNull()
    {
        // Arrange
        var mockConverter = CreateMockConverter("html", "pdf");
        var factory = new ConverterFactory(new[] { mockConverter.Object });

        // Act
        var result = factory.GetConverter("html", "docx");

        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    /// Tests that GetConverter is case-insensitive for source format.
    /// </summary>
    [Fact]
    public void GetConverter_WhenSourceFormatDiffersInCase_ReturnsConverter()
    {
        // Arrange
        var mockConverter = CreateMockConverter("html", "pdf");
        var factory = new ConverterFactory(new[] { mockConverter.Object });

        // Act
        var result = factory.GetConverter("HTML", "pdf");

        // Assert
        result.Should().Be(mockConverter.Object);
    }

    /// <summary>
    /// Tests that GetConverter is case-insensitive for target format.
    /// </summary>
    [Fact]
    public void GetConverter_WhenTargetFormatDiffersInCase_ReturnsConverter()
    {
        // Arrange
        var mockConverter = CreateMockConverter("html", "pdf");
        var factory = new ConverterFactory(new[] { mockConverter.Object });

        // Act
        var result = factory.GetConverter("html", "PDF");

        // Assert
        result.Should().Be(mockConverter.Object);
    }

    /// <summary>
    /// Tests that GetConverter is case-insensitive for both formats.
    /// </summary>
    [Fact]
    public void GetConverter_WhenBothFormatsDifferInCase_ReturnsConverter()
    {
        // Arrange
        var mockConverter = CreateMockConverter("html", "pdf");
        var factory = new ConverterFactory(new[] { mockConverter.Object });

        // Act
        var result = factory.GetConverter("HTML", "PDF");

        // Assert
        result.Should().Be(mockConverter.Object);
    }

    /// <summary>
    /// Tests that GetConverter returns the first matching converter when multiple exist.
    /// </summary>
    [Fact]
    public void GetConverter_WhenMultipleConvertersMatch_ReturnsFirst()
    {
        // Arrange
        var mockConverter1 = CreateMockConverter("html", "pdf");
        var mockConverter2 = CreateMockConverter("html", "pdf");
        var factory = new ConverterFactory(new[] { mockConverter1.Object, mockConverter2.Object });

        // Act
        var result = factory.GetConverter("html", "pdf");

        // Assert
        result.Should().Be(mockConverter1.Object);
    }

    /// <summary>
    /// Tests that GetConverter selects correct converter from multiple converters.
    /// </summary>
    [Fact]
    public void GetConverter_WithMultipleConverters_SelectsCorrectOne()
    {
        // Arrange
        var htmlToPdf = CreateMockConverter("html", "pdf");
        var docxToPdf = CreateMockConverter("docx", "pdf");
        var pdfToDocx = CreateMockConverter("pdf", "docx");
        var factory = new ConverterFactory(new[] { htmlToPdf.Object, docxToPdf.Object, pdfToDocx.Object });

        // Act
        var result = factory.GetConverter("docx", "pdf");

        // Assert
        result.Should().Be(docxToPdf.Object);
    }

    /// <summary>
    /// Tests that GetConverter returns null when converters collection is empty.
    /// </summary>
    [Fact]
    public void GetConverter_WhenNoConvertersAvailable_ReturnsNull()
    {
        // Arrange
        var factory = new ConverterFactory(Enumerable.Empty<IFileConverter>());

        // Act
        var result = factory.GetConverter("html", "pdf");

        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    /// Tests that IsConversionSupported returns true when converter exists.
    /// </summary>
    [Fact]
    public void IsConversionSupported_WhenConverterExists_ReturnsTrue()
    {
        // Arrange
        var mockConverter = CreateMockConverter("html", "pdf");
        var factory = new ConverterFactory(new[] { mockConverter.Object });

        // Act
        var result = factory.IsConversionSupported("html", "pdf");

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Tests that IsConversionSupported returns false when no converter exists.
    /// </summary>
    [Fact]
    public void IsConversionSupported_WhenNoConverterExists_ReturnsFalse()
    {
        // Arrange
        var mockConverter = CreateMockConverter("html", "pdf");
        var factory = new ConverterFactory(new[] { mockConverter.Object });

        // Act
        var result = factory.IsConversionSupported("docx", "pdf");

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Tests that IsConversionSupported is case-insensitive.
    /// </summary>
    /// <param name="source">The source format.</param>
    /// <param name="target">The target format.</param>
    [Theory]
    [InlineData("HTML", "pdf")]
    [InlineData("html", "PDF")]
    [InlineData("HTML", "PDF")]
    [InlineData("HtMl", "PdF")]
    public void IsConversionSupported_WhenFormatsDifferInCase_ReturnsTrue(string source, string target)
    {
        // Arrange
        var mockConverter = CreateMockConverter("html", "pdf");
        var factory = new ConverterFactory(new[] { mockConverter.Object });

        // Act
        var result = factory.IsConversionSupported(source, target);

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Tests that IsConversionSupported returns false when converters collection is empty.
    /// </summary>
    [Fact]
    public void IsConversionSupported_WhenNoConvertersAvailable_ReturnsFalse()
    {
        // Arrange
        var factory = new ConverterFactory(Enumerable.Empty<IFileConverter>());

        // Act
        var result = factory.IsConversionSupported("html", "pdf");

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Tests that GetConverter handles mixed case formats stored in converter.
    /// </summary>
    [Fact]
    public void GetConverter_WhenConverterHasMixedCaseFormats_MatchesCorrectly()
    {
        // Arrange
        var mockConverter = CreateMockConverter("HTML", "PDF");
        var factory = new ConverterFactory(new[] { mockConverter.Object });

        // Act
        var result = factory.GetConverter("html", "pdf");

        // Assert
        result.Should().Be(mockConverter.Object);
    }

    /// <summary>
    /// Tests GetConverter with various format combinations.
    /// </summary>
    /// <param name="source">The source format.</param>
    /// <param name="target">The target format.</param>
    /// <param name="shouldFind">Whether a converter should be found.</param>
    [Theory]
    [InlineData("html", "pdf", true)]
    [InlineData("docx", "pdf", false)]
    [InlineData("pdf", "docx", false)]
    [InlineData("xml", "json", false)]
    public void GetConverter_WithVariousFormats_ReturnsExpectedResult(
        string source,
        string target,
        bool shouldFind)
    {
        // Arrange
        var mockConverter = CreateMockConverter("html", "pdf");
        var factory = new ConverterFactory(new[] { mockConverter.Object });

        // Act
        var result = factory.GetConverter(source, target);

        // Assert
        if (shouldFind)
        {
            result.Should().NotBeNull();
        }
        else
        {
            result.Should().BeNull();
        }
    }

    /// <summary>
    /// Tests that GetConverter works with formats containing special characters.
    /// </summary>
    [Fact]
    public void GetConverter_WhenFormatsContainSpecialPatterns_MatchesExactly()
    {
        // Arrange
        var mockConverter = CreateMockConverter("text/html", "application/pdf");
        var factory = new ConverterFactory(new[] { mockConverter.Object });

        // Act
        var result = factory.GetConverter("text/html", "application/pdf");

        // Assert
        result.Should().Be(mockConverter.Object);
    }

    /// <summary>
    /// Tests that GetConverter does not match partial format names.
    /// </summary>
    [Fact]
    public void GetConverter_WhenPartialFormatMatch_ReturnsNull()
    {
        // Arrange
        var mockConverter = CreateMockConverter("html", "pdf");
        var factory = new ConverterFactory(new[] { mockConverter.Object });

        // Act
        var result = factory.GetConverter("htm", "pdf");

        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    /// Tests IsConversionSupported with multiple converters for different format combinations.
    /// </summary>
    /// <param name="source">The source format.</param>
    /// <param name="target">The target format.</param>
    /// <param name="expected">The expected result.</param>
    [Theory]
    [InlineData("html", "pdf", true)]
    [InlineData("docx", "pdf", true)]
    [InlineData("md", "html", true)]
    [InlineData("html", "docx", false)]
    [InlineData("pdf", "html", false)]
    public void IsConversionSupported_WithMultipleConverters_ReturnsCorrectResult(
        string source,
        string target,
        bool expected)
    {
        // Arrange
        var htmlToPdf = CreateMockConverter("html", "pdf");
        var docxToPdf = CreateMockConverter("docx", "pdf");
        var mdToHtml = CreateMockConverter("md", "html");
        var factory = new ConverterFactory(new[] { htmlToPdf.Object, docxToPdf.Object, mdToHtml.Object });

        // Act
        var result = factory.IsConversionSupported(source, target);

        // Assert
        result.Should().Be(expected);
    }

    /// <summary>
    /// Creates a mock IFileConverter with the specified source and target formats.
    /// </summary>
    /// <param name="sourceFormat">The source format.</param>
    /// <param name="targetFormat">The target format.</param>
    /// <returns>A mock of IFileConverter.</returns>
    private static Mock<IFileConverter> CreateMockConverter(string sourceFormat, string targetFormat)
    {
        var mock = new Mock<IFileConverter>();
        mock.Setup(c => c.SourceFormat).Returns(sourceFormat);
        mock.Setup(c => c.TargetFormat).Returns(targetFormat);
        return mock;
    }
}
