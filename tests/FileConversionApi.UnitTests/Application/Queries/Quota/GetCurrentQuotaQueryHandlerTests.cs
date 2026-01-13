// <copyright file="GetCurrentQuotaQueryHandlerTests.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using System;
using System.Threading;
using System.Threading.Tasks;

using FileConversionApi.Application.Interfaces;
using FileConversionApi.Application.Queries.Quota;
using FileConversionApi.Domain.Entities;
using FileConversionApi.Domain.Primitives;
using FileConversionApi.Domain.ValueObjects;

using FluentAssertions;

using Moq;

using Xunit;

namespace FileConversionApi.UnitTests.Application.Queries.Quota;

/// <summary>
/// Unit tests for <see cref="GetCurrentQuotaQueryHandler"/>.
/// </summary>
public class GetCurrentQuotaQueryHandlerTests
{
    private readonly Mock<IUsageQuotaService> quotaServiceMock;
    private readonly Mock<ICurrentUserService> currentUserServiceMock;
    private readonly GetCurrentQuotaQueryHandler handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetCurrentQuotaQueryHandlerTests"/> class.
    /// </summary>
    public GetCurrentQuotaQueryHandlerTests()
    {
        this.quotaServiceMock = new Mock<IUsageQuotaService>();
        this.currentUserServiceMock = new Mock<ICurrentUserService>();
        this.handler = new GetCurrentQuotaQueryHandler(
            this.quotaServiceMock.Object,
            this.currentUserServiceMock.Object);
    }

    /// <summary>
    /// Tests that Handle returns quota when user is authenticated.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenUserAuthenticated_ReturnsQuotaDto()
    {
        // Arrange
        var userId = new UserId(Guid.NewGuid());
        var quota = UsageQuota.Create(userId, 2026, 1, 1000, 1073741824);

        this.currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        this.quotaServiceMock
            .Setup(x => x.GetCurrentQuotaAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UsageQuota>.Success(quota));

        var query = new GetCurrentQuotaQuery();

        // Act
        var result = await this.handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Year.Should().Be(2026);
        result.Value.Month.Should().Be(1);
        result.Value.ConversionsLimit.Should().Be(1000);
    }

    /// <summary>
    /// Tests that Handle returns error when user is not authenticated.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ReturnsUnauthorizedError()
    {
        // Arrange
        this.currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns((UserId?)null);

        var query = new GetCurrentQuotaQuery();

        // Act
        var result = await this.handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Unauthorized");
    }

    /// <summary>
    /// Tests that constructor throws ArgumentNullException when quota service is null.
    /// </summary>
    [Fact]
    public void Constructor_WhenQuotaServiceIsNull_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new GetCurrentQuotaQueryHandler(null!, this.currentUserServiceMock.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("quotaService");
    }

    /// <summary>
    /// Tests that constructor throws ArgumentNullException when current user service is null.
    /// </summary>
    [Fact]
    public void Constructor_WhenCurrentUserServiceIsNull_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new GetCurrentQuotaQueryHandler(this.quotaServiceMock.Object, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("currentUserService");
    }
}
