// <copyright file="GetUserByIdQueryHandlerTests.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using System;
using System.Threading;
using System.Threading.Tasks;

using FileConversionApi.Application.Interfaces;
using FileConversionApi.Application.Queries.Admin;
using FileConversionApi.Domain.Entities;
using FileConversionApi.Domain.ValueObjects;

using FluentAssertions;

using Moq;

using Xunit;

namespace FileConversionApi.UnitTests.Application.Queries.Admin;

/// <summary>
/// Unit tests for <see cref="GetUserByIdQueryHandler"/>.
/// </summary>
public class GetUserByIdQueryHandlerTests
{
    private readonly Mock<IUserRepository> userRepositoryMock;
    private readonly GetUserByIdQueryHandler handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetUserByIdQueryHandlerTests"/> class.
    /// </summary>
    public GetUserByIdQueryHandlerTests()
    {
        this.userRepositoryMock = new Mock<IUserRepository>();
        this.handler = new GetUserByIdQueryHandler(this.userRepositoryMock.Object);
    }

    /// <summary>
    /// Tests that Handle returns user when found.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenUserFound_ReturnsUserDto()
    {
        // Arrange
        var user = User.Create("test@test.com", "hash");
        var userId = user.Id.Value;

        this.userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var query = new GetUserByIdQuery { UserId = userId };

        // Act
        var result = await this.handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Email.Should().Be("test@test.com");
        result.Value.Id.Should().Be(userId);
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

        var query = new GetUserByIdQuery { UserId = Guid.NewGuid() };

        // Act
        var result = await this.handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Admin.UserNotFound");
    }

    /// <summary>
    /// Tests that constructor throws ArgumentNullException when repository is null.
    /// </summary>
    [Fact]
    public void Constructor_WhenRepositoryIsNull_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new GetUserByIdQueryHandler(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("userRepository");
    }
}
