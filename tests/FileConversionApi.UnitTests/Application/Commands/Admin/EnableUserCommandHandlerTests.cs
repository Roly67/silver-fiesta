// <copyright file="EnableUserCommandHandlerTests.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using System;
using System.Threading;
using System.Threading.Tasks;

using FileConversionApi.Application.Commands.Admin;
using FileConversionApi.Application.Interfaces;
using FileConversionApi.Domain.Entities;
using FileConversionApi.Domain.ValueObjects;

using FluentAssertions;

using Microsoft.Extensions.Logging;

using Moq;

using Xunit;

namespace FileConversionApi.UnitTests.Application.Commands.Admin;

/// <summary>
/// Unit tests for <see cref="EnableUserCommandHandler"/>.
/// </summary>
public class EnableUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> userRepositoryMock;
    private readonly Mock<IUnitOfWork> unitOfWorkMock;
    private readonly Mock<ILogger<EnableUserCommandHandler>> loggerMock;
    private readonly EnableUserCommandHandler handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="EnableUserCommandHandlerTests"/> class.
    /// </summary>
    public EnableUserCommandHandlerTests()
    {
        this.userRepositoryMock = new Mock<IUserRepository>();
        this.unitOfWorkMock = new Mock<IUnitOfWork>();
        this.loggerMock = new Mock<ILogger<EnableUserCommandHandler>>();
        this.handler = new EnableUserCommandHandler(
            this.userRepositoryMock.Object,
            this.unitOfWorkMock.Object,
            this.loggerMock.Object);
    }

    /// <summary>
    /// Tests that Handle enables user successfully.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenUserIsDisabled_EnablesUserSuccessfully()
    {
        // Arrange
        var user = User.Create("test@test.com", "hash");
        user.Deactivate();

        this.userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        this.unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command = new EnableUserCommand { UserId = user.Id.Value };

        // Act
        var result = await this.handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.IsActive.Should().BeTrue();
        this.userRepositoryMock.Verify(x => x.Update(user), Times.Once);
    }

    /// <summary>
    /// Tests that Handle returns error when user not found.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenUserNotFound_ReturnsError()
    {
        // Arrange
        this.userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var command = new EnableUserCommand { UserId = Guid.NewGuid() };

        // Act
        var result = await this.handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Admin.UserNotFound");
    }

    /// <summary>
    /// Tests that Handle returns error when user is already enabled.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenUserAlreadyEnabled_ReturnsError()
    {
        // Arrange
        var user = User.Create("test@test.com", "hash");

        this.userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var command = new EnableUserCommand { UserId = user.Id.Value };

        // Act
        var result = await this.handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Admin.UserAlreadyEnabled");
    }
}
