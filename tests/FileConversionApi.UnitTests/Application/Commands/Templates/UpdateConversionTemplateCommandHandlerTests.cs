// <copyright file="UpdateConversionTemplateCommandHandlerTests.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using System;
using System.Threading;
using System.Threading.Tasks;

using FileConversionApi.Application.Commands.Templates;
using FileConversionApi.Application.DTOs;
using FileConversionApi.Application.Interfaces;
using FileConversionApi.Domain.Entities;
using FileConversionApi.Domain.ValueObjects;

using FluentAssertions;

using Microsoft.Extensions.Logging;

using Moq;

using Xunit;

namespace FileConversionApi.UnitTests.Application.Commands.Templates;

/// <summary>
/// Unit tests for <see cref="UpdateConversionTemplateCommandHandler"/>.
/// </summary>
public class UpdateConversionTemplateCommandHandlerTests
{
    private readonly Mock<IConversionTemplateRepository> templateRepositoryMock;
    private readonly Mock<IUnitOfWork> unitOfWorkMock;
    private readonly Mock<ILogger<UpdateConversionTemplateCommandHandler>> loggerMock;
    private readonly UpdateConversionTemplateCommandHandler handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateConversionTemplateCommandHandlerTests"/> class.
    /// </summary>
    public UpdateConversionTemplateCommandHandlerTests()
    {
        this.templateRepositoryMock = new Mock<IConversionTemplateRepository>();
        this.unitOfWorkMock = new Mock<IUnitOfWork>();
        this.loggerMock = new Mock<ILogger<UpdateConversionTemplateCommandHandler>>();
        this.handler = new UpdateConversionTemplateCommandHandler(
            this.templateRepositoryMock.Object,
            this.unitOfWorkMock.Object,
            this.loggerMock.Object);
    }

    /// <summary>
    /// Tests that Handle updates template successfully.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenValidRequest_UpdatesTemplateSuccessfully()
    {
        // Arrange
        var userId = UserId.New();
        var template = ConversionTemplate.Create(userId, "Old Name", "pdf", "{}", "Old description");
        var templateId = template.Id.Value;

        this.templateRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<ConversionTemplateId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(template);

        this.templateRepositoryMock
            .Setup(x => x.NameExistsForUserAsync(
                It.IsAny<UserId>(),
                It.IsAny<string>(),
                It.IsAny<ConversionTemplateId>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        this.unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command = new UpdateConversionTemplateCommand
        {
            TemplateId = templateId,
            UserId = userId.Value,
            Name = "New Name",
            Description = "New description",
            TargetFormat = "html",
            Options = new ConversionOptions { PageSize = "Letter" },
        };

        // Act
        var result = await this.handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("New Name");
        result.Value.Description.Should().Be("New description");
        result.Value.TargetFormat.Should().Be("html");
        this.templateRepositoryMock.Verify(x => x.Update(template), Times.Once);
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

        var command = new UpdateConversionTemplateCommand
        {
            TemplateId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Name = "New Name",
            TargetFormat = "pdf",
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

        var command = new UpdateConversionTemplateCommand
        {
            TemplateId = template.Id.Value,
            UserId = otherUserId,
            Name = "New Name",
            TargetFormat = "pdf",
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
        var act = () => new UpdateConversionTemplateCommandHandler(
            null!,
            this.unitOfWorkMock.Object,
            this.loggerMock.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("templateRepository");
    }
}
