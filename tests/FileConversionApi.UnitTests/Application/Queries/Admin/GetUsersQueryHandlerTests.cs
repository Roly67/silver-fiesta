// <copyright file="GetUsersQueryHandlerTests.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using FileConversionApi.Application.Interfaces;
using FileConversionApi.Application.Queries.Admin;
using FileConversionApi.Domain.Entities;

using FluentAssertions;

using Moq;

using Xunit;

namespace FileConversionApi.UnitTests.Application.Queries.Admin;

/// <summary>
/// Unit tests for <see cref="GetUsersQueryHandler"/>.
/// </summary>
public class GetUsersQueryHandlerTests
{
    private readonly Mock<IUserRepository> userRepositoryMock;
    private readonly GetUsersQueryHandler handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetUsersQueryHandlerTests"/> class.
    /// </summary>
    public GetUsersQueryHandlerTests()
    {
        this.userRepositoryMock = new Mock<IUserRepository>();
        this.handler = new GetUsersQueryHandler(this.userRepositoryMock.Object);
    }

    /// <summary>
    /// Tests that Handle returns paginated users successfully.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_ReturnsPagedUsersSuccessfully()
    {
        // Arrange
        var users = new List<User>
        {
            User.Create("user1@test.com", "hash1"),
            User.Create("user2@test.com", "hash2"),
        };

        this.userRepositoryMock
            .Setup(x => x.GetAllAsync(1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync((users, 2));

        var query = new GetUsersQuery { Page = 1, PageSize = 20 };

        // Act
        var result = await this.handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(2);
        result.Value.TotalCount.Should().Be(2);
        result.Value.Page.Should().Be(1);
        result.Value.PageSize.Should().Be(20);
    }

    /// <summary>
    /// Tests that Handle clamps page size to maximum of 100.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_ClampsPageSizeToMax100()
    {
        // Arrange
        this.userRepositoryMock
            .Setup(x => x.GetAllAsync(1, 100, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<User>(), 0));

        var query = new GetUsersQuery { Page = 1, PageSize = 500 };

        // Act
        await this.handler.Handle(query, CancellationToken.None);

        // Assert
        this.userRepositoryMock.Verify(x => x.GetAllAsync(1, 100, It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that Handle ensures page is at least 1.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_EnsuresPageIsAtLeastOne()
    {
        // Arrange
        this.userRepositoryMock
            .Setup(x => x.GetAllAsync(1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<User>(), 0));

        var query = new GetUsersQuery { Page = -5, PageSize = 20 };

        // Act
        await this.handler.Handle(query, CancellationToken.None);

        // Assert
        this.userRepositoryMock.Verify(x => x.GetAllAsync(1, 20, It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that constructor throws ArgumentNullException when repository is null.
    /// </summary>
    [Fact]
    public void Constructor_WhenRepositoryIsNull_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new GetUsersQueryHandler(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("userRepository");
    }
}
