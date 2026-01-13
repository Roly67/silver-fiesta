// <copyright file="SetUserRateLimitTierCommandHandlerTests.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using System;
using System.Threading;
using System.Threading.Tasks;

using FileConversionApi.Application.Commands.RateLimit;
using FileConversionApi.Application.Interfaces;
using FileConversionApi.Domain.Entities;
using FileConversionApi.Domain.Enums;
using FileConversionApi.Domain.Primitives;
using FileConversionApi.Domain.ValueObjects;

using FluentAssertions;

using Moq;

using Xunit;

namespace FileConversionApi.UnitTests.Application.Commands.RateLimit;

/// <summary>
/// Unit tests for <see cref="SetUserRateLimitTierCommandHandler"/>.
/// </summary>
public class SetUserRateLimitTierCommandHandlerTests
{
    private readonly Mock<IUserRateLimitService> rateLimitServiceMock;
    private readonly Mock<IUserRepository> userRepositoryMock;
    private readonly SetUserRateLimitTierCommandHandler handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="SetUserRateLimitTierCommandHandlerTests"/> class.
    /// </summary>
    public SetUserRateLimitTierCommandHandlerTests()
    {
        this.rateLimitServiceMock = new Mock<IUserRateLimitService>();
        this.userRepositoryMock = new Mock<IUserRepository>();
        this.handler = new SetUserRateLimitTierCommandHandler(
            this.rateLimitServiceMock.Object,
            this.userRepositoryMock.Object);
    }

    /// <summary>
    /// Tests that Handle updates tier successfully.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_UpdatesTier_Successfully()
    {
        // Arrange
        var user = User.Create("test@test.com", "hash");

        this.userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        this.rateLimitServiceMock
            .Setup(x => x.UpdateTierAsync(It.IsAny<UserId>(), It.IsAny<RateLimitTier>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var command = new SetUserRateLimitTierCommand
        {
            UserId = user.Id.Value,
            Tier = RateLimitTier.Premium,
        };

        // Act
        var result = await this.handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        this.rateLimitServiceMock.Verify(
            x => x.UpdateTierAsync(It.Is<UserId>(id => id.Value == user.Id.Value), RateLimitTier.Premium, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that Handle returns error when user not found.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_ReturnsError_WhenUserNotFound()
    {
        // Arrange
        this.userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(null as User);

        var command = new SetUserRateLimitTierCommand
        {
            UserId = Guid.NewGuid(),
            Tier = RateLimitTier.Premium,
        };

        // Act
        var result = await this.handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Admin.UserNotFound");
    }
}
