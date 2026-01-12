// <copyright file="LoginCommandHandlerTests.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using System;
using System.Threading;
using System.Threading.Tasks;

using FileConversionApi.Application.Commands.Auth;
using FileConversionApi.Application.DTOs;
using FileConversionApi.Application.Interfaces;
using FileConversionApi.Domain.Entities;
using FileConversionApi.Domain.Errors;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FileConversionApi.UnitTests.Application.Commands.Auth;

/// <summary>
/// Unit tests for <see cref="LoginCommandHandler"/>.
/// </summary>
public class LoginCommandHandlerTests
{
    private readonly Mock<IUserRepository> userRepositoryMock;
    private readonly Mock<IPasswordHasher> passwordHasherMock;
    private readonly Mock<IJwtTokenGenerator> jwtTokenGeneratorMock;
    private readonly Mock<ILogger<LoginCommandHandler>> loggerMock;
    private readonly LoginCommandHandler handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoginCommandHandlerTests"/> class.
    /// </summary>
    public LoginCommandHandlerTests()
    {
        this.userRepositoryMock = new Mock<IUserRepository>();
        this.passwordHasherMock = new Mock<IPasswordHasher>();
        this.jwtTokenGeneratorMock = new Mock<IJwtTokenGenerator>();
        this.loggerMock = new Mock<ILogger<LoginCommandHandler>>();

        this.handler = new LoginCommandHandler(
            this.userRepositoryMock.Object,
            this.passwordHasherMock.Object,
            this.jwtTokenGeneratorMock.Object,
            this.loggerMock.Object);
    }

    /// <summary>
    /// Tests that Handle returns success with token response when credentials are valid.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenCredentialsAreValid_ReturnsSuccessWithTokenResponse()
    {
        // Arrange
        var command = new LoginCommand("test@example.com", "Password123!");
        var user = User.Create("test@example.com", "hashed_password");
        var expectedTokenResponse = new TokenResponse
        {
            AccessToken = "access_token",
            RefreshToken = "refresh_token",
            ExpiresIn = 3600,
        };

        this.userRepositoryMock
            .Setup(x => x.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        this.passwordHasherMock
            .Setup(x => x.Verify(command.Password, user.PasswordHash))
            .Returns(true);

        this.jwtTokenGeneratorMock
            .Setup(x => x.GenerateToken(user))
            .Returns(expectedTokenResponse);

        // Act
        var result = await this.handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(expectedTokenResponse);

        this.userRepositoryMock.Verify(
            x => x.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()),
            Times.Once);
        this.passwordHasherMock.Verify(x => x.Verify(command.Password, user.PasswordHash), Times.Once);
        this.jwtTokenGeneratorMock.Verify(x => x.GenerateToken(user), Times.Once);
    }

    /// <summary>
    /// Tests that Handle returns failure when user is not found.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenUserNotFound_ReturnsFailureWithInvalidCredentialsError()
    {
        // Arrange
        var command = new LoginCommand("nonexistent@example.com", "Password123!");

        this.userRepositoryMock
            .Setup(x => x.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(User));

        // Act
        var result = await this.handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.InvalidCredentials);

        this.userRepositoryMock.Verify(
            x => x.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()),
            Times.Once);
        this.passwordHasherMock.Verify(x => x.Verify(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        this.jwtTokenGeneratorMock.Verify(x => x.GenerateToken(It.IsAny<User>()), Times.Never);
    }

    /// <summary>
    /// Tests that Handle returns failure when user is inactive.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenUserIsInactive_ReturnsFailureWithInactiveError()
    {
        // Arrange
        var command = new LoginCommand("inactive@example.com", "Password123!");
        var user = User.Create("inactive@example.com", "hashed_password");
        user.Deactivate();

        this.userRepositoryMock
            .Setup(x => x.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await this.handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.Inactive);

        this.userRepositoryMock.Verify(
            x => x.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()),
            Times.Once);
        this.passwordHasherMock.Verify(x => x.Verify(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        this.jwtTokenGeneratorMock.Verify(x => x.GenerateToken(It.IsAny<User>()), Times.Never);
    }

    /// <summary>
    /// Tests that Handle returns failure when password is invalid.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenPasswordIsInvalid_ReturnsFailureWithInvalidCredentialsError()
    {
        // Arrange
        var command = new LoginCommand("test@example.com", "WrongPassword!");
        var user = User.Create("test@example.com", "hashed_password");

        this.userRepositoryMock
            .Setup(x => x.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        this.passwordHasherMock
            .Setup(x => x.Verify(command.Password, user.PasswordHash))
            .Returns(false);

        // Act
        var result = await this.handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.InvalidCredentials);

        this.userRepositoryMock.Verify(
            x => x.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()),
            Times.Once);
        this.passwordHasherMock.Verify(x => x.Verify(command.Password, user.PasswordHash), Times.Once);
        this.jwtTokenGeneratorMock.Verify(x => x.GenerateToken(It.IsAny<User>()), Times.Never);
    }

    /// <summary>
    /// Tests that Handle verifies password with correct hash.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenUserExists_VerifiesPasswordWithCorrectHash()
    {
        // Arrange
        var command = new LoginCommand("test@example.com", "Password123!");
        var expectedHash = "expected_hash_value";
        var user = User.Create("test@example.com", expectedHash);
        string? capturedPassword = null;
        string? capturedHash = null;

        this.userRepositoryMock
            .Setup(x => x.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        this.passwordHasherMock
            .Setup(x => x.Verify(It.IsAny<string>(), It.IsAny<string>()))
            .Callback<string, string>((password, hash) =>
            {
                capturedPassword = password;
                capturedHash = hash;
            })
            .Returns(true);

        this.jwtTokenGeneratorMock
            .Setup(x => x.GenerateToken(user))
            .Returns(new TokenResponse
            {
                AccessToken = "token",
                RefreshToken = "refresh",
                ExpiresIn = 3600,
            });

        // Act
        await this.handler.Handle(command, CancellationToken.None);

        // Assert
        capturedPassword.Should().Be(command.Password);
        capturedHash.Should().Be(expectedHash);
    }

    /// <summary>
    /// Tests that constructor throws ArgumentNullException when userRepository is null.
    /// </summary>
    [Fact]
    public void Constructor_WhenUserRepositoryIsNull_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new LoginCommandHandler(
            null!,
            this.passwordHasherMock.Object,
            this.jwtTokenGeneratorMock.Object,
            this.loggerMock.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("userRepository");
    }

    /// <summary>
    /// Tests that constructor throws ArgumentNullException when passwordHasher is null.
    /// </summary>
    [Fact]
    public void Constructor_WhenPasswordHasherIsNull_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new LoginCommandHandler(
            this.userRepositoryMock.Object,
            null!,
            this.jwtTokenGeneratorMock.Object,
            this.loggerMock.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("passwordHasher");
    }

    /// <summary>
    /// Tests that constructor throws ArgumentNullException when jwtTokenGenerator is null.
    /// </summary>
    [Fact]
    public void Constructor_WhenJwtTokenGeneratorIsNull_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new LoginCommandHandler(
            this.userRepositoryMock.Object,
            this.passwordHasherMock.Object,
            null!,
            this.loggerMock.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("jwtTokenGenerator");
    }

    /// <summary>
    /// Tests that constructor throws ArgumentNullException when logger is null.
    /// </summary>
    [Fact]
    public void Constructor_WhenLoggerIsNull_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new LoginCommandHandler(
            this.userRepositoryMock.Object,
            this.passwordHasherMock.Object,
            this.jwtTokenGeneratorMock.Object,
            null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }
}
