// <copyright file="GetConversionHistoryQueryHandlerTests.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using FileConversionApi.Application.Interfaces;
using FileConversionApi.Application.Queries.Conversion;
using FileConversionApi.Domain.Entities;
using FileConversionApi.Domain.Enums;
using FileConversionApi.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FileConversionApi.UnitTests.Application.Queries.Conversion;

/// <summary>
/// Unit tests for <see cref="GetConversionHistoryQueryHandler"/>.
/// </summary>
public class GetConversionHistoryQueryHandlerTests
{
    private readonly Mock<IConversionJobRepository> jobRepositoryMock;
    private readonly Mock<ICurrentUserService> currentUserServiceMock;
    private readonly Mock<ILogger<GetConversionHistoryQueryHandler>> loggerMock;
    private readonly GetConversionHistoryQueryHandler handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetConversionHistoryQueryHandlerTests"/> class.
    /// </summary>
    public GetConversionHistoryQueryHandlerTests()
    {
        this.jobRepositoryMock = new Mock<IConversionJobRepository>();
        this.currentUserServiceMock = new Mock<ICurrentUserService>();
        this.loggerMock = new Mock<ILogger<GetConversionHistoryQueryHandler>>();

        this.handler = new GetConversionHistoryQueryHandler(
            this.jobRepositoryMock.Object,
            this.currentUserServiceMock.Object,
            this.loggerMock.Object);
    }

