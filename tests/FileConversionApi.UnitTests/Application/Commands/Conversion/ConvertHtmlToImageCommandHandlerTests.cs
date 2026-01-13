// <copyright file="ConvertHtmlToImageCommandHandlerTests.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using FileConversionApi.Application.Commands.Conversion;
using FileConversionApi.Application.DTOs;
using FileConversionApi.Application.Interfaces;
using FileConversionApi.Domain.Entities;
using FileConversionApi.Domain.Enums;
using FileConversionApi.Domain.Primitives;
using FileConversionApi.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FileConversionApi.UnitTests.Application.Commands.Conversion;

/// <summary>
/// Unit tests for <see cref="ConvertHtmlToImageCommandHandler"/>.
/// </summary>
public class ConvertHtmlToImageCommandHandlerTests
{
    private readonly Mock<IConversionJobRepository> jobRepositoryMock;
    private readonly Mock<IUnitOfWork> unitOfWorkMock;
    private readonly Mock<IConverterFactory> converterFactoryMock;
    private readonly Mock<ICurrentUserService> currentUserServiceMock;
    private readonly Mock<IWebhookService> webhookServiceMock;
    private readonly Mock<IMetricsService> metricsServiceMock;
    private readonly Mock<ICloudStorageService> cloudStorageServiceMock;
    private readonly Mock<ILogger<ConvertHtmlToImageCommandHandler>> loggerMock;
    private readonly Mock<IFileConverter> fileConverterMock;
    private readonly ConvertHtmlToImageCommandHandler handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConvertHtmlToImageCommandHandlerTests"/> class.
    /// </summary>
    public ConvertHtmlToImageCommandHandlerTests()
    {
        this.jobRepositoryMock = new Mock<IConversionJobRepository>();
        this.unitOfWorkMock = new Mock<IUnitOfWork>();
        this.converterFactoryMock = new Mock<IConverterFactory>();
        this.currentUserServiceMock = new Mock<ICurrentUserService>();
        this.webhookServiceMock = new Mock<IWebhookService>();
        this.metricsServiceMock = new Mock<IMetricsService>();
        this.cloudStorageServiceMock = new Mock<ICloudStorageService>();
        this.loggerMock = new Mock<ILogger<ConvertHtmlToImageCommandHandler>>();
        this.fileConverterMock = new Mock<IFileConverter>();

        this.handler = new ConvertHtmlToImageCommandHandler(
            this.jobRepositoryMock.Object,
            this.unitOfWorkMock.Object,
            this.converterFactoryMock.Object,
            this.currentUserServiceMock.Object,
            this.webhookServiceMock.Object,
            this.metricsServiceMock.Object,
            this.cloudStorageServiceMock.Object,
            this.loggerMock.Object);
    }

