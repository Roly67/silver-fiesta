// <copyright file="ImageConverterTests.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Application.DTOs;
using FileConversionApi.Infrastructure.Converters;

using FluentAssertions;

using Microsoft.Extensions.Logging;

using Moq;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;

using Xunit;

namespace FileConversionApi.UnitTests.Infrastructure.Converters;

/// <summary>
/// Unit tests for image converters.
/// </summary>
public class ImageConverterTests
{
    /// <summary>
    /// Tests that PngToJpegConverter has correct format properties.
    /// </summary>
    [Fact]
    public void PngToJpegConverter_HasCorrectFormats()
    {
        // Arrange
        var logger = new Mock<ILogger<PngToJpegConverter>>();
        var converter = new PngToJpegConverter(logger.Object);

        // Assert
        converter.SourceFormat.Should().Be("png");
        converter.TargetFormat.Should().Be("jpeg");
    }

    /// <summary>
    /// Tests that JpegToPngConverter has correct format properties.
    /// </summary>
    [Fact]
    public void JpegToPngConverter_HasCorrectFormats()
    {
        // Arrange
        var logger = new Mock<ILogger<JpegToPngConverter>>();
        var converter = new JpegToPngConverter(logger.Object);

        // Assert
        converter.SourceFormat.Should().Be("jpeg");
        converter.TargetFormat.Should().Be("png");
    }

    /// <summary>
    /// Tests that PngToWebpConverter has correct format properties.
    /// </summary>
    [Fact]
    public void PngToWebpConverter_HasCorrectFormats()
    {
        // Arrange
        var logger = new Mock<ILogger<PngToWebpConverter>>();
        var converter = new PngToWebpConverter(logger.Object);

        // Assert
        converter.SourceFormat.Should().Be("png");
        converter.TargetFormat.Should().Be("webp");
    }

    /// <summary>
    /// Tests that JpegToWebpConverter has correct format properties.
    /// </summary>
    [Fact]
    public void JpegToWebpConverter_HasCorrectFormats()
    {
        // Arrange
        var logger = new Mock<ILogger<JpegToWebpConverter>>();
        var converter = new JpegToWebpConverter(logger.Object);

        // Assert
        converter.SourceFormat.Should().Be("jpeg");
        converter.TargetFormat.Should().Be("webp");
    }

    /// <summary>
    /// Tests that WebpToPngConverter has correct format properties.
    /// </summary>
    [Fact]
    public void WebpToPngConverter_HasCorrectFormats()
    {
        // Arrange
        var logger = new Mock<ILogger<WebpToPngConverter>>();
        var converter = new WebpToPngConverter(logger.Object);

        // Assert
        converter.SourceFormat.Should().Be("webp");
        converter.TargetFormat.Should().Be("png");
    }

    /// <summary>
    /// Tests that WebpToJpegConverter has correct format properties.
    /// </summary>
    [Fact]
    public void WebpToJpegConverter_HasCorrectFormats()
    {
        // Arrange
        var logger = new Mock<ILogger<WebpToJpegConverter>>();
        var converter = new WebpToJpegConverter(logger.Object);

        // Assert
        converter.SourceFormat.Should().Be("webp");
        converter.TargetFormat.Should().Be("jpeg");
    }

