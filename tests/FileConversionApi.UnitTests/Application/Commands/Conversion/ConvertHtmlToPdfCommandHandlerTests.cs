// <copyright file="ConvertHtmlToPdfCommandHandlerTests.cs" company="FileConversionApi">
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
/// Unit tests for <see cref="ConvertHtmlToPdfCommandHandler"/>.
/// </summary>
public class ConvertHtmlToPdfCommandHandlerTests
{
    private readonly Mock<IConversionJobRepository> jobRepositoryMock;
    private readonly Mock<IUnitOfWork> unitOfWorkMock;
    private readonly Mock<IConverterFactory> converterFactoryMock;
    private readonly Mock<ICurrentUserService> currentUserServiceMock;
    private readonly Mock<IWebhookService> webhookServiceMock;
    private readonly Mock<ILogger<ConvertHtmlToPdfCommandHandler>> loggerMock;
    private readonly Mock<IFileConverter> fileConverterMock;
    private readonly ConvertHtmlToPdfCommandHandler handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConvertHtmlToPdfCommandHandlerTests"/> class.
    /// </summary>
    public ConvertHtmlToPdfCommandHandlerTests()
    {
        this.jobRepositoryMock = new Mock<IConversionJobRepository>();
        this.unitOfWorkMock = new Mock<IUnitOfWork>();
        this.converterFactoryMock = new Mock<IConverterFactory>();
        this.currentUserServiceMock = new Mock<ICurrentUserService>();
        this.webhookServiceMock = new Mock<IWebhookService>();
        this.loggerMock = new Mock<ILogger<ConvertHtmlToPdfCommandHandler>>();
        this.fileConverterMock = new Mock<IFileConverter>();

        this.handler = new ConvertHtmlToPdfCommandHandler(
            this.jobRepositoryMock.Object,
            this.unitOfWorkMock.Object,
            this.converterFactoryMock.Object,
            this.currentUserServiceMock.Object,
            this.webhookServiceMock.Object,
            this.loggerMock.Object);
    }