    /// <summary>
    /// Tests that Handle returns success with job DTO when conversion to PNG is successful.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenConversionToPngIsSuccessful_ReturnsSuccessWithJobDto()
    {
        // Arrange
        var userId = UserId.New();
        var command = new ConvertHtmlToImageCommand
        {
            HtmlContent = "<html><body>Hello World</body></html>",
            FileName = "test.html",
            TargetFormat = "png",
        };
        var expectedImageBytes = new byte[] { 0x89, 0x50, 0x4E, 0x47 }; // PNG magic bytes

        this.currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        this.converterFactoryMock
            .Setup(x => x.GetConverter("html", "png"))
            .Returns(this.fileConverterMock.Object);

        this.fileConverterMock
            .Setup(x => x.ConvertAsync(It.IsAny<Stream>(), It.IsAny<ConversionOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(expectedImageBytes));

        this.jobRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<ConversionJob>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        this.unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await this.handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.SourceFormat.Should().Be("html");
        result.Value.TargetFormat.Should().Be("png");
        result.Value.Status.Should().Be(ConversionStatus.Completed);
        result.Value.InputFileName.Should().Be("test.html");
        result.Value.OutputFileName.Should().Be("test.png");

        this.converterFactoryMock.Verify(x => x.GetConverter("html", "png"), Times.Once);
        this.jobRepositoryMock.Verify(x => x.AddAsync(It.IsAny<ConversionJob>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that Handle returns success with job DTO when conversion to JPEG is successful.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenConversionToJpegIsSuccessful_ReturnsSuccessWithJobDto()
    {
        // Arrange
        var userId = UserId.New();
        var command = new ConvertHtmlToImageCommand
        {
            HtmlContent = "<html><body>Hello World</body></html>",
            FileName = "test.html",
            TargetFormat = "jpeg",
        };
        var expectedImageBytes = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 }; // JPEG magic bytes

        this.currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        this.converterFactoryMock
            .Setup(x => x.GetConverter("html", "jpeg"))
            .Returns(this.fileConverterMock.Object);

        this.fileConverterMock
            .Setup(x => x.ConvertAsync(It.IsAny<Stream>(), It.IsAny<ConversionOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(expectedImageBytes));

        this.jobRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<ConversionJob>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        this.unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await this.handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.TargetFormat.Should().Be("jpeg");
        result.Value.OutputFileName.Should().Be("test.jpeg");

        this.converterFactoryMock.Verify(x => x.GetConverter("html", "jpeg"), Times.Once);
    }

    /// <summary>
    /// Tests that Handle returns success with job DTO when conversion to WebP is successful.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenConversionToWebpIsSuccessful_ReturnsSuccessWithJobDto()
    {
        // Arrange
        var userId = UserId.New();
        var command = new ConvertHtmlToImageCommand
        {
            HtmlContent = "<html><body>Hello World</body></html>",
            FileName = "test.html",
            TargetFormat = "webp",
        };
        var expectedImageBytes = new byte[] { 0x52, 0x49, 0x46, 0x46 }; // WebP RIFF header

        this.currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        this.converterFactoryMock
            .Setup(x => x.GetConverter("html", "webp"))
            .Returns(this.fileConverterMock.Object);

        this.fileConverterMock
            .Setup(x => x.ConvertAsync(It.IsAny<Stream>(), It.IsAny<ConversionOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(expectedImageBytes));

        this.jobRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<ConversionJob>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        this.unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await this.handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.TargetFormat.Should().Be("webp");
        result.Value.OutputFileName.Should().Be("test.webp");

        this.converterFactoryMock.Verify(x => x.GetConverter("html", "webp"), Times.Once);
    }

    /// <summary>
    /// Tests that Handle normalizes 'jpg' format to 'jpeg'.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenTargetFormatIsJpg_NormalizesToJpeg()
    {
        // Arrange
        var userId = UserId.New();
        var command = new ConvertHtmlToImageCommand
        {
            HtmlContent = "<html><body>Hello World</body></html>",
            FileName = "test.html",
            TargetFormat = "jpg",
        };
        var expectedImageBytes = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 };

        this.currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        this.converterFactoryMock
            .Setup(x => x.GetConverter("html", "jpeg"))
            .Returns(this.fileConverterMock.Object);

        this.fileConverterMock
            .Setup(x => x.ConvertAsync(It.IsAny<Stream>(), It.IsAny<ConversionOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(expectedImageBytes));

        this.jobRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<ConversionJob>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        this.unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await this.handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        this.converterFactoryMock.Verify(x => x.GetConverter("html", "jpeg"), Times.Once);
    }

    /// <summary>
    /// Tests that Handle returns unauthorized error when user is not authenticated.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenUserIsNotAuthenticated_ReturnsUnauthorizedError()
    {
        // Arrange
        var command = new ConvertHtmlToImageCommand
        {
            HtmlContent = "<html><body>Test</body></html>",
        };

        this.currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(default(UserId?));

        // Act
        var result = await this.handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.Unauthorized");

        this.converterFactoryMock.Verify(x => x.GetConverter(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        this.jobRepositoryMock.Verify(x => x.AddAsync(It.IsAny<ConversionJob>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Tests that Handle returns unsupported format error when format is not supported.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenFormatIsNotSupported_ReturnsUnsupportedFormatError()
    {
        // Arrange
        var userId = UserId.New();
        var command = new ConvertHtmlToImageCommand
        {
            HtmlContent = "<html><body>Test</body></html>",
            TargetFormat = "bmp",
        };

        this.currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        // Act
        var result = await this.handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Conversion.UnsupportedFormat");
        result.Error.Message.Should().Contain("bmp");

        this.converterFactoryMock.Verify(x => x.GetConverter(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    /// <summary>
    /// Tests that Handle returns unsupported conversion error when converter is not found.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenConverterNotFound_ReturnsUnsupportedConversionError()
    {
        // Arrange
        var userId = UserId.New();
        var command = new ConvertHtmlToImageCommand
        {
            HtmlContent = "<html><body>Test</body></html>",
            TargetFormat = "png",
        };

        this.currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        this.converterFactoryMock
            .Setup(x => x.GetConverter("html", "png"))
            .Returns(default(IFileConverter));

        // Act
        var result = await this.handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("ConversionJob.UnsupportedConversion");

        this.jobRepositoryMock.Verify(x => x.AddAsync(It.IsAny<ConversionJob>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Tests that Handle returns failure when conversion fails.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenConversionFails_ReturnsFailureWithError()
    {
        // Arrange
        var userId = UserId.New();
        var command = new ConvertHtmlToImageCommand
        {
            HtmlContent = "<html><body>Test</body></html>",
            FileName = "test.html",
            TargetFormat = "png",
        };
        var conversionError = new Error("Conversion.Failed", "Screenshot generation failed");

        this.currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        this.converterFactoryMock
            .Setup(x => x.GetConverter("html", "png"))
            .Returns(this.fileConverterMock.Object);

        this.fileConverterMock
            .Setup(x => x.ConvertAsync(It.IsAny<Stream>(), It.IsAny<ConversionOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<byte[]>(conversionError));

        this.jobRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<ConversionJob>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        this.unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await this.handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(conversionError);
    }

    /// <summary>
    /// Tests that Handle returns failure when converter throws exception.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenConverterThrowsException_ReturnsConversionFailedError()
    {
        // Arrange
        var userId = UserId.New();
        var command = new ConvertHtmlToImageCommand
        {
            HtmlContent = "<html><body>Test</body></html>",
            FileName = "test.html",
            TargetFormat = "png",
        };
        var exceptionMessage = "Unexpected error during screenshot";

        this.currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        this.converterFactoryMock
            .Setup(x => x.GetConverter("html", "png"))
            .Returns(this.fileConverterMock.Object);

        this.fileConverterMock
            .Setup(x => x.ConvertAsync(It.IsAny<Stream>(), It.IsAny<ConversionOptions>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException(exceptionMessage));

        this.jobRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<ConversionJob>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        this.unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await this.handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("ConversionJob.ConversionFailed");
        result.Error.Message.Should().Contain(exceptionMessage);
    }

    /// <summary>
    /// Tests that Handle uses URL content when HTML content is not provided.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenUrlIsProvided_UsesUrlForConversion()
    {
        // Arrange
        var userId = UserId.New();
        var command = new ConvertHtmlToImageCommand
        {
            Url = "https://example.com",
            FileName = "webpage.html",
            TargetFormat = "png",
        };
        var expectedImageBytes = new byte[] { 0x89, 0x50, 0x4E, 0x47 };

        this.currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        this.converterFactoryMock
            .Setup(x => x.GetConverter("html", "png"))
            .Returns(this.fileConverterMock.Object);

        this.fileConverterMock
            .Setup(x => x.ConvertAsync(It.IsAny<Stream>(), It.IsAny<ConversionOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(expectedImageBytes));

        this.jobRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<ConversionJob>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        this.unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await this.handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        this.fileConverterMock.Verify(
            x => x.ConvertAsync(It.IsAny<Stream>(), It.IsAny<ConversionOptions>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that Handle generates default file name when none is provided.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenFileNameNotProvided_GeneratesDefaultFileName()
    {
        // Arrange
        var userId = UserId.New();
        var command = new ConvertHtmlToImageCommand
        {
            HtmlContent = "<html><body>Test</body></html>",
            TargetFormat = "png",
        };
        var expectedImageBytes = new byte[] { 0x89, 0x50, 0x4E, 0x47 };
        ConversionJob? capturedJob = null;

        this.currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        this.converterFactoryMock
            .Setup(x => x.GetConverter("html", "png"))
            .Returns(this.fileConverterMock.Object);

        this.fileConverterMock
            .Setup(x => x.ConvertAsync(It.IsAny<Stream>(), It.IsAny<ConversionOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(expectedImageBytes));

        this.jobRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<ConversionJob>(), It.IsAny<CancellationToken>()))
            .Callback<ConversionJob, CancellationToken>((job, _) => capturedJob = job)
            .Returns(Task.CompletedTask);

        this.unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await this.handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        capturedJob.Should().NotBeNull();
        capturedJob!.InputFileName.Should().StartWith("screenshot_");
        capturedJob.InputFileName.Should().EndWith(".html");
    }

    /// <summary>
    /// Tests that Handle passes conversion options to converter.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenOptionsProvided_PassesOptionsToConverter()
    {
        // Arrange
        var userId = UserId.New();
        var options = new ConversionOptions
        {
            FullPage = false,
            ViewportWidth = 1280,
            ViewportHeight = 720,
        };
        var command = new ConvertHtmlToImageCommand
        {
            HtmlContent = "<html><body>Test</body></html>",
            FileName = "test.html",
            TargetFormat = "png",
            Options = options,
        };
        var expectedImageBytes = new byte[] { 0x89, 0x50, 0x4E, 0x47 };
        ConversionOptions? capturedOptions = null;

        this.currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        this.converterFactoryMock
            .Setup(x => x.GetConverter("html", "png"))
            .Returns(this.fileConverterMock.Object);

        this.fileConverterMock
            .Setup(x => x.ConvertAsync(It.IsAny<Stream>(), It.IsAny<ConversionOptions>(), It.IsAny<CancellationToken>()))
            .Callback<Stream, ConversionOptions, CancellationToken>((_, opts, _) => capturedOptions = opts)
            .ReturnsAsync(Result.Success(expectedImageBytes));

        this.jobRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<ConversionJob>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        this.unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await this.handler.Handle(command, CancellationToken.None);

        // Assert
        capturedOptions.Should().NotBeNull();
        capturedOptions!.FullPage.Should().BeFalse();
        capturedOptions.ViewportWidth.Should().Be(1280);
        capturedOptions.ViewportHeight.Should().Be(720);
    }

    /// <summary>
    /// Tests that Handle records metrics when conversion starts.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenConversionStarts_RecordsMetrics()
    {
        // Arrange
        var userId = UserId.New();
        var command = new ConvertHtmlToImageCommand
        {
            HtmlContent = "<html><body>Test</body></html>",
            TargetFormat = "png",
        };
        var expectedImageBytes = new byte[] { 0x89, 0x50, 0x4E, 0x47 };

        this.currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        this.converterFactoryMock
            .Setup(x => x.GetConverter("html", "png"))
            .Returns(this.fileConverterMock.Object);

        this.fileConverterMock
            .Setup(x => x.ConvertAsync(It.IsAny<Stream>(), It.IsAny<ConversionOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(expectedImageBytes));

        this.jobRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<ConversionJob>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        this.unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await this.handler.Handle(command, CancellationToken.None);

        // Assert
        this.metricsServiceMock.Verify(x => x.RecordConversionStarted("html", "png"), Times.Once);
        this.metricsServiceMock.Verify(x => x.RecordConversionCompleted("html", "png", It.IsAny<double>()), Times.Once);
    }

    /// <summary>
    /// Tests that Handle records failure metrics when conversion fails.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenConversionFails_RecordsFailureMetrics()
    {
        // Arrange
        var userId = UserId.New();
        var command = new ConvertHtmlToImageCommand
        {
            HtmlContent = "<html><body>Test</body></html>",
            TargetFormat = "png",
        };
        var conversionError = new Error("Conversion.Failed", "Screenshot failed");

        this.currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        this.converterFactoryMock
            .Setup(x => x.GetConverter("html", "png"))
            .Returns(this.fileConverterMock.Object);

        this.fileConverterMock
            .Setup(x => x.ConvertAsync(It.IsAny<Stream>(), It.IsAny<ConversionOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<byte[]>(conversionError));

        this.jobRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<ConversionJob>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        this.unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await this.handler.Handle(command, CancellationToken.None);

        // Assert
        this.metricsServiceMock.Verify(x => x.RecordConversionFailed("html", "png"), Times.Once);
    }

    /// <summary>
    /// Tests that Handle sends webhook notification when conversion completes.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenConversionCompletes_SendsWebhookNotification()
    {
        // Arrange
        var userId = UserId.New();
        var command = new ConvertHtmlToImageCommand
        {
            HtmlContent = "<html><body>Test</body></html>",
            TargetFormat = "png",
            WebhookUrl = "https://example.com/webhook",
        };
        var expectedImageBytes = new byte[] { 0x89, 0x50, 0x4E, 0x47 };

        this.currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        this.converterFactoryMock
            .Setup(x => x.GetConverter("html", "png"))
            .Returns(this.fileConverterMock.Object);

        this.fileConverterMock
            .Setup(x => x.ConvertAsync(It.IsAny<Stream>(), It.IsAny<ConversionOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(expectedImageBytes));

        this.jobRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<ConversionJob>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        this.unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await this.handler.Handle(command, CancellationToken.None);

        // Assert
        this.webhookServiceMock.Verify(
            x => x.SendJobCompletedAsync(It.IsAny<ConversionJob>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that constructor throws ArgumentNullException when jobRepository is null.
    /// </summary>
    [Fact]
    public void Constructor_WhenJobRepositoryIsNull_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new ConvertHtmlToImageCommandHandler(
            null!,
            this.unitOfWorkMock.Object,
            this.converterFactoryMock.Object,
            this.currentUserServiceMock.Object,
            this.webhookServiceMock.Object,
            this.metricsServiceMock.Object,
            this.cloudStorageServiceMock.Object,
            this.loggerMock.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("jobRepository");
    }

    /// <summary>
    /// Tests that constructor throws ArgumentNullException when unitOfWork is null.
    /// </summary>
    [Fact]
    public void Constructor_WhenUnitOfWorkIsNull_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new ConvertHtmlToImageCommandHandler(
            this.jobRepositoryMock.Object,
            null!,
            this.converterFactoryMock.Object,
            this.currentUserServiceMock.Object,
            this.webhookServiceMock.Object,
            this.metricsServiceMock.Object,
            this.cloudStorageServiceMock.Object,
            this.loggerMock.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("unitOfWork");
    }

    /// <summary>
    /// Tests that constructor throws ArgumentNullException when converterFactory is null.
    /// </summary>
    [Fact]
    public void Constructor_WhenConverterFactoryIsNull_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new ConvertHtmlToImageCommandHandler(
            this.jobRepositoryMock.Object,
            this.unitOfWorkMock.Object,
            null!,
            this.currentUserServiceMock.Object,
            this.webhookServiceMock.Object,
            this.metricsServiceMock.Object,
            this.cloudStorageServiceMock.Object,
            this.loggerMock.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("converterFactory");
    }

    /// <summary>
    /// Tests that constructor throws ArgumentNullException when currentUserService is null.
    /// </summary>
    [Fact]
    public void Constructor_WhenCurrentUserServiceIsNull_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new ConvertHtmlToImageCommandHandler(
            this.jobRepositoryMock.Object,
            this.unitOfWorkMock.Object,
            this.converterFactoryMock.Object,
            null!,
            this.webhookServiceMock.Object,
            this.metricsServiceMock.Object,
            this.cloudStorageServiceMock.Object,
            this.loggerMock.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("currentUserService");
    }

    /// <summary>
    /// Tests that constructor throws ArgumentNullException when webhookService is null.
    /// </summary>
    [Fact]
    public void Constructor_WhenWebhookServiceIsNull_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new ConvertHtmlToImageCommandHandler(
            this.jobRepositoryMock.Object,
            this.unitOfWorkMock.Object,
            this.converterFactoryMock.Object,
            this.currentUserServiceMock.Object,
            null!,
            this.metricsServiceMock.Object,
            this.cloudStorageServiceMock.Object,
            this.loggerMock.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("webhookService");
    }

    /// <summary>
    /// Tests that constructor throws ArgumentNullException when metricsService is null.
    /// </summary>
    [Fact]
    public void Constructor_WhenMetricsServiceIsNull_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new ConvertHtmlToImageCommandHandler(
            this.jobRepositoryMock.Object,
            this.unitOfWorkMock.Object,
            this.converterFactoryMock.Object,
            this.currentUserServiceMock.Object,
            this.webhookServiceMock.Object,
            null!,
            this.cloudStorageServiceMock.Object,
            this.loggerMock.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("metricsService");
    }

    /// <summary>
    /// Tests that constructor throws ArgumentNullException when cloudStorageService is null.
    /// </summary>
    [Fact]
    public void Constructor_WhenCloudStorageServiceIsNull_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new ConvertHtmlToImageCommandHandler(
            this.jobRepositoryMock.Object,
            this.unitOfWorkMock.Object,
            this.converterFactoryMock.Object,
            this.currentUserServiceMock.Object,
            this.webhookServiceMock.Object,
            this.metricsServiceMock.Object,
            null!,
            this.loggerMock.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("cloudStorageService");
    }

    /// <summary>
    /// Tests that constructor throws ArgumentNullException when logger is null.
    /// </summary>
    [Fact]
    public void Constructor_WhenLoggerIsNull_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new ConvertHtmlToImageCommandHandler(
            this.jobRepositoryMock.Object,
            this.unitOfWorkMock.Object,
            this.converterFactoryMock.Object,
            this.currentUserServiceMock.Object,
            this.webhookServiceMock.Object,
            this.metricsServiceMock.Object,
            this.cloudStorageServiceMock.Object,
            null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }
}
