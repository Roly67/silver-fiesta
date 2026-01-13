// <copyright file="DeleteConversionTemplateCommandHandlerTests.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using System;
using System.Threading;
using System.Threading.Tasks;

using FileConversionApi.Application.Commands.Templates;
using FileConversionApi.Application.Interfaces;
using FileConversionApi.Domain.Entities;
using FileConversionApi.Domain.ValueObjects;

using FluentAssertions;

using Microsoft.Extensions.Logging;

using Moq;

using Xunit;

namespace FileConversionApi.UnitTests.Application.Commands.Templates;

/// <summary>
/// Unit tests for <see cref="DeleteConversionTemplateCommandHandler"/>.
/// </summary>
public class DeleteConversionTemplateCommandHandlerTests
{
    private readonly Mock<IConversionTemplateRepository> templateRepositoryMock;
    private readonly Mock<IUnitOfWork> unitOfWorkMock;
    private readonly Mock<ILogger<DeleteConversionTemplateCommandHandler>> loggerMock;
    private readonly DeleteConversionTemplateCommandHandler handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteConversionTemplateCommandHandlerTests"/> class.
    /// </summary>
    public DeleteConversionTemplateCommandHandlerTests()
    {
        this.templateRepositoryMock = new Mock<IConversionTemplateRepository>();
        this.unitOfWorkMock = new Mock<IUnitOfWork>();
        this.loggerMock = new Mock<ILogger<DeleteConversionTemplateCommandHandler>>();
        this.handler = new DeleteConversionTemplateCommandHandler(
            this.templateRepositoryMock.Object,
            this.unitOfWorkMock.Object,
            this.loggerMock.Object);
    }

    /// <summary>
    /// Tests that Handle deletes template successfully.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenValidRequest_DeletesTemplateSuccessfully()
    {
        // Arrange
        var userId = UserId.New();
        var template = ConversionTemplate.Create(userId, "Template", "pdf", "{}");

        this.templateRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<ConversionTemplateId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(template);

        this.unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command = new DeleteConversionTemplateCommand
        {
            TemplateId = template.Id.Value,
            UserId = userId.Value,
        };

        // Act
        var result = await this.handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        this.templateRepositoryMock.Verify(x => x.Delete(template), Times.Once);
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

        var command = new DeleteConversionTemplateCommand
        {
            TemplateId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
        };

        // Act
        var result = await this.handler.Handle(command, CancellationToken.None);

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

        var command = new DeleteConversionTemplateCommand
        {
            TemplateId = template.Id.Value,
            UserId = otherUserId,
        };

        // Act
        var result = await this.handler.Handle(command, CancellationToken.None);

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
        var act = () => new DeleteConversionTemplateCommandHandler(
            null!,
            this.unitOfWorkMock.Object,
            this.loggerMock.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("templateRepository");
    }
}
