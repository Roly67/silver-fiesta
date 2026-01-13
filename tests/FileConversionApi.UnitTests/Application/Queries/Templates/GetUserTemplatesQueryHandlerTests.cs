// <copyright file="GetUserTemplatesQueryHandlerTests.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using FileConversionApi.Application.Interfaces;
using FileConversionApi.Application.Queries.Templates;
using FileConversionApi.Domain.Entities;
using FileConversionApi.Domain.ValueObjects;

using FluentAssertions;

using Moq;

using Xunit;

namespace FileConversionApi.UnitTests.Application.Queries.Templates;

/// <summary>
/// Unit tests for <see cref="GetUserTemplatesQueryHandler"/>.
/// </summary>
public class GetUserTemplatesQueryHandlerTests
{
    private readonly Mock<IConversionTemplateRepository> templateRepositoryMock;
    private readonly GetUserTemplatesQueryHandler handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetUserTemplatesQueryHandlerTests"/> class.
    /// </summary>
    public GetUserTemplatesQueryHandlerTests()
    {
        this.templateRepositoryMock = new Mock<IConversionTemplateRepository>();
        this.handler = new GetUserTemplatesQueryHandler(this.templateRepositoryMock.Object);
    }

    /// <summary>
    /// Tests that Handle returns all templates for user.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_ReturnsAllTemplatesForUser()
    {
        // Arrange
        var userId = UserId.New();
        var templates = new List<ConversionTemplate>
        {
            ConversionTemplate.Create(userId, "Template 1", "pdf", "{}"),
            ConversionTemplate.Create(userId, "Template 2", "html", "{}"),
        };

        this.templateRepositoryMock
            .Setup(x => x.GetByUserIdAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(templates);

        var query = new GetUserTemplatesQuery { UserId = userId.Value };

        // Act
        var result = await this.handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value[0].Name.Should().Be("Template 1");
        result.Value[1].Name.Should().Be("Template 2");
    }

    /// <summary>
    /// Tests that Handle filters by target format when specified.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenTargetFormatSpecified_FiltersTemplates()
    {
        // Arrange
        var userId = UserId.New();
        var templates = new List<ConversionTemplate>
        {
            ConversionTemplate.Create(userId, "PDF Template", "pdf", "{}"),
        };

        this.templateRepositoryMock
            .Setup(x => x.GetByUserIdAndFormatAsync(
                It.IsAny<UserId>(),
                "pdf",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(templates);

        var query = new GetUserTemplatesQuery
        {
            UserId = userId.Value,
            TargetFormat = "pdf",
        };

        // Act
        var result = await this.handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value[0].TargetFormat.Should().Be("pdf");
        this.templateRepositoryMock.Verify(
            x => x.GetByUserIdAndFormatAsync(It.IsAny<UserId>(), "pdf", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that Handle returns empty list when no templates exist.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenNoTemplates_ReturnsEmptyList()
    {
        // Arrange
        this.templateRepositoryMock
            .Setup(x => x.GetByUserIdAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ConversionTemplate>());

        var query = new GetUserTemplatesQuery { UserId = Guid.NewGuid() };

        // Act
        var result = await this.handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that constructor throws when repository is null.
    /// </summary>
    [Fact]
    public void Constructor_WhenRepositoryIsNull_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new GetUserTemplatesQueryHandler(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("templateRepository");
    }
}
