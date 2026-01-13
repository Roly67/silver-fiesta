// <copyright file="GetUserQuotaQueryHandlerTests.cs" company="FileConversionApi">
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
/// Unit tests for <see cref="GetUserQuotaQueryHandler"/>.
/// </summary>
public class GetUserQuotaQueryHandlerTests
{
    private readonly Mock<IUsageQuotaService> quotaServiceMock;
    private readonly Mock<IUserRepository> userRepositoryMock;
    private readonly GetUserQuotaQueryHandler handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetUserQuotaQueryHandlerTests"/> class.
    /// </summary>
    public GetUserQuotaQueryHandlerTests()
    {
        this.quotaServiceMock = new Mock<IUsageQuotaService>();
        this.userRepositoryMock = new Mock<IUserRepository>();
        this.handler = new GetUserQuotaQueryHandler(
            this.quotaServiceMock.Object,
            this.userRepositoryMock.Object);
    }

    /// <summary>
    /// Tests that Handle returns quota when user exists.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenUserExists_ReturnsQuotaDto()
    {
        // Arrange
        var user = User.Create("test@test.com", "hash");
        var userId = user.Id;
        var quota = UsageQuota.Create(userId, 2026, 1, 1000, 1073741824);

        this.userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        this.quotaServiceMock
            .Setup(x => x.GetCurrentQuotaAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UsageQuota>.Success(quota));

        var query = new GetUserQuotaQuery { UserId = userId.Value };

        // Act
        var result = await this.handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Year.Should().Be(2026);
        result.Value.Month.Should().Be(1);
        result.Value.ConversionsLimit.Should().Be(1000);
    }

    /// <summary>
    /// Tests that Handle returns error when user not found.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenUserNotFound_ReturnsNotFoundError()
    {
        // Arrange
        this.userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(null as User);

        var query = new GetUserQuotaQuery { UserId = Guid.NewGuid() };

        // Act
        var result = await this.handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Admin.UserNotFound");
    }

    /// <summary>
    /// Tests that constructor throws ArgumentNullException when quota service is null.
    /// </summary>
    [Fact]
    public void Constructor_WhenQuotaServiceIsNull_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new GetUserQuotaQueryHandler(null!, this.userRepositoryMock.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("quotaService");
    }

    /// <summary>
    /// Tests that constructor throws ArgumentNullException when user repository is null.
    /// </summary>
    [Fact]
    public void Constructor_WhenUserRepositoryIsNull_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new GetUserQuotaQueryHandler(this.quotaServiceMock.Object, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("userRepository");
    }
}
