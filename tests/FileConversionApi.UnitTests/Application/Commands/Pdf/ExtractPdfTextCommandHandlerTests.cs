// <copyright file="ExtractPdfTextCommandHandlerTests.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using FileConversionApi.Application.Commands.Pdf;
using FileConversionApi.Application.Interfaces;
using FileConversionApi.Domain.Entities;
using FileConversionApi.Domain.Enums;
using FileConversionApi.Domain.Primitives;
using FileConversionApi.Domain.ValueObjects;

using FluentAssertions;

using Microsoft.Extensions.Logging;

using Moq;

using Xunit;

namespace FileConversionApi.UnitTests.Application.Commands.Pdf;

/// <summary>
/// Unit tests for <see cref="ExtractPdfTextCommandHandler"/>.
/// </summary>
public class ExtractPdfTextCommandHandlerTests
{
    private readonly Mock<IConversionJobRepository> jobRepositoryMock;
    private readonly Mock<IUnitOfWork> unitOfWorkMock;
    private readonly Mock<IPdfTextExtractor> pdfTextExtractorMock;
    private readonly Mock<ICurrentUserService> currentUserServiceMock;
    private readonly Mock<IWebhookService> webhookServiceMock;
    private readonly Mock<IMetricsService> metricsServiceMock;
    private readonly Mock<ICloudStorageService> cloudStorageServiceMock;
    private readonly Mock<ILogger<ExtractPdfTextCommandHandler>> loggerMock;
    private readonly ExtractPdfTextCommandHandler handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExtractPdfTextCommandHandlerTests"/> class.
    /// </summary>
    public ExtractPdfTextCommandHandlerTests()
    {
        this.jobRepositoryMock = new Mock<IConversionJobRepository>();
        this.unitOfWorkMock = new Mock<IUnitOfWork>();
        this.pdfTextExtractorMock = new Mock<IPdfTextExtractor>();
        this.currentUserServiceMock = new Mock<ICurrentUserService>();
        this.webhookServiceMock = new Mock<IWebhookService>();
        this.metricsServiceMock = new Mock<IMetricsService>();
        this.cloudStorageServiceMock = new Mock<ICloudStorageService>();
        this.loggerMock = new Mock<ILogger<ExtractPdfTextCommandHandler>>();

        this.handler = new ExtractPdfTextCommandHandler(
            this.jobRepositoryMock.Object,
            this.unitOfWorkMock.Object,
            this.pdfTextExtractorMock.Object,
            this.currentUserServiceMock.Object,
            this.webhookServiceMock.Object,
            this.metricsServiceMock.Object,
            this.cloudStorageServiceMock.Object,
            this.loggerMock.Object);
    }