    /// <summary>
    /// Tests that Handle returns success with job DTO when conversion is successful.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenConversionIsSuccessful_ReturnsSuccessWithJobDto()
    {
        // Arrange
        var userId = UserId.New();
        var command = new ConvertHtmlToPdfCommand
        {
            HtmlContent = "<html><body>Hello World</body></html>",
            FileName = "test.html",
        };
        var expectedPdfBytes = new byte[] { 0x25, 0x50, 0x44, 0x46 };

        this.currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        this.converterFactoryMock
            .Setup(x => x.GetConverter("html", "pdf"))
            .Returns(this.fileConverterMock.Object);

        this.fileConverterMock
            .Setup(x => x.ConvertAsync(It.IsAny<Stream>(), It.IsAny<ConversionOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(expectedPdfBytes));

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
        result.Value.TargetFormat.Should().Be("pdf");
        result.Value.Status.Should().Be(ConversionStatus.Completed);
        result.Value.InputFileName.Should().Be("test.html");
        result.Value.OutputFileName.Should().Be("test.pdf");

        this.converterFactoryMock.Verify(x => x.GetConverter("html", "pdf"), Times.Once);
        this.jobRepositoryMock.Verify(x => x.AddAsync(It.IsAny<ConversionJob>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that Handle returns unauthorized error when user is not authenticated.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenUserIsNotAuthenticated_ReturnsUnauthorizedError()
    {
        // Arrange
        var command = new ConvertHtmlToPdfCommand
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
    /// Tests that Handle returns unsupported conversion error when converter is not found.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenConverterNotFound_ReturnsUnsupportedConversionError()
    {
        // Arrange
        var userId = UserId.New();
        var command = new ConvertHtmlToPdfCommand
        {
            HtmlContent = "<html><body>Test</body></html>",
        };

        this.currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        this.converterFactoryMock
            .Setup(x => x.GetConverter("html", "pdf"))
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
        var command = new ConvertHtmlToPdfCommand
        {
            HtmlContent = "<html><body>Test</body></html>",
            FileName = "test.html",
        };
        var conversionError = new Error("Conversion.Failed", "PDF generation failed");

        this.currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        this.converterFactoryMock
            .Setup(x => x.GetConverter("html", "pdf"))
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
        var command = new ConvertHtmlToPdfCommand
        {
            HtmlContent = "<html><body>Test</body></html>",
            FileName = "test.html",
        };
        var exceptionMessage = "Unexpected error during conversion";

        this.currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        this.converterFactoryMock
            .Setup(x => x.GetConverter("html", "pdf"))
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
        var command = new ConvertHtmlToPdfCommand
        {
            Url = "https://example.com",
            FileName = "webpage.html",
        };
        var expectedPdfBytes = new byte[] { 0x25, 0x50, 0x44, 0x46 };

        this.currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        this.converterFactoryMock
            .Setup(x => x.GetConverter("html", "pdf"))
            .Returns(this.fileConverterMock.Object);

        this.fileConverterMock
            .Setup(x => x.ConvertAsync(It.IsAny<Stream>(), It.IsAny<ConversionOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(expectedPdfBytes));

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
        var command = new ConvertHtmlToPdfCommand
        {
            HtmlContent = "<html><body>Test</body></html>",
        };
        var expectedPdfBytes = new byte[] { 0x25, 0x50, 0x44, 0x46 };
        ConversionJob? capturedJob = null;

        this.currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        this.converterFactoryMock
            .Setup(x => x.GetConverter("html", "pdf"))
            .Returns(this.fileConverterMock.Object);

        this.fileConverterMock
            .Setup(x => x.ConvertAsync(It.IsAny<Stream>(), It.IsAny<ConversionOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(expectedPdfBytes));

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
        capturedJob!.InputFileName.Should().StartWith("document_");
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
            PageSize = "Letter",
            Landscape = true,
            MarginTop = 30,
        };
        var command = new ConvertHtmlToPdfCommand
        {
            HtmlContent = "<html><body>Test</body></html>",
            FileName = "test.html",
            Options = options,
        };
        var expectedPdfBytes = new byte[] { 0x25, 0x50, 0x44, 0x46 };
        ConversionOptions? capturedOptions = null;

        this.currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        this.converterFactoryMock
            .Setup(x => x.GetConverter("html", "pdf"))
            .Returns(this.fileConverterMock.Object);

        this.fileConverterMock
            .Setup(x => x.ConvertAsync(It.IsAny<Stream>(), It.IsAny<ConversionOptions>(), It.IsAny<CancellationToken>()))
            .Callback<Stream, ConversionOptions, CancellationToken>((_, opts, _) => capturedOptions = opts)
            .ReturnsAsync(Result.Success(expectedPdfBytes));

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
        capturedOptions!.PageSize.Should().Be("Letter");
        capturedOptions.Landscape.Should().BeTrue();
        capturedOptions.MarginTop.Should().Be(30);
    }

    /// <summary>
    /// Tests that constructor throws ArgumentNullException when jobRepository is null.
    /// </summary>
    [Fact]
    public void Constructor_WhenJobRepositoryIsNull_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new ConvertHtmlToPdfCommandHandler(
            null!,
            this.unitOfWorkMock.Object,
            this.converterFactoryMock.Object,
            this.currentUserServiceMock.Object,
            this.webhookServiceMock.Object,
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
        var act = () => new ConvertHtmlToPdfCommandHandler(
            this.jobRepositoryMock.Object,
            null!,
            this.converterFactoryMock.Object,
            this.currentUserServiceMock.Object,
            this.webhookServiceMock.Object,
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
        var act = () => new ConvertHtmlToPdfCommandHandler(
            this.jobRepositoryMock.Object,
            this.unitOfWorkMock.Object,
            null!,
            this.currentUserServiceMock.Object,
            this.webhookServiceMock.Object,
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
        var act = () => new ConvertHtmlToPdfCommandHandler(
            this.jobRepositoryMock.Object,
            this.unitOfWorkMock.Object,
            this.converterFactoryMock.Object,
            null!,
            this.webhookServiceMock.Object,
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
        var act = () => new ConvertHtmlToPdfCommandHandler(
            this.jobRepositoryMock.Object,
            this.unitOfWorkMock.Object,
            this.converterFactoryMock.Object,
            this.currentUserServiceMock.Object,
            null!,
            this.loggerMock.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("webhookService");
    }

    /// <summary>
    /// Tests that constructor throws ArgumentNullException when logger is null.
    /// </summary>
    [Fact]
    public void Constructor_WhenLoggerIsNull_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new ConvertHtmlToPdfCommandHandler(
            this.jobRepositoryMock.Object,
            this.unitOfWorkMock.Object,
            this.converterFactoryMock.Object,
            this.currentUserServiceMock.Object,
            this.webhookServiceMock.Object,
            null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }
}
