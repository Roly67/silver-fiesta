// <copyright file="GetJobStatisticsQueryHandlerTests.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using System;
using System.Threading;
using System.Threading.Tasks;

using FileConversionApi.Application.Interfaces;
using FileConversionApi.Application.Queries.Admin;

using FluentAssertions;

using Moq;

using Xunit;

namespace FileConversionApi.UnitTests.Application.Queries.Admin;

/// <summary>
/// Unit tests for <see cref="GetJobStatisticsQueryHandler"/>.
/// </summary>
public class GetJobStatisticsQueryHandlerTests
{
    private readonly Mock<IConversionJobRepository> jobRepositoryMock;
    private readonly Mock<IUserRepository> userRepositoryMock;
    private readonly GetJobStatisticsQueryHandler handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetJobStatisticsQueryHandlerTests"/> class.
    /// </summary>
    public GetJobStatisticsQueryHandlerTests()
    {
        this.jobRepositoryMock = new Mock<IConversionJobRepository>();
        this.userRepositoryMock = new Mock<IUserRepository>();
        this.handler = new GetJobStatisticsQueryHandler(
            this.jobRepositoryMock.Object,
            this.userRepositoryMock.Object);
    }

    /// <summary>
    /// Tests that Handle returns statistics successfully.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_ReturnsStatisticsSuccessfully()
    {
        // Arrange
        this.jobRepositoryMock
            .Setup(x => x.GetStatisticsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((100, 80, 15, 5));

        this.userRepositoryMock
            .Setup(x => x.GetTotalCountAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(10);

        var query = new GetJobStatisticsQuery();

        // Act
        var result = await this.handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.TotalJobs.Should().Be(100);
        result.Value.CompletedJobs.Should().Be(80);
        result.Value.FailedJobs.Should().Be(15);
        result.Value.PendingJobs.Should().Be(5);
        result.Value.TotalUsers.Should().Be(10);
        result.Value.SuccessRate.Should().Be(80);
    }

    /// <summary>
    /// Tests that Handle returns zero success rate when no jobs.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenNoJobs_ReturnsZeroSuccessRate()
    {
        // Arrange
        this.jobRepositoryMock
            .Setup(x => x.GetStatisticsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((0, 0, 0, 0));

        this.userRepositoryMock
            .Setup(x => x.GetTotalCountAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var query = new GetJobStatisticsQuery();

        // Act
        var result = await this.handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.SuccessRate.Should().Be(0);
    }

    /// <summary>
    /// Tests that constructor throws ArgumentNullException when job repository is null.
    /// </summary>
    [Fact]
    public void Constructor_WhenJobRepositoryIsNull_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new GetJobStatisticsQueryHandler(null!, this.userRepositoryMock.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("jobRepository");
    }

    /// <summary>
    /// Tests that constructor throws ArgumentNullException when user repository is null.
    /// </summary>
    [Fact]
    public void Constructor_WhenUserRepositoryIsNull_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new GetJobStatisticsQueryHandler(this.jobRepositoryMock.Object, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("userRepository");
    }
}
