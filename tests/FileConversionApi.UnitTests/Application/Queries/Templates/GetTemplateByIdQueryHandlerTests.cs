// <copyright file="GetTemplateByIdQueryHandlerTests.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using System;
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
/// Unit tests for <see cref="GetTemplateByIdQueryHandler"/>.
/// </summary>
public class GetTemplateByIdQueryHandlerTests
{
    private readonly Mock<IConversionTemplateRepository> templateRepositoryMock;
    private readonly GetTemplateByIdQueryHandler handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetTemplateByIdQueryHandlerTests"/> class.
    /// </summary>
    public GetTemplateByIdQueryHandlerTests()
    {
        this.templateRepositoryMock = new Mock<IConversionTemplateRepository>();
        this.handler = new GetTemplateByIdQueryHandler(this.templateRepositoryMock.Object);
    }

    /// <summary>
    /// Tests that Handle returns template when found.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenTemplateFound_ReturnsTemplateDto()
    {
        // Arrange
        var userId = UserId.New();
        var template = ConversionTemplate.Create(userId, "Test Template", "pdf", "{}");

        this.templateRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<ConversionTemplateId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(template);

        var query = new GetTemplateByIdQuery
        {
            TemplateId = template.Id.Value,
            UserId = userId.Value,
        };

        // Act
        var result = await this.handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Test Template");
        result.Value.TargetFormat.Should().Be("pdf");
    }

    /// <summary>
    /// Tests that Handle returns error when template not found.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenTemplateNotFound_ReturnsError()
    {
        // Arrange
        this.templateRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<ConversionTemplateId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ConversionTemplate?)null);

        var query = new GetTemplateByIdQuery
        {
            TemplateId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
        };

        // Act
        var result = await this.handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Template.NotFound");
    }

    /// <summary>
    /// Tests that Handle returns error when user doesn't own template.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenUserDoesNotOwnTemplate_ReturnsAccessDeniedError()
    {
        // Arrange
        var ownerId = UserId.New();
        var otherUserId = Guid.NewGuid();
        var template = ConversionTemplate.Create(ownerId, "Template", "pdf", "{}");

        this.templateRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<ConversionTemplateId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(template);

        var query = new GetTemplateByIdQuery
        {
            TemplateId = template.Id.Value,
            UserId = otherUserId,
        };

        // Act
        var result = await this.handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Template.AccessDenied");
    }

    /// <summary>
    /// Tests that constructor throws when repository is null.
    /// </summary>
    [Fact]
    public void Constructor_WhenRepositoryIsNull_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new GetTemplateByIdQueryHandler(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("templateRepository");
    }
}