    /// <summary>
    /// Tests that Handle returns success with paged result when jobs are found.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenJobsExist_ReturnsSuccessWithPagedResult()
    {
        // Arrange
        var userId = UserId.New();
        var query = new GetConversionHistoryQuery(1, 10);
        var jobs = new List<ConversionJob>
        {
            ConversionJob.Create(userId, "html", "pdf", "test1.html"),
            ConversionJob.Create(userId, "html", "pdf", "test2.html"),
            ConversionJob.Create(userId, "html", "pdf", "test3.html"),
        };

        this.currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        this.jobRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(jobs);

        this.jobRepositoryMock
            .Setup(x => x.GetCountByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(3);

        // Act
        var result = await this.handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Items.Should().HaveCount(3);
        result.Value.Page.Should().Be(1);
        result.Value.PageSize.Should().Be(10);
        result.Value.TotalCount.Should().Be(3);
        result.Value.TotalPages.Should().Be(1);
        result.Value.HasNextPage.Should().BeFalse();
        result.Value.HasPreviousPage.Should().BeFalse();
    }

    /// <summary>
    /// Tests that Handle returns empty result when no jobs exist.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenNoJobsExist_ReturnsEmptyPagedResult()
    {
        // Arrange
        var userId = UserId.New();
        var query = new GetConversionHistoryQuery(1, 10);

        this.currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        this.jobRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ConversionJob>());

        this.jobRepositoryMock
            .Setup(x => x.GetCountByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        // Act
        var result = await this.handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
        result.Value.TotalPages.Should().Be(0);
    }

    /// <summary>
    /// Tests that Handle returns unauthorized error when user is not authenticated.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenUserIsNotAuthenticated_ReturnsUnauthorizedError()
    {
        // Arrange
        var query = new GetConversionHistoryQuery(1, 10);

        this.currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns((UserId?)null);

        // Act
        var result = await this.handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.Unauthorized");

        this.jobRepositoryMock.Verify(
            x => x.GetByUserIdAsync(It.IsAny<UserId>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    /// <summary>
    /// Tests that Handle returns correct pagination information for multiple pages.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenMultiplePagesExist_ReturnsCorrectPaginationInfo()
    {
        // Arrange
        var userId = UserId.New();
        var query = new GetConversionHistoryQuery(2, 5);
        var jobs = new List<ConversionJob>
        {
            ConversionJob.Create(userId, "html", "pdf", "test6.html"),
            ConversionJob.Create(userId, "html", "pdf", "test7.html"),
            ConversionJob.Create(userId, "html", "pdf", "test8.html"),
            ConversionJob.Create(userId, "html", "pdf", "test9.html"),
            ConversionJob.Create(userId, "html", "pdf", "test10.html"),
        };

        this.currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        this.jobRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, 2, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(jobs);

        this.jobRepositoryMock
            .Setup(x => x.GetCountByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(25);

        // Act
        var result = await this.handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Page.Should().Be(2);
        result.Value.PageSize.Should().Be(5);
        result.Value.TotalCount.Should().Be(25);
        result.Value.TotalPages.Should().Be(5);
        result.Value.HasNextPage.Should().BeTrue();
        result.Value.HasPreviousPage.Should().BeTrue();
    }

    /// <summary>
    /// Tests that Handle uses default pagination values when not specified.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenDefaultPagination_UsesDefaultValues()
    {
        // Arrange
        var userId = UserId.New();
        var query = new GetConversionHistoryQuery();
        int? capturedPage = null;
        int? capturedPageSize = null;

        this.currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        this.jobRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Callback<UserId, int, int, CancellationToken>((_, page, pageSize, _) =>
            {
                capturedPage = page;
                capturedPageSize = pageSize;
            })
            .ReturnsAsync(new List<ConversionJob>());

        this.jobRepositoryMock
            .Setup(x => x.GetCountByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        // Act
        await this.handler.Handle(query, CancellationToken.None);

        // Assert
        capturedPage.Should().Be(1);
        capturedPageSize.Should().Be(20);
    }

    /// <summary>
    /// Tests that Handle maps job entities to DTOs correctly.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenJobsExist_MapsJobsToDtosCorrectly()
    {
        // Arrange
        var userId = UserId.New();
        var query = new GetConversionHistoryQuery(1, 10);
        var job = ConversionJob.Create(userId, "html", "pdf", "document.html");
        job.MarkAsProcessing();
        job.MarkAsCompleted("document.pdf", new byte[] { 0x25, 0x50, 0x44, 0x46 });

        var jobs = new List<ConversionJob> { job };

        this.currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        this.jobRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(jobs);

        this.jobRepositoryMock
            .Setup(x => x.GetCountByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await this.handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(1);

        var dto = result.Value.Items[0];
        dto.Id.Should().Be(job.Id.Value);
        dto.SourceFormat.Should().Be("html");
        dto.TargetFormat.Should().Be("pdf");
        dto.InputFileName.Should().Be("document.html");
        dto.OutputFileName.Should().Be("document.pdf");
        dto.Status.Should().Be(ConversionStatus.Completed);
        dto.CompletedAt.Should().NotBeNull();
    }

    /// <summary>
    /// Tests that Handle returns last page correctly.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenOnLastPage_HasNextPageIsFalse()
    {
        // Arrange
        var userId = UserId.New();
        var query = new GetConversionHistoryQuery(3, 10);
        var jobs = new List<ConversionJob>
        {
            ConversionJob.Create(userId, "html", "pdf", "test.html"),
        };

        this.currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        this.jobRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, 3, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(jobs);

        this.jobRepositoryMock
            .Setup(x => x.GetCountByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(21);

        // Act
        var result = await this.handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Page.Should().Be(3);
        result.Value.TotalPages.Should().Be(3);
        result.Value.HasNextPage.Should().BeFalse();
        result.Value.HasPreviousPage.Should().BeTrue();
    }

    /// <summary>
    /// Tests that Handle queries with correct user ID.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenCalled_QueriesWithCorrectUserId()
    {
        // Arrange
        var userId = UserId.New();
        var query = new GetConversionHistoryQuery(1, 10);
        UserId? capturedUserId = null;

        this.currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        this.jobRepositoryMock
            .Setup(x => x.GetByUserIdAsync(It.IsAny<UserId>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Callback<UserId, int, int, CancellationToken>((uid, _, _, _) => capturedUserId = uid)
            .ReturnsAsync(new List<ConversionJob>());

        this.jobRepositoryMock
            .Setup(x => x.GetCountByUserIdAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        // Act
        await this.handler.Handle(query, CancellationToken.None);

        // Assert
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
        var act = () => new GetConversionHistoryQueryHandler(
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
        var act = () => new GetConversionHistoryQueryHandler(
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
        var act = () => new GetConversionHistoryQueryHandler(
            this.jobRepositoryMock.Object,
            this.currentUserServiceMock.Object,
            null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }
}
