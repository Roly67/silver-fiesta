// <copyright file="DownloadConversionResultQueryHandlerTests.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using System;
using System.Threading;
using System.Threading.Tasks;

using FileConversionApi.Application.Interfaces;
using FileConversionApi.Application.Queries.Conversion;
using FileConversionApi.Domain.Entities;
using FileConversionApi.Domain.Enums;
using FileConversionApi.Domain.Errors;
using FileConversionApi.Domain.Primitives;
using FileConversionApi.Domain.ValueObjects;

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FileConversionApi.UnitTests.Application.Queries.Conversion;

/// <summary>
/// Unit tests for <see cref="DownloadConversionResultQueryHandler"/>.
/// </summary>
public class DownloadConversionResultQueryHandlerTests
{
    private readonly Mock<IConversionJobRepository> jobRepositoryMock;
    private readonly Mock<ICurrentUserService> currentUserServiceMock;
    private readonly Mock<ICloudStorageService> cloudStorageServiceMock;
    private readonly Mock<ILogger<DownloadConversionResultQueryHandler>> loggerMock;
    private readonly DownloadConversionResultQueryHandler handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="DownloadConversionResultQueryHandlerTests"/> class.
    /// </summary>
    public DownloadConversionResultQueryHandlerTests()
    {
        this.jobRepositoryMock = new Mock<IConversionJobRepository>();
        this.currentUserServiceMock = new Mock<ICurrentUserService>();
        this.cloudStorageServiceMock = new Mock<ICloudStorageService>();
        this.loggerMock = new Mock<ILogger<DownloadConversionResultQueryHandler>>();

        this.handler = new DownloadConversionResultQueryHandler(
            this.jobRepositoryMock.Object,
            this.currentUserServiceMock.Object,
            this.cloudStorageServiceMock.Object,
            this.loggerMock.Object);
    }

