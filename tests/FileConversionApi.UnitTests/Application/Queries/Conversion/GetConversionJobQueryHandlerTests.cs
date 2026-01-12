// <copyright file="GetConversionJobQueryHandlerTests.cs" company="FileConversionApi">
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
using FileConversionApi.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FileConversionApi.UnitTests.Application.Queries.Conversion;

/// <summary>
/// Unit tests for <see cref="GetConversionJobQueryHandler"/>.
/// </summary>
public class GetConversionJobQueryHandlerTests
{
    private readonly Mock<IConversionJobRepository> jobRepositoryMock;
    private readonly Mock<ICurrentUserService> currentUserServiceMock;
    private readonly Mock<ILogger<GetConversionJobQueryHandler>> loggerMock;
    private readonly GetConversionJobQueryHandler handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetConversionJobQueryHandlerTests"/> class.
    /// </summary>
    public GetConversionJobQueryHandlerTests()
    {
        this.jobRepositoryMock = new Mock<IConversionJobRepository>();
        this.currentUserServiceMock = new Mock<ICurrentUserService>();
        this.loggerMock = new Mock<ILogger<GetConversionJobQueryHandler>>();

        this.handler = new GetConversionJobQueryHandler(
            this.jobRepositoryMock.Object,
            this.currentUserServiceMock.Object,
            this.loggerMock.Object);
    }

    /// <summary>
    /// Tests that Handle returns success with job DTO when job is found.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenJobIsFound_ReturnsSuccessWithJobDto()
    {
        // Arrange
        var userId = UserId.New();
        var jobId = Guid.NewGuid();
        var query = new GetConversionJobQuery(jobId);
        var job = ConversionJob.Create(userId, "html", "pdf", "test.html");

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
        result.Value.SourceFormat.Should().Be("html");
        result.Value.TargetFormat.Should().Be("pdf");
        result.Value.InputFileName.Should().Be("test.html");
        result.Value.Status.Should().Be(ConversionStatus.Pending);

        this.jobRepositoryMock.Verify(
            x => x.GetByIdForUserAsync(
                It.Is<ConversionJobId>(id => id.Value == jobId),
                userId,
                It.IsAny<CancellationToken>()),
            Times.Once);
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
        var query = new GetConversionJobQuery(jobId);

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
    /// Tests that Handle returns unauthorized error when user is not authenticated.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenUserIsNotAuthenticated_ReturnsUnauthorizedError()
    {
        // Arrange
        var jobId = Guid.NewGuid();
        var query = new GetConversionJobQuery(jobId);

        this.currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(default(UserId?));

        // Act
        var result = await this.handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.Unauthorized");

        this.jobRepositoryMock.Verify(
            x => x.GetByIdForUserAsync(
                It.IsAny<ConversionJobId>(),
                It.IsAny<UserId>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    /// <summary>
    /// Tests that Handle returns job with completed status when job is completed.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenJobIsCompleted_ReturnsJobWithCompletedStatus()
    {
        // Arrange
        var userId = UserId.New();
        var jobId = Guid.NewGuid();
        var query = new GetConversionJobQuery(jobId);
        var job = ConversionJob.Create(userId, "html", "pdf", "test.html");
        job.MarkAsProcessing();
        job.MarkAsCompleted("test.pdf", new byte[] { 0x25, 0x50, 0x44, 0x46 });

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
        result.Value.Status.Should().Be(ConversionStatus.Completed);
        result.Value.OutputFileName.Should().Be("test.pdf");
        result.Value.CompletedAt.Should().NotBeNull();
    }

    /// <summary>
    /// Tests that Handle returns job with failed status and error message when job has failed.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenJobHasFailed_ReturnsJobWithFailedStatusAndErrorMessage()
    {
        // Arrange
        var userId = UserId.New();
        var jobId = Guid.NewGuid();
        var query = new GetConversionJobQuery(jobId);
        var job = ConversionJob.Create(userId, "html", "pdf", "test.html");
        var errorMessage = "Conversion failed due to invalid HTML";
        job.MarkAsProcessing();
        job.MarkAsFailed(errorMessage);

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
        result.Value.Status.Should().Be(ConversionStatus.Failed);
        result.Value.ErrorMessage.Should().Be(errorMessage);
        result.Value.CompletedAt.Should().NotBeNull();
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
        var query = new GetConversionJobQuery(jobId);
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
        var act = () => new GetConversionJobQueryHandler(
            null!,
            this.currentUserServiceMock.Object,
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
        var act = () => new GetConversionJobQueryHandler(
            this.jobRepositoryMock.Object,
            null!,
            this.loggerMock.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("currentUserService");
    }

    /// <summary>
    /// Tests that constructor throws ArgumentNullException when logger is null.
    /// </summary>
    [Fact]
    public void Constructor_WhenLoggerIsNull_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new GetConversionJobQueryHandler(
            this.jobRepositoryMock.Object,
            this.currentUserServiceMock.Object,
            null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }
}
