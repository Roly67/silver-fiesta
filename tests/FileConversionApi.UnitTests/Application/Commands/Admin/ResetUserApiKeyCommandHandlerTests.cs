// <copyright file="ResetUserApiKeyCommandHandlerTests.cs" company="FileConversionApi">
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
/// Unit tests for <see cref="ResetUserApiKeyCommandHandler"/>.
/// </summary>
public class ResetUserApiKeyCommandHandlerTests
{
    private readonly Mock<IUserRepository> userRepositoryMock;
    private readonly Mock<IUnitOfWork> unitOfWorkMock;
    private readonly Mock<ILogger<ResetUserApiKeyCommandHandler>> loggerMock;
    private readonly ResetUserApiKeyCommandHandler handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResetUserApiKeyCommandHandlerTests"/> class.
    /// </summary>
    public ResetUserApiKeyCommandHandlerTests()
    {
        this.userRepositoryMock = new Mock<IUserRepository>();
        this.unitOfWorkMock = new Mock<IUnitOfWork>();
        this.loggerMock = new Mock<ILogger<ResetUserApiKeyCommandHandler>>();
        this.handler = new ResetUserApiKeyCommandHandler(
            this.userRepositoryMock.Object,
            this.unitOfWorkMock.Object,
            this.loggerMock.Object);
    }

    /// <summary>
    /// Tests that Handle resets API key successfully.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenUserExists_ResetsApiKeySuccessfully()
    {
        // Arrange
        var user = User.Create("test@test.com", "hash");
        var oldApiKey = user.ApiKey;

        this.userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        this.unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command = new ResetUserApiKeyCommand { UserId = user.Id.Value };

        // Act
        var result = await this.handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBe(oldApiKey);
        result.Value.Should().StartWith("fca_");
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
            .ReturnsAsync(null as User);

        var command = new ResetUserApiKeyCommand { UserId = Guid.NewGuid() };

        // Act
        var result = await this.handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Admin.UserNotFound");
    }
}