    /// <summary>
    /// Tests that Handle returns success with job DTO when extraction is successful.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenExtractionIsSuccessful_ReturnsSuccessWithJobDto()
    {
        // Arrange
        var userId = UserId.New();
        var pdfBytes = new byte[] { 0x25, 0x50, 0x44, 0x46 }; // %PDF
        var command = new ExtractPdfTextCommand
        {
            PdfData = Convert.ToBase64String(pdfBytes),
            FileName = "test.pdf",
        };
        var extractedText = "Hello World\nThis is extracted text.";

        this.currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        this.pdfTextExtractorMock
            .Setup(x => x.ExtractTextAsync(It.IsAny<Stream>(), null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(extractedText));

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
        result.Value.SourceFormat.Should().Be("pdf");
        result.Value.TargetFormat.Should().Be("txt");
        result.Value.Status.Should().Be(ConversionStatus.Completed);
        result.Value.InputFileName.Should().Be("test.pdf");
        result.Value.OutputFileName.Should().Be("test.txt");

        this.pdfTextExtractorMock.Verify(
            x => x.ExtractTextAsync(It.IsAny<Stream>(), null, null, It.IsAny<CancellationToken>()),
            Times.Once);
        this.jobRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<ConversionJob>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that Handle extracts from specific page when PageNumber is provided.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenPageNumberProvided_ExtractsFromSpecificPage()
    {
        // Arrange
        var userId = UserId.New();
        var pdfBytes = new byte[] { 0x25, 0x50, 0x44, 0x46 };
        var command = new ExtractPdfTextCommand
        {
            PdfData = Convert.ToBase64String(pdfBytes),
            FileName = "test.pdf",
            PageNumber = 2,
        };
        var extractedText = "Page 2 content";

        this.currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        this.pdfTextExtractorMock
            .Setup(x => x.ExtractTextAsync(It.IsAny<Stream>(), 2, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(extractedText));

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
        this.pdfTextExtractorMock.Verify(
            x => x.ExtractTextAsync(It.IsAny<Stream>(), 2, null, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that Handle passes password to extractor when provided.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenPasswordProvided_PassesPasswordToExtractor()
    {
        // Arrange
        var userId = UserId.New();
        var pdfBytes = new byte[] { 0x25, 0x50, 0x44, 0x46 };
        var password = "secret123";
        var command = new ExtractPdfTextCommand
        {
            PdfData = Convert.ToBase64String(pdfBytes),
            FileName = "test.pdf",
            Password = password,
        };
        var extractedText = "Decrypted content";

        this.currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        this.pdfTextExtractorMock
            .Setup(x => x.ExtractTextAsync(It.IsAny<Stream>(), null, password, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(extractedText));

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
        this.pdfTextExtractorMock.Verify(
            x => x.ExtractTextAsync(It.IsAny<Stream>(), null, password, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that Handle returns unauthorized error when user is not authenticated.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenUserIsNotAuthenticated_ReturnsUnauthorizedError()
    {
        // Arrange
        var pdfBytes = new byte[] { 0x25, 0x50, 0x44, 0x46 };
        var command = new ExtractPdfTextCommand
        {
            PdfData = Convert.ToBase64String(pdfBytes),
        };

        this.currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(default(UserId?));

        // Act
        var result = await this.handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.Unauthorized");

        this.pdfTextExtractorMock.Verify(
            x => x.ExtractTextAsync(It.IsAny<Stream>(), It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    /// <summary>
    /// Tests that Handle returns invalid input error when PDF data is empty.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenPdfDataIsEmpty_ReturnsInvalidInputError()
    {
        // Arrange
        var userId = UserId.New();
        var command = new ExtractPdfTextCommand
        {
            PdfData = string.Empty,
        };

        this.currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        // Act
        var result = await this.handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Extraction.InvalidInput");
        result.Error.Message.Should().Contain("PDF data is required");
    }

    /// <summary>
    /// Tests that Handle returns invalid input error when PDF data is null.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenPdfDataIsNull_ReturnsInvalidInputError()
    {
        // Arrange
        var userId = UserId.New();
        var command = new ExtractPdfTextCommand
        {
            PdfData = null,
        };

        this.currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        // Act
        var result = await this.handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Extraction.InvalidInput");
    }

    /// <summary>
    /// Tests that Handle returns failure when extraction fails.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenExtractionFails_ReturnsFailureWithError()
    {
        // Arrange
        var userId = UserId.New();
        var pdfBytes = new byte[] { 0x25, 0x50, 0x44, 0x46 };
        var command = new ExtractPdfTextCommand
        {
            PdfData = Convert.ToBase64String(pdfBytes),
            FileName = "test.pdf",
        };
        var extractionError = new Error("ConversionJob.ConversionFailed", "PDF is password protected");

        this.currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        this.pdfTextExtractorMock
            .Setup(x => x.ExtractTextAsync(It.IsAny<Stream>(), null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<string>(extractionError));

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
        result.Error.Should().Be(extractionError);
    }

    /// <summary>
    /// Tests that Handle returns failure when base64 data is invalid.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenBase64DataIsInvalid_ReturnsInvalidInputError()
    {
        // Arrange
        var userId = UserId.New();
        var command = new ExtractPdfTextCommand
        {
            PdfData = "not-valid-base64!!!",
            FileName = "test.pdf",
        };

        this.currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

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
        result.Error.Code.Should().Be("Extraction.InvalidInput");
        result.Error.Message.Should().Contain("base64");
    }

    /// <summary>
    /// Tests that Handle returns failure when extractor throws exception.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenExtractorThrowsException_ReturnsExtractionFailedError()
    {
        // Arrange
        var userId = UserId.New();
        var pdfBytes = new byte[] { 0x25, 0x50, 0x44, 0x46 };
        var command = new ExtractPdfTextCommand
        {
            PdfData = Convert.ToBase64String(pdfBytes),
            FileName = "test.pdf",
        };
        var exceptionMessage = "Unexpected error during PDF extraction";

        this.currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        this.pdfTextExtractorMock
            .Setup(x => x.ExtractTextAsync(It.IsAny<Stream>(), null, null, It.IsAny<CancellationToken>()))
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
        result.Error.Code.Should().Be("Extraction.Failed");
        result.Error.Message.Should().Contain(exceptionMessage);
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
        var pdfBytes = new byte[] { 0x25, 0x50, 0x44, 0x46 };
        var command = new ExtractPdfTextCommand
        {
            PdfData = Convert.ToBase64String(pdfBytes),
        };
        var extractedText = "Extracted text";
        ConversionJob? capturedJob = null;

        this.currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        this.pdfTextExtractorMock
            .Setup(x => x.ExtractTextAsync(It.IsAny<Stream>(), null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(extractedText));

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
        capturedJob.InputFileName.Should().EndWith(".pdf");
    }

    /// <summary>
    /// Tests that Handle records metrics when extraction starts and completes.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenExtractionCompletes_RecordsMetrics()
    {
        // Arrange
        var userId = UserId.New();
        var pdfBytes = new byte[] { 0x25, 0x50, 0x44, 0x46 };
        var command = new ExtractPdfTextCommand
        {
            PdfData = Convert.ToBase64String(pdfBytes),
        };
        var extractedText = "Extracted text";

        this.currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        this.pdfTextExtractorMock
            .Setup(x => x.ExtractTextAsync(It.IsAny<Stream>(), null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(extractedText));

        this.jobRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<ConversionJob>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        this.unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await this.handler.Handle(command, CancellationToken.None);

        // Assert
        this.metricsServiceMock.Verify(x => x.RecordConversionStarted("pdf", "txt"), Times.Once);
        this.metricsServiceMock.Verify(x => x.RecordConversionCompleted("pdf", "txt", It.IsAny<double>()), Times.Once);
    }

    /// <summary>
    /// Tests that Handle records failure metrics when extraction fails.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenExtractionFails_RecordsFailureMetrics()
    {
        // Arrange
        var userId = UserId.New();
        var pdfBytes = new byte[] { 0x25, 0x50, 0x44, 0x46 };
        var command = new ExtractPdfTextCommand
        {
            PdfData = Convert.ToBase64String(pdfBytes),
        };
        var extractionError = new Error("ConversionJob.ConversionFailed", "Extraction failed");

        this.currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        this.pdfTextExtractorMock
            .Setup(x => x.ExtractTextAsync(It.IsAny<Stream>(), null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<string>(extractionError));

        this.jobRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<ConversionJob>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        this.unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await this.handler.Handle(command, CancellationToken.None);

        // Assert
        this.metricsServiceMock.Verify(x => x.RecordConversionFailed("pdf", "txt"), Times.Once);
    }

    /// <summary>
    /// Tests that Handle sends webhook notification when extraction completes.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenExtractionCompletes_SendsWebhookNotification()
    {
        // Arrange
        var userId = UserId.New();
        var pdfBytes = new byte[] { 0x25, 0x50, 0x44, 0x46 };
        var command = new ExtractPdfTextCommand
        {
            PdfData = Convert.ToBase64String(pdfBytes),
            WebhookUrl = "https://example.com/webhook",
        };
        var extractedText = "Extracted text";

        this.currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        this.pdfTextExtractorMock
            .Setup(x => x.ExtractTextAsync(It.IsAny<Stream>(), null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(extractedText));

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
        var act = () => new ExtractPdfTextCommandHandler(
            null!,
            this.unitOfWorkMock.Object,
            this.pdfTextExtractorMock.Object,
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
        var act = () => new ExtractPdfTextCommandHandler(
            this.jobRepositoryMock.Object,
            null!,
            this.pdfTextExtractorMock.Object,
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
    /// Tests that constructor throws ArgumentNullException when pdfTextExtractor is null.
    /// </summary>
    [Fact]
    public void Constructor_WhenPdfTextExtractorIsNull_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new ExtractPdfTextCommandHandler(
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
            .WithParameterName("pdfTextExtractor");
    }

    /// <summary>
    /// Tests that constructor throws ArgumentNullException when currentUserService is null.
    /// </summary>
    [Fact]
    public void Constructor_WhenCurrentUserServiceIsNull_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new ExtractPdfTextCommandHandler(
            this.jobRepositoryMock.Object,
            this.unitOfWorkMock.Object,
            this.pdfTextExtractorMock.Object,
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
        var act = () => new ExtractPdfTextCommandHandler(
            this.jobRepositoryMock.Object,
            this.unitOfWorkMock.Object,
            this.pdfTextExtractorMock.Object,
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
        var act = () => new ExtractPdfTextCommandHandler(
            this.jobRepositoryMock.Object,
            this.unitOfWorkMock.Object,
            this.pdfTextExtractorMock.Object,
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
        var act = () => new ExtractPdfTextCommandHandler(
            this.jobRepositoryMock.Object,
            this.unitOfWorkMock.Object,
            this.pdfTextExtractorMock.Object,
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
        var act = () => new ExtractPdfTextCommandHandler(
            this.jobRepositoryMock.Object,
            this.unitOfWorkMock.Object,
            this.pdfTextExtractorMock.Object,
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