    /// <summary>
    /// Tests that PngToJpegConverter converts PNG to JPEG successfully.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task PngToJpegConverter_ConvertAsync_ConvertsSuccessfully()
    {
        // Arrange
        var logger = new Mock<ILogger<PngToJpegConverter>>();
        var converter = new PngToJpegConverter(logger.Object);
        var options = new ConversionOptions();

        using var pngStream = CreateTestPngImage(100, 100);

        // Act
        var result = await converter.ConvertAsync(pngStream, options, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        // Verify output is valid JPEG (starts with FFD8)
        result.Value[0].Should().Be(0xFF);
        result.Value[1].Should().Be(0xD8);
    }

    /// <summary>
    /// Tests that JpegToPngConverter converts JPEG to PNG successfully.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task JpegToPngConverter_ConvertAsync_ConvertsSuccessfully()
    {
        // Arrange
        var logger = new Mock<ILogger<JpegToPngConverter>>();
        var converter = new JpegToPngConverter(logger.Object);
        var options = new ConversionOptions();

        using var jpegStream = CreateTestJpegImage(100, 100);

        // Act
        var result = await converter.ConvertAsync(jpegStream, options, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        // Verify output is valid PNG (starts with PNG signature)
        result.Value[0].Should().Be(0x89);
        result.Value[1].Should().Be(0x50); // P
        result.Value[2].Should().Be(0x4E); // N
        result.Value[3].Should().Be(0x47); // G
    }

    /// <summary>
    /// Tests that PngToWebpConverter converts PNG to WebP successfully.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task PngToWebpConverter_ConvertAsync_ConvertsSuccessfully()
    {
        // Arrange
        var logger = new Mock<ILogger<PngToWebpConverter>>();
        var converter = new PngToWebpConverter(logger.Object);
        var options = new ConversionOptions();

        using var pngStream = CreateTestPngImage(100, 100);

        // Act
        var result = await converter.ConvertAsync(pngStream, options, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        // Verify output is valid WebP (starts with RIFF...WEBP)
        result.Value[0].Should().Be(0x52); // R
        result.Value[1].Should().Be(0x49); // I
        result.Value[2].Should().Be(0x46); // F
        result.Value[3].Should().Be(0x46); // F
    }

    /// <summary>
    /// Tests that converter applies image quality option.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Converter_WithQualityOption_AppliesQuality()
    {
        // Arrange
        var logger = new Mock<ILogger<PngToJpegConverter>>();
        var converter = new PngToJpegConverter(logger.Object);
        var highQualityOptions = new ConversionOptions { ImageQuality = 100 };
        var lowQualityOptions = new ConversionOptions { ImageQuality = 10 };

        using var pngStream1 = CreateTestPngImage(100, 100);
        using var pngStream2 = CreateTestPngImage(100, 100);

        // Act
        var highQualityResult = await converter.ConvertAsync(pngStream1, highQualityOptions, CancellationToken.None);
        var lowQualityResult = await converter.ConvertAsync(pngStream2, lowQualityOptions, CancellationToken.None);

        // Assert
        highQualityResult.IsSuccess.Should().BeTrue();
        lowQualityResult.IsSuccess.Should().BeTrue();

        // Lower quality should result in smaller file size
        lowQualityResult.Value.Length.Should().BeLessThan(highQualityResult.Value.Length);
    }

    /// <summary>
    /// Tests that converter applies resize options.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Converter_WithResizeOptions_ResizesImage()
    {
        // Arrange
        var logger = new Mock<ILogger<PngToJpegConverter>>();
        var converter = new PngToJpegConverter(logger.Object);
        var options = new ConversionOptions { ImageWidth = 50, ImageHeight = 50 };

        using var pngStream = CreateTestPngImage(100, 100);

        // Act
        var result = await converter.ConvertAsync(pngStream, options, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Load the output and verify dimensions
        using var outputStream = new MemoryStream(result.Value);
        using var outputImage = await Image.LoadAsync(outputStream);
        outputImage.Width.Should().BeLessThanOrEqualTo(50);
        outputImage.Height.Should().BeLessThanOrEqualTo(50);
    }

    /// <summary>
    /// Tests that converter returns failure for invalid image data.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Converter_WithInvalidImageData_ReturnsFailure()
    {
        // Arrange
        var logger = new Mock<ILogger<PngToJpegConverter>>();
        var converter = new PngToJpegConverter(logger.Object);
        var options = new ConversionOptions();

        using var invalidStream = new MemoryStream("not an image"u8.ToArray());

        // Act
        var result = await converter.ConvertAsync(invalidStream, options, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Unknown or unsupported image format");
    }

    private static MemoryStream CreateTestPngImage(int width, int height)
    {
        using var image = new Image<Rgba32>(width, height);

        // Fill with a gradient for more realistic test data
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                image[x, y] = new Rgba32((byte)(x * 255 / width), (byte)(y * 255 / height), 128, 255);
            }
        }

        var stream = new MemoryStream();
        image.Save(stream, new PngEncoder());
        stream.Position = 0;
        return stream;
    }

    private static MemoryStream CreateTestJpegImage(int width, int height)
    {
        using var image = new Image<Rgba32>(width, height);

        // Fill with a gradient
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                image[x, y] = new Rgba32((byte)(x * 255 / width), (byte)(y * 255 / height), 128, 255);
            }
        }

        var stream = new MemoryStream();
        image.Save(stream, new SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder());
        stream.Position = 0;
        return stream;
    }
}
