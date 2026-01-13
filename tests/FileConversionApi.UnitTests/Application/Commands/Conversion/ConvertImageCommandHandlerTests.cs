// <copyright file="ConvertImageCommandHandlerTests.cs" company="FileConversionApi">
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
/// Unit tests for <see cref="ConvertImageCommandHandler"/>.
/// </summary>
public class ConvertImageCommandHandlerTests
{
    private readonly Mock<IConversionJobRepository> jobRepositoryMock;
    private readonly Mock<IUnitOfWork> unitOfWorkMock;
    private readonly Mock<IConverterFactory> converterFactoryMock;
    private readonly Mock<ICurrentUserService> currentUserServiceMock;
    private readonly Mock<IWebhookService> webhookServiceMock;
    private readonly Mock<IMetricsService> metricsServiceMock;
    private readonly Mock<ICloudStorageService> cloudStorageServiceMock;
    private readonly Mock<ILogger<ConvertImageCommandHandler>> loggerMock;
    private readonly Mock<IFileConverter> fileConverterMock;
    private readonly ConvertImageCommandHandler handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConvertImageCommandHandlerTests"/> class.
    /// </summary>
    public ConvertImageCommandHandlerTests()
    {
        this.jobRepositoryMock = new Mock<IConversionJobRepository>();
        this.unitOfWorkMock = new Mock<IUnitOfWork>();
        this.converterFactoryMock = new Mock<IConverterFactory>();
        this.currentUserServiceMock = new Mock<ICurrentUserService>();
        this.webhookServiceMock = new Mock<IWebhookService>();
        this.metricsServiceMock = new Mock<IMetricsService>();
        this.cloudStorageServiceMock = new Mock<ICloudStorageService>();
        this.loggerMock = new Mock<ILogger<ConvertImageCommandHandler>>();
        this.fileConverterMock = new Mock<IFileConverter>();

        this.handler = new ConvertImageCommandHandler(
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
    /// Tests that Handle returns success when conversion is successful.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenConversionIsSuccessful_ReturnsSuccessWithJobDto()
    {
        // Arrange
        var userId = UserId.New();
        var imageData = Convert.ToBase64String(new byte[] { 0x89, 0x50, 0x4E, 0x47 }); // Fake PNG header
        var command = new ConvertImageCommand
        {
            ImageData = imageData,
            SourceFormat = "png",
            TargetFormat = "jpeg",
            FileName = "test.png",
        };
        var expectedOutputBytes = new byte[] { 0xFF, 0xD8, 0xFF }; // JPEG header

        this.currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        this.converterFactoryMock
            .Setup(x => x.GetConverter("png", "jpeg"))
            .Returns(this.fileConverterMock.Object);

        this.fileConverterMock
            .Setup(x => x.ConvertAsync(It.IsAny<Stream>(), It.IsAny<ConversionOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(expectedOutputBytes));

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
        result.Value.SourceFormat.Should().Be("png");
        result.Value.TargetFormat.Should().Be("jpeg");
        result.Value.Status.Should().Be(ConversionStatus.Completed);

        this.metricsServiceMock.Verify(x => x.RecordConversionStarted("png", "jpeg"), Times.Once);
        this.metricsServiceMock.Verify(x => x.RecordConversionCompleted("png", "jpeg", It.IsAny<double>()), Times.Once);
    }

    /// <summary>
    /// Tests that Handle returns unauthorized error when user is not authenticated.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenUserIsNotAuthenticated_ReturnsUnauthorizedError()
    {
        // Arrange
        var command = new ConvertImageCommand
        {
            ImageData = "dGVzdA==",
            SourceFormat = "png",
            TargetFormat = "jpeg",
        };

        this.currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(default(UserId?));

        // Act
        var result = await this.handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.Unauthorized");
    }

    /// <summary>
    /// Tests that Handle returns error for invalid source format.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenSourceFormatIsInvalid_ReturnsError()
    {
        // Arrange
        var userId = UserId.New();
        var command = new ConvertImageCommand
        {
            ImageData = "dGVzdA==",
            SourceFormat = "invalid",
            TargetFormat = "jpeg",
        };

        this.currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        // Act
        var result = await this.handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Conversion.InvalidSourceFormat");
    }

    /// <summary>
    /// Tests that Handle returns error for invalid target format.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenTargetFormatIsInvalid_ReturnsError()
    {
        // Arrange
        var userId = UserId.New();
        var command = new ConvertImageCommand
        {
            ImageData = "dGVzdA==",
            SourceFormat = "png",
            TargetFormat = "invalid",
        };

        this.currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        // Act
        var result = await this.handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Conversion.InvalidTargetFormat");
    }

    /// <summary>
    /// Tests that Handle returns error when source and target formats are the same.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenSourceAndTargetAreSame_ReturnsError()
    {
        // Arrange
        var userId = UserId.New();
        var command = new ConvertImageCommand
        {
            ImageData = "dGVzdA==",
            SourceFormat = "png",
            TargetFormat = "png",
        };

        this.currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        // Act
        var result = await this.handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Conversion.SameFormat");
    }

    /// <summary>
    /// Tests that Handle returns error for invalid base64 data.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenImageDataIsInvalidBase64_ReturnsError()
    {
        // Arrange
        var userId = UserId.New();
        var command = new ConvertImageCommand
        {
            ImageData = "not-valid-base64!!!",
            SourceFormat = "png",
            TargetFormat = "jpeg",
        };

        this.currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        this.converterFactoryMock
            .Setup(x => x.GetConverter("png", "jpeg"))
            .Returns(this.fileConverterMock.Object);

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
        result.Error.Code.Should().Be("Conversion.InvalidBase64");
        this.metricsServiceMock.Verify(x => x.RecordConversionFailed("png", "jpeg"), Times.Once);
    }

    /// <summary>
    /// Tests that Handle normalizes jpg to jpeg.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenJpgFormatUsed_NormalizesToJpeg()
    {
        // Arrange
        var userId = UserId.New();
        var imageData = Convert.ToBase64String(new byte[] { 0x89, 0x50, 0x4E, 0x47 });
        var command = new ConvertImageCommand
        {
            ImageData = imageData,
            SourceFormat = "png",
            TargetFormat = "jpg",
            FileName = "test.png",
        };
        var expectedOutputBytes = new byte[] { 0xFF, 0xD8, 0xFF };

        this.currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        this.converterFactoryMock
            .Setup(x => x.GetConverter("png", "jpeg"))
            .Returns(this.fileConverterMock.Object);

        this.fileConverterMock
            .Setup(x => x.ConvertAsync(It.IsAny<Stream>(), It.IsAny<ConversionOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(expectedOutputBytes));

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
        this.converterFactoryMock.Verify(x => x.GetConverter("png", "jpeg"), Times.Once);
    }

    /// <summary>
    /// Tests that constructor throws ArgumentNullException when jobRepository is null.
    /// </summary>
    [Fact]
    public void Constructor_WhenJobRepositoryIsNull_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new ConvertImageCommandHandler(
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
    /// Tests that constructor throws ArgumentNullException when metricsService is null.
    /// </summary>
    [Fact]
    public void Constructor_WhenMetricsServiceIsNull_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new ConvertImageCommandHandler(
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
        var act = () => new ConvertImageCommandHandler(
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
        var act = () => new ConvertImageCommandHandler(
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
