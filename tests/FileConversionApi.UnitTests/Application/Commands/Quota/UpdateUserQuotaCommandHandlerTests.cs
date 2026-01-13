// <copyright file="UpdateUserQuotaCommandHandlerTests.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using System;
using System.Threading;
using System.Threading.Tasks;

using FileConversionApi.Application.Commands.Quota;
using FileConversionApi.Application.Interfaces;
using FileConversionApi.Domain.Entities;
using FileConversionApi.Domain.Primitives;
using FileConversionApi.Domain.ValueObjects;

using FluentAssertions;

using Moq;

using Xunit;

namespace FileConversionApi.UnitTests.Application.Commands.Quota;

/// <summary>
/// Unit tests for <see cref="UpdateUserQuotaCommandHandler"/>.
/// </summary>
public class UpdateUserQuotaCommandHandlerTests
{
    private readonly Mock<IUsageQuotaService> quotaServiceMock;
    private readonly Mock<IUserRepository> userRepositoryMock;
    private readonly UpdateUserQuotaCommandHandler handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateUserQuotaCommandHandlerTests"/> class.
    /// </summary>
    public UpdateUserQuotaCommandHandlerTests()
    {
        this.quotaServiceMock = new Mock<IUsageQuotaService>();
        this.userRepositoryMock = new Mock<IUserRepository>();
        this.handler = new UpdateUserQuotaCommandHandler(
            this.quotaServiceMock.Object,
            this.userRepositoryMock.Object);
    }

    /// <summary>
    /// Tests that Handle updates quota when user exists.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenUserExists_UpdatesQuotaAndReturnsDto()
    {
        // Arrange
        var user = User.Create("test@test.com", "hash");
        var userId = user.Id;
        var quota = UsageQuota.Create(userId, 2026, 1, 2000, 2147483648);

        this.userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        this.quotaServiceMock
            .Setup(x => x.UpdateQuotaLimitsAsync(
                It.IsAny<UserId>(),
                It.IsAny<int>(),
                It.IsAny<long>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UsageQuota>.Success(quota));

        var command = new UpdateUserQuotaCommand
        {
            UserId = userId.Value,
            ConversionsLimit = 2000,
            BytesLimit = 2147483648,
        };

        // Act
        var result = await this.handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ConversionsLimit.Should().Be(2000);
        result.Value.BytesLimit.Should().Be(2147483648);

        this.quotaServiceMock.Verify(
            x => x.UpdateQuotaLimitsAsync(
                userId,
                2000,
                2147483648,
                It.IsAny<CancellationToken>()),
            Times.Once);
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

        var command = new UpdateUserQuotaCommand
        {
            UserId = Guid.NewGuid(),
            ConversionsLimit = 2000,
            BytesLimit = 2147483648,
        };

        // Act
        var result = await this.handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Admin.UserNotFound");

        this.quotaServiceMock.Verify(
            x => x.UpdateQuotaLimitsAsync(
                It.IsAny<UserId>(),
                It.IsAny<int>(),
                It.IsAny<long>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    /// <summary>
    /// Tests that constructor throws ArgumentNullException when quota service is null.
    /// </summary>
    [Fact]
    public void Constructor_WhenQuotaServiceIsNull_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new UpdateUserQuotaCommandHandler(null!, this.userRepositoryMock.Object);

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
        var act = () => new UpdateUserQuotaCommandHandler(this.quotaServiceMock.Object, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("userRepository");
    }
}
