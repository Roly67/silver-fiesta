// <copyright file="SetUserAdminCommandHandlerTests.cs" company="FileConversionApi">
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
/// Unit tests for <see cref="SetUserAdminCommandHandler"/>.
/// </summary>
public class SetUserAdminCommandHandlerTests
{
    private readonly Mock<IUserRepository> userRepositoryMock;
    private readonly Mock<IUnitOfWork> unitOfWorkMock;
    private readonly Mock<ILogger<SetUserAdminCommandHandler>> loggerMock;
    private readonly SetUserAdminCommandHandler handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="SetUserAdminCommandHandlerTests"/> class.
    /// </summary>
    public SetUserAdminCommandHandlerTests()
    {
        this.userRepositoryMock = new Mock<IUserRepository>();
        this.unitOfWorkMock = new Mock<IUnitOfWork>();
        this.loggerMock = new Mock<ILogger<SetUserAdminCommandHandler>>();
        this.handler = new SetUserAdminCommandHandler(
            this.userRepositoryMock.Object,
            this.unitOfWorkMock.Object,
            this.loggerMock.Object);
    }

    /// <summary>
    /// Tests that Handle grants admin privileges successfully.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenGrantingAdmin_GrantsSuccessfully()
    {
        // Arrange
        var user = User.Create("test@test.com", "hash");

        this.userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        this.unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command = new SetUserAdminCommand { UserId = user.Id.Value, IsAdmin = true };

        // Act
        var result = await this.handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.IsAdmin.Should().BeTrue();
        this.userRepositoryMock.Verify(x => x.Update(user), Times.Once);
    }

    /// <summary>
    /// Tests that Handle revokes admin privileges successfully.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenRevokingAdmin_RevokesSuccessfully()
    {
        // Arrange
        var user = User.Create("test@test.com", "hash");
        user.GrantAdmin();

        this.userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        this.unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command = new SetUserAdminCommand { UserId = user.Id.Value, IsAdmin = false };

        // Act
        var result = await this.handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.IsAdmin.Should().BeFalse();
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

        var command = new SetUserAdminCommand { UserId = Guid.NewGuid(), IsAdmin = true };

        // Act
        var result = await this.handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Admin.UserNotFound");
    }

    /// <summary>
    /// Tests that Handle returns error when user already has admin privileges.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenUserAlreadyAdmin_ReturnsError()
    {
        // Arrange
        var user = User.Create("test@test.com", "hash");
        user.GrantAdmin();

        this.userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var command = new SetUserAdminCommand { UserId = user.Id.Value, IsAdmin = true };

        // Act
        var result = await this.handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Admin.UserAlreadyAdmin");
    }

    /// <summary>
    /// Tests that Handle returns error when user is not an admin.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenUserNotAdmin_ReturnsError()
    {
        // Arrange
        var user = User.Create("test@test.com", "hash");

        this.userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var command = new SetUserAdminCommand { UserId = user.Id.Value, IsAdmin = false };

        // Act
        var result = await this.handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Admin.UserNotAdmin");
    }
}