    /// <summary>
    /// Tests that Handle returns success with file download result when job is completed.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenJobIsCompleted_ReturnsSuccessWithFileDownloadResult()
    {
        // Arrange
        var userId = UserId.New();
        var jobId = Guid.NewGuid();
        var query = new DownloadConversionResultQuery(jobId);
        var outputData = new byte[] { 0x25, 0x50, 0x44, 0x46 }; // PDF magic bytes
        var outputFileName = "output.pdf";

        var job = ConversionJob.Create(userId, "html", "pdf", "input.html");
        job.MarkAsProcessing();
        job.MarkAsCompleted(outputFileName, outputData);

        this.currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        this.jobRepositoryMock
            .Setup(x => x.GetByIdForUserAsync(
                It.Is<ConversionJobId>(id => id.Value == jobId),
                userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(job);

        // Act
        var result = await this.handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Content.Should().BeEquivalentTo(outputData);
        result.Value.FileName.Should().Be(outputFileName);
        result.Value.ContentType.Should().Be("application/pdf");
    }

    /// <summary>
    /// Tests that Handle returns unauthorized error when user is not authenticated.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenUserIsNotAuthenticated_ReturnsUnauthorizedError()
    {
        // Arrange
        var jobId = Guid.NewGuid();
        var query = new DownloadConversionResultQuery(jobId);

        this.currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(default(UserId?));

        // Act
        var result = await this.handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.Unauthorized");
        result.Error.Message.Should().Be("User is not authenticated.");

        this.jobRepositoryMock.Verify(
            x => x.GetByIdForUserAsync(
                It.IsAny<ConversionJobId>(),
                It.IsAny<UserId>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    /// <summary>
    /// Tests that Handle returns not found error when job does not exist.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenJobNotFound_ReturnsNotFoundError()
    {
        // Arrange
        var userId = UserId.New();
        var jobId = Guid.NewGuid();
        var query = new DownloadConversionResultQuery(jobId);

        this.currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        this.jobRepositoryMock
            .Setup(x => x.GetByIdForUserAsync(
                It.IsAny<ConversionJobId>(),
                It.IsAny<UserId>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(ConversionJob));

        // Act
        var result = await this.handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("ConversionJob.NotFound");
        result.Error.Message.Should().Contain(jobId.ToString());
    }

    /// <summary>
    /// Tests that Handle returns not completed error when job is pending.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenJobIsPending_ReturnsNotCompletedError()
    {
        // Arrange
        var userId = UserId.New();
        var jobId = Guid.NewGuid();
        var query = new DownloadConversionResultQuery(jobId);
        var job = ConversionJob.Create(userId, "html", "pdf", "input.html");

        this.currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        this.jobRepositoryMock
            .Setup(x => x.GetByIdForUserAsync(
                It.Is<ConversionJobId>(id => id.Value == jobId),
                userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(job);

        // Act
        var result = await this.handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ConversionJobErrors.NotCompleted);
    }

    /// <summary>
    /// Tests that Handle returns not completed error when job is processing.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenJobIsProcessing_ReturnsNotCompletedError()
    {
        // Arrange
        var userId = UserId.New();
        var jobId = Guid.NewGuid();
        var query = new DownloadConversionResultQuery(jobId);
        var job = ConversionJob.Create(userId, "html", "pdf", "input.html");
        job.MarkAsProcessing();

        this.currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        this.jobRepositoryMock
            .Setup(x => x.GetByIdForUserAsync(
                It.Is<ConversionJobId>(id => id.Value == jobId),
                userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(job);

        // Act
        var result = await this.handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ConversionJobErrors.NotCompleted);
    }

    /// <summary>
    /// Tests that Handle returns not completed error when job has failed.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenJobHasFailed_ReturnsNotCompletedError()
    {
        // Arrange
        var userId = UserId.New();
        var jobId = Guid.NewGuid();
        var query = new DownloadConversionResultQuery(jobId);
        var job = ConversionJob.Create(userId, "html", "pdf", "input.html");
        job.MarkAsProcessing();
        job.MarkAsFailed("Conversion error");

        this.currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        this.jobRepositoryMock
            .Setup(x => x.GetByIdForUserAsync(
                It.Is<ConversionJobId>(id => id.Value == jobId),
                userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(job);

        // Act
        var result = await this.handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ConversionJobErrors.NotCompleted);
    }

    /// <summary>
    /// Tests that Handle returns correct content type for PDF format.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenTargetFormatIsPdf_ReturnsCorrectContentType()
    {
        // Arrange
        var userId = UserId.New();
        var jobId = Guid.NewGuid();
        var query = new DownloadConversionResultQuery(jobId);
        var job = ConversionJob.Create(userId, "html", "pdf", "input.html");
        job.MarkAsProcessing();
        job.MarkAsCompleted("output.pdf", new byte[] { 0x25 });

        this.currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        this.jobRepositoryMock
            .Setup(x => x.GetByIdForUserAsync(It.IsAny<ConversionJobId>(), userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(job);

        // Act
        var result = await this.handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ContentType.Should().Be("application/pdf");
    }

    /// <summary>
    /// Tests that Handle returns correct content type for HTML format.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenTargetFormatIsHtml_ReturnsCorrectContentType()
    {
        // Arrange
        var userId = UserId.New();
        var jobId = Guid.NewGuid();
        var query = new DownloadConversionResultQuery(jobId);
        var job = ConversionJob.Create(userId, "pdf", "html", "input.pdf");
        job.MarkAsProcessing();
        job.MarkAsCompleted("output.html", new byte[] { 0x3C });

        this.currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        this.jobRepositoryMock
            .Setup(x => x.GetByIdForUserAsync(It.IsAny<ConversionJobId>(), userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(job);

        // Act
        var result = await this.handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ContentType.Should().Be("text/html");
    }

    /// <summary>
    /// Tests that Handle returns correct content type for DOCX format.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenTargetFormatIsDocx_ReturnsCorrectContentType()
    {
        // Arrange
        var userId = UserId.New();
        var jobId = Guid.NewGuid();
        var query = new DownloadConversionResultQuery(jobId);
        var job = ConversionJob.Create(userId, "html", "docx", "input.html");
        job.MarkAsProcessing();
        job.MarkAsCompleted("output.docx", new byte[] { 0x50 });

        this.currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        this.jobRepositoryMock
            .Setup(x => x.GetByIdForUserAsync(It.IsAny<ConversionJobId>(), userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(job);

        // Act
        var result = await this.handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ContentType.Should().Be("application/vnd.openxmlformats-officedocument.wordprocessingml.document");
    }

    /// <summary>
    /// Tests that Handle returns correct content type for XLSX format.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenTargetFormatIsXlsx_ReturnsCorrectContentType()
    {
        // Arrange
        var userId = UserId.New();
        var jobId = Guid.NewGuid();
        var query = new DownloadConversionResultQuery(jobId);
        var job = ConversionJob.Create(userId, "csv", "xlsx", "input.csv");
        job.MarkAsProcessing();
        job.MarkAsCompleted("output.xlsx", new byte[] { 0x50 });

        this.currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        this.jobRepositoryMock
            .Setup(x => x.GetByIdForUserAsync(It.IsAny<ConversionJobId>(), userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(job);

        // Act
        var result = await this.handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ContentType.Should().Be("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
    }

    /// <summary>
    /// Tests that Handle returns correct content type for PNG format.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenTargetFormatIsPng_ReturnsCorrectContentType()
    {
        // Arrange
        var userId = UserId.New();
        var jobId = Guid.NewGuid();
        var query = new DownloadConversionResultQuery(jobId);
        var job = ConversionJob.Create(userId, "html", "png", "input.html");
        job.MarkAsProcessing();
        job.MarkAsCompleted("output.png", new byte[] { 0x89 });

        this.currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        this.jobRepositoryMock
            .Setup(x => x.GetByIdForUserAsync(It.IsAny<ConversionJobId>(), userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(job);

        // Act
        var result = await this.handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ContentType.Should().Be("image/png");
    }

    /// <summary>
    /// Tests that Handle returns correct content type for JPG format.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenTargetFormatIsJpg_ReturnsCorrectContentType()
    {
        // Arrange
        var userId = UserId.New();
        var jobId = Guid.NewGuid();
        var query = new DownloadConversionResultQuery(jobId);
        var job = ConversionJob.Create(userId, "html", "jpg", "input.html");
        job.MarkAsProcessing();
        job.MarkAsCompleted("output.jpg", new byte[] { 0xFF });

        this.currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        this.jobRepositoryMock
            .Setup(x => x.GetByIdForUserAsync(It.IsAny<ConversionJobId>(), userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(job);

        // Act
        var result = await this.handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ContentType.Should().Be("image/jpeg");
    }

    /// <summary>
    /// Tests that Handle returns correct content type for JPEG format.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenTargetFormatIsJpeg_ReturnsCorrectContentType()
    {
        // Arrange
        var userId = UserId.New();
        var jobId = Guid.NewGuid();
        var query = new DownloadConversionResultQuery(jobId);
        var job = ConversionJob.Create(userId, "html", "jpeg", "input.html");
        job.MarkAsProcessing();
        job.MarkAsCompleted("output.jpeg", new byte[] { 0xFF });

        this.currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        this.jobRepositoryMock
            .Setup(x => x.GetByIdForUserAsync(It.IsAny<ConversionJobId>(), userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(job);

        // Act
        var result = await this.handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ContentType.Should().Be("image/jpeg");
    }

    /// <summary>
    /// Tests that Handle returns octet-stream for unknown formats.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenTargetFormatIsUnknown_ReturnsOctetStreamContentType()
    {
        // Arrange
        var userId = UserId.New();
        var jobId = Guid.NewGuid();
        var query = new DownloadConversionResultQuery(jobId);
        var job = ConversionJob.Create(userId, "html", "xyz", "input.html");
        job.MarkAsProcessing();
        job.MarkAsCompleted("output.xyz", new byte[] { 0x00 });

        this.currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        this.jobRepositoryMock
            .Setup(x => x.GetByIdForUserAsync(It.IsAny<ConversionJobId>(), userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(job);

        // Act
        var result = await this.handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ContentType.Should().Be("application/octet-stream");
    }

    /// <summary>
    /// Tests that Handle queries with correct job ID and user ID.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenCalled_QueriesWithCorrectJobIdAndUserId()
    {
        // Arrange
        var userId = UserId.New();
        var jobId = Guid.NewGuid();
        var query = new DownloadConversionResultQuery(jobId);
        ConversionJobId? capturedJobId = null;
        UserId? capturedUserId = null;

        this.currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        this.jobRepositoryMock
            .Setup(x => x.GetByIdForUserAsync(
                It.IsAny<ConversionJobId>(),
                It.IsAny<UserId>(),
                It.IsAny<CancellationToken>()))
            .Callback<ConversionJobId, UserId, CancellationToken>((jid, uid, _) =>
            {
                capturedJobId = jid;
                capturedUserId = uid;
            })
            .ReturnsAsync(default(ConversionJob));

        // Act
        await this.handler.Handle(query, CancellationToken.None);

        // Assert
        capturedJobId.Should().NotBeNull();
        capturedJobId!.Value.Value.Should().Be(jobId);
        capturedUserId.Should().NotBeNull();
        capturedUserId!.Value.Should().Be(userId);
    }

    /// <summary>
    /// Tests that constructor throws ArgumentNullException when jobRepository is null.
    /// </summary>
    [Fact]
    public void Constructor_WhenJobRepositoryIsNull_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new DownloadConversionResultQueryHandler(
            null!,
            this.currentUserServiceMock.Object,
            this.cloudStorageServiceMock.Object,
            this.loggerMock.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("jobRepository");
    }

    /// <summary>
    /// Tests that constructor throws ArgumentNullException when currentUserService is null.
    /// </summary>
    [Fact]
    public void Constructor_WhenCurrentUserServiceIsNull_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new DownloadConversionResultQueryHandler(
            this.jobRepositoryMock.Object,
            null!,
            this.cloudStorageServiceMock.Object,
            this.loggerMock.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("currentUserService");
    }

    /// <summary>
    /// Tests that constructor throws ArgumentNullException when cloudStorageService is null.
    /// </summary>
    [Fact]
    public void Constructor_WhenCloudStorageServiceIsNull_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new DownloadConversionResultQueryHandler(
            this.jobRepositoryMock.Object,
            this.currentUserServiceMock.Object,
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
        var act = () => new DownloadConversionResultQueryHandler(
            this.jobRepositoryMock.Object,
            this.currentUserServiceMock.Object,
            this.cloudStorageServiceMock.Object,
            null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    /// <summary>
    /// Tests that Handle downloads from cloud storage when job uses cloud storage.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenJobUsesCloudStorage_DownloadsFromCloudStorage()
    {
        // Arrange
        var userId = UserId.New();
        var jobId = Guid.NewGuid();
        var query = new DownloadConversionResultQuery(jobId);
        var cloudStorageKey = $"{userId.Value}/{jobId}/output.pdf";
        var outputData = new byte[] { 0x25, 0x50, 0x44, 0x46 }; // PDF magic bytes

        var job = ConversionJob.Create(userId, "html", "pdf", "input.html");
        job.MarkAsProcessing();
        job.MarkAsCompletedWithCloudStorage("output.pdf", cloudStorageKey);

        this.currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        this.jobRepositoryMock
            .Setup(x => x.GetByIdForUserAsync(
                It.Is<ConversionJobId>(id => id.Value == jobId),
                userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(job);

        this.cloudStorageServiceMock
            .Setup(x => x.DownloadAsync(cloudStorageKey, It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(Result.Success(outputData)));

        // Act
        var result = await this.handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Content.Should().BeEquivalentTo(outputData);
        result.Value.FileName.Should().Be("output.pdf");

        this.cloudStorageServiceMock.Verify(
            x => x.DownloadAsync(cloudStorageKey, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that Handle returns error when cloud storage download fails.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenCloudStorageDownloadFails_ReturnsError()
    {
        // Arrange
        var userId = UserId.New();
        var jobId = Guid.NewGuid();
        var query = new DownloadConversionResultQuery(jobId);
        var cloudStorageKey = $"{userId.Value}/{jobId}/output.pdf";

        var job = ConversionJob.Create(userId, "html", "pdf", "input.html");
        job.MarkAsProcessing();
        job.MarkAsCompletedWithCloudStorage("output.pdf", cloudStorageKey);

        this.currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        this.jobRepositoryMock
            .Setup(x => x.GetByIdForUserAsync(
                It.Is<ConversionJobId>(id => id.Value == jobId),
                userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(job);

        var downloadError = new Error("CloudStorage.DownloadFailed", "Failed to download file");
        this.cloudStorageServiceMock
            .Setup(x => x.DownloadAsync(cloudStorageKey, It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(Result.Failure<byte[]>(downloadError)));

        // Act
        var result = await this.handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(downloadError);
    }
}
