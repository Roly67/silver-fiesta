// <copyright file="CreateConversionTemplateCommandHandlerTests.cs" company="FileConversionApi">
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
/// Unit tests for <see cref="CreateConversionTemplateCommandHandler"/>.
/// </summary>
public class CreateConversionTemplateCommandHandlerTests
{
    private readonly Mock<IConversionTemplateRepository> templateRepositoryMock;
    private readonly Mock<IUnitOfWork> unitOfWorkMock;
    private readonly Mock<ILogger<CreateConversionTemplateCommandHandler>> loggerMock;
    private readonly CreateConversionTemplateCommandHandler handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateConversionTemplateCommandHandlerTests"/> class.
    /// </summary>
    public CreateConversionTemplateCommandHandlerTests()
    {
        this.templateRepositoryMock = new Mock<IConversionTemplateRepository>();
        this.unitOfWorkMock = new Mock<IUnitOfWork>();
        this.loggerMock = new Mock<ILogger<CreateConversionTemplateCommandHandler>>();
        this.handler = new CreateConversionTemplateCommandHandler(
            this.templateRepositoryMock.Object,
            this.unitOfWorkMock.Object,
            this.loggerMock.Object);
    }

    /// <summary>
    /// Tests that Handle creates template successfully.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenValidRequest_CreatesTemplateSuccessfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        this.templateRepositoryMock
            .Setup(x => x.NameExistsForUserAsync(
                It.IsAny<UserId>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        this.unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command = new CreateConversionTemplateCommand
        {
            UserId = userId,
            Name = "My Template",
            Description = "A test template",
            TargetFormat = "pdf",
            Options = new ConversionOptions { PageSize = "A4" },
        };

        // Act
        var result = await this.handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("My Template");
        result.Value.Description.Should().Be("A test template");
        result.Value.TargetFormat.Should().Be("pdf");
        this.templateRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<ConversionTemplate>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that Handle returns error when name already exists.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenNameExists_ReturnsError()
    {
        // Arrange
        this.templateRepositoryMock
            .Setup(x => x.NameExistsForUserAsync(
                It.IsAny<UserId>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var command = new CreateConversionTemplateCommand
        {
            UserId = Guid.NewGuid(),
            Name = "Existing Template",
            TargetFormat = "pdf",
        };

        // Act
        var result = await this.handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Template.NameAlreadyExists");
    }

    /// <summary>
    /// Tests that Handle returns error for invalid target format.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenInvalidTargetFormat_ReturnsError()
    {
        // Arrange
        var command = new CreateConversionTemplateCommand
        {
            UserId = Guid.NewGuid(),
            Name = "My Template",
            TargetFormat = "invalid",
        };

        // Act
        var result = await this.handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Template.InvalidTargetFormat");
    }

    /// <summary>
    /// Tests that constructor throws when repository is null.
    /// </summary>
    [Fact]
    public void Constructor_WhenRepositoryIsNull_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new CreateConversionTemplateCommandHandler(
            null!,
            this.unitOfWorkMock.Object,
            this.loggerMock.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("templateRepository");
    }
}
