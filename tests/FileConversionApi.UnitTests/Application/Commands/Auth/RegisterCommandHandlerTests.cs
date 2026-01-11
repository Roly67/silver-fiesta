// <copyright file="RegisterCommandHandlerTests.cs" company="FileConversionApi">
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
/// Unit tests for <see cref="RegisterCommandHandler"/>.
/// </summary>
public class RegisterCommandHandlerTests
{
    private readonly Mock<IUserRepository> userRepositoryMock;
    private readonly Mock<IUnitOfWork> unitOfWorkMock;
    private readonly Mock<IPasswordHasher> passwordHasherMock;
    private readonly Mock<IJwtTokenGenerator> jwtTokenGeneratorMock;
    private readonly Mock<ILogger<RegisterCommandHandler>> loggerMock;
    private readonly RegisterCommandHandler handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterCommandHandlerTests"/> class.
    /// </summary>
    public RegisterCommandHandlerTests()
    {
        this.userRepositoryMock = new Mock<IUserRepository>();
        this.unitOfWorkMock = new Mock<IUnitOfWork>();
        this.passwordHasherMock = new Mock<IPasswordHasher>();
        this.jwtTokenGeneratorMock = new Mock<IJwtTokenGenerator>();
        this.loggerMock = new Mock<ILogger<RegisterCommandHandler>>();

        this.handler = new RegisterCommandHandler(
            this.userRepositoryMock.Object,
            this.unitOfWorkMock.Object,
            this.passwordHasherMock.Object,
            this.jwtTokenGeneratorMock.Object,
            this.loggerMock.Object);
    }

    /// <summary>
    /// Tests that Handle returns success with token response when registration is successful.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenEmailDoesNotExist_ReturnsSuccessWithTokenResponse()
    {
        // Arrange
        var command = new RegisterCommand("test@example.com", "Password123!");
        var hashedPassword = "hashed_password";
        var expectedTokenResponse = new TokenResponse
        {
            AccessToken = "access_token",
            RefreshToken = "refresh_token",
            ExpiresIn = 3600,
        };

        this.userRepositoryMock
            .Setup(x => x.EmailExistsAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        this.passwordHasherMock
            .Setup(x => x.Hash(command.Password))
            .Returns(hashedPassword);

        this.userRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        this.unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        this.jwtTokenGeneratorMock
            .Setup(x => x.GenerateToken(It.IsAny<User>()))
            .Returns(expectedTokenResponse);

        // Act
        var result = await this.handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(expectedTokenResponse);

        this.userRepositoryMock.Verify(
            x => x.EmailExistsAsync(command.Email, It.IsAny<CancellationToken>()),
            Times.Once);
        this.passwordHasherMock.Verify(x => x.Hash(command.Password), Times.Once);
        this.userRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Once);
        this.unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        this.jwtTokenGeneratorMock.Verify(x => x.GenerateToken(It.IsAny<User>()), Times.Once);
    }

    /// <summary>
    /// Tests that Handle returns failure when email already exists.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenEmailAlreadyExists_ReturnsFailureWithEmailAlreadyExistsError()
    {
        // Arrange
        var command = new RegisterCommand("existing@example.com", "Password123!");

        this.userRepositoryMock
            .Setup(x => x.EmailExistsAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await this.handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.EmailAlreadyExists);

        this.userRepositoryMock.Verify(
            x => x.EmailExistsAsync(command.Email, It.IsAny<CancellationToken>()),
            Times.Once);
        this.passwordHasherMock.Verify(x => x.Hash(It.IsAny<string>()), Times.Never);
        this.userRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Never);
        this.unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        this.jwtTokenGeneratorMock.Verify(x => x.GenerateToken(It.IsAny<User>()), Times.Never);
    }

    /// <summary>
    /// Tests that Handle creates user with correct email and hashed password.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenSuccessful_CreatesUserWithCorrectEmailAndHashedPassword()
    {
        // Arrange
        var command = new RegisterCommand("newuser@example.com", "SecurePass123!");
        var hashedPassword = "hashed_secure_password";
        User? capturedUser = null;

        this.userRepositoryMock
            .Setup(x => x.EmailExistsAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        this.passwordHasherMock
            .Setup(x => x.Hash(command.Password))
            .Returns(hashedPassword);

        this.userRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((user, _) => capturedUser = user)
            .Returns(Task.CompletedTask);

        this.unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        this.jwtTokenGeneratorMock
            .Setup(x => x.GenerateToken(It.IsAny<User>()))
            .Returns(new TokenResponse
            {
                AccessToken = "token",
                RefreshToken = "refresh",
                ExpiresIn = 3600,
            });

        // Act
        await this.handler.Handle(command, CancellationToken.None);

        // Assert
        capturedUser.Should().NotBeNull();
        capturedUser!.Email.Should().Be(command.Email);
        capturedUser.PasswordHash.Should().Be(hashedPassword);
        capturedUser.IsActive.Should().BeTrue();
    }

    /// <summary>
    /// Tests that constructor throws ArgumentNullException when userRepository is null.
    /// </summary>
    [Fact]
    public void Constructor_WhenUserRepositoryIsNull_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new RegisterCommandHandler(
            null!,
            this.unitOfWorkMock.Object,
            this.passwordHasherMock.Object,
            this.jwtTokenGeneratorMock.Object,
            this.loggerMock.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("userRepository");
    }

    /// <summary>
    /// Tests that constructor throws ArgumentNullException when unitOfWork is null.
    /// </summary>
    [Fact]
    public void Constructor_WhenUnitOfWorkIsNull_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new RegisterCommandHandler(
            this.userRepositoryMock.Object,
            null!,
            this.passwordHasherMock.Object,
            this.jwtTokenGeneratorMock.Object,
            this.loggerMock.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("unitOfWork");
    }

    /// <summary>
    /// Tests that constructor throws ArgumentNullException when passwordHasher is null.
    /// </summary>
    [Fact]
    public void Constructor_WhenPasswordHasherIsNull_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new RegisterCommandHandler(
            this.userRepositoryMock.Object,
            this.unitOfWorkMock.Object,
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
        var act = () => new RegisterCommandHandler(
            this.userRepositoryMock.Object,
            this.unitOfWorkMock.Object,
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
        var act = () => new RegisterCommandHandler(
            this.userRepositoryMock.Object,
            this.unitOfWorkMock.Object,
            this.passwordHasherMock.Object,
            this.jwtTokenGeneratorMock.Object,
            null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }
}
