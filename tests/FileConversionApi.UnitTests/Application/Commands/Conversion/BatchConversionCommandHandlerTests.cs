// <copyright file="BatchConversionCommandHandlerTests.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using System;
using System.Threading;
using System.Threading.Tasks;

using FileConversionApi.Application.Commands.Conversion;
using FileConversionApi.Application.DTOs;
using FileConversionApi.Domain.Enums;
using FileConversionApi.Domain.Primitives;

using FluentAssertions;

using MediatR;

using Microsoft.Extensions.Logging;

using Moq;

using Xunit;

namespace FileConversionApi.UnitTests.Application.Commands.Conversion;

/// <summary>
/// Unit tests for <see cref="BatchConversionCommandHandler"/>.
/// </summary>
public class BatchConversionCommandHandlerTests
{
    private readonly Mock<IMediator> mediatorMock;
    private readonly Mock<ILogger<BatchConversionCommandHandler>> loggerMock;
    private readonly BatchConversionCommandHandler handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="BatchConversionCommandHandlerTests"/> class.
    /// </summary>
    public BatchConversionCommandHandlerTests()
    {
        this.mediatorMock = new Mock<IMediator>();
        this.loggerMock = new Mock<ILogger<BatchConversionCommandHandler>>();
        this.handler = new BatchConversionCommandHandler(
            this.mediatorMock.Object,
            this.loggerMock.Object);
    }

    /// <summary>
    /// Tests that Handle returns error when items list is empty.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenItemsEmpty_ReturnsEmptyRequestError()
    {
        // Arrange
        var command = new BatchConversionCommand
        {
            Items = [],
        };

        // Act
        var result = await this.handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Batch.EmptyRequest");
    }

    /// <summary>
    /// Tests that Handle returns error when batch size exceeds maximum.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenBatchSizeExceedsMax_ReturnsTooManyItemsError()
    {
        // Arrange
        var items = new List<BatchConversionItem>();
        for (int i = 0; i < 21; i++)
        {
            items.Add(new BatchConversionItem { Type = "html-to-pdf" });
        }

        var command = new BatchConversionCommand
        {
            Items = items,
        };

        // Act
        var result = await this.handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Batch.TooManyItems");
        result.Error.Message.Should().Contain("20");
    }

    /// <summary>
    /// Tests that Handle returns failure result when item type is missing.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenItemTypeMissing_ReturnsFailureForItem()
    {
        // Arrange
        var command = new BatchConversionCommand
        {
            Items =
            [
                new BatchConversionItem { Type = null },
            ],
        };

        // Act
        var result = await this.handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.TotalItems.Should().Be(1);
        result.Value.FailureCount.Should().Be(1);
        result.Value.SuccessCount.Should().Be(0);
        result.Value.Results[0].Success.Should().BeFalse();
        result.Value.Results[0].ErrorCode.Should().Be("Batch.MissingType");
    }

    /// <summary>
    /// Tests that Handle returns failure result when item type is invalid.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenItemTypeInvalid_ReturnsFailureForItem()
    {
        // Arrange
        var command = new BatchConversionCommand
        {
            Items =
            [
                new BatchConversionItem { Type = "unknown-type" },
            ],
        };

        // Act
        var result = await this.handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.TotalItems.Should().Be(1);
        result.Value.FailureCount.Should().Be(1);
        result.Value.Results[0].Success.Should().BeFalse();
        result.Value.Results[0].ErrorCode.Should().Be("Batch.InvalidType");
        result.Value.Results[0].ErrorMessage.Should().Contain("unknown-type");
    }

    /// <summary>
    /// Tests that Handle processes html-to-pdf conversion correctly.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenHtmlToPdfItem_DelegatesToMediator()
    {
        // Arrange
        var jobDto = CreateTestJobDto();

        this.mediatorMock
            .Setup(x => x.Send(It.IsAny<ConvertHtmlToPdfCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(jobDto));

        var command = new BatchConversionCommand
        {
            Items =
            [
                new BatchConversionItem
                {
                    Type = "html-to-pdf",
                    HtmlContent = "<html><body>Test</body></html>",
                    FileName = "test.html",
                },
            ],
        };

        // Act
        var result = await this.handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.SuccessCount.Should().Be(1);
        result.Value.FailureCount.Should().Be(0);
        result.Value.Results[0].Success.Should().BeTrue();
        result.Value.Results[0].Job.Should().Be(jobDto);

        this.mediatorMock.Verify(
            x => x.Send(It.Is<ConvertHtmlToPdfCommand>(c => c.HtmlContent == "<html><body>Test</body></html>"), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that Handle processes markdown-to-pdf conversion correctly.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenMarkdownToPdfItem_DelegatesToMediator()
    {
        // Arrange
        var jobDto = CreateTestJobDto();

        this.mediatorMock
            .Setup(x => x.Send(It.IsAny<ConvertMarkdownToPdfCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(jobDto));

        var command = new BatchConversionCommand
        {
            Items =
            [
                new BatchConversionItem
                {
                    Type = "markdown-to-pdf",
                    Markdown = "# Hello",
                    FileName = "test.md",
                },
            ],
        };

        // Act
        var result = await this.handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.SuccessCount.Should().Be(1);
        result.Value.Results[0].Success.Should().BeTrue();

        this.mediatorMock.Verify(
            x => x.Send(It.Is<ConvertMarkdownToPdfCommand>(c => c.Markdown == "# Hello"), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that Handle processes markdown-to-html conversion correctly.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenMarkdownToHtmlItem_DelegatesToMediator()
    {
        // Arrange
        var jobDto = CreateTestJobDto();

        this.mediatorMock
            .Setup(x => x.Send(It.IsAny<ConvertMarkdownToHtmlCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(jobDto));

        var command = new BatchConversionCommand
        {
            Items =
            [
                new BatchConversionItem
                {
                    Type = "markdown-to-html",
                    Markdown = "# Test",
                },
            ],
        };

        // Act
        var result = await this.handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.SuccessCount.Should().Be(1);

        this.mediatorMock.Verify(
            x => x.Send(It.Is<ConvertMarkdownToHtmlCommand>(c => c.Markdown == "# Test"), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that Handle processes image conversion correctly.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenImageItem_DelegatesToMediator()
    {
        // Arrange
        var jobDto = CreateTestJobDto();

        this.mediatorMock
            .Setup(x => x.Send(It.IsAny<ConvertImageCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(jobDto));

        var command = new BatchConversionCommand
        {
            Items =
            [
                new BatchConversionItem
                {
                    Type = "image",
                    ImageData = "base64data",
                    SourceFormat = "png",
                    TargetFormat = "jpeg",
                },
            ],
        };

        // Act
        var result = await this.handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.SuccessCount.Should().Be(1);

        this.mediatorMock.Verify(
            x => x.Send(It.Is<ConvertImageCommand>(c => c.ImageData == "base64data" && c.SourceFormat == "png" && c.TargetFormat == "jpeg"), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that Handle processes multiple items with mixed results.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenMixedResults_ReturnsCorrectCounts()
    {
        // Arrange
        var jobDto = CreateTestJobDto();

        this.mediatorMock
            .Setup(x => x.Send(It.IsAny<ConvertHtmlToPdfCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(jobDto));

        var command = new BatchConversionCommand
        {
            Items =
            [
                new BatchConversionItem
                {
                    Type = "html-to-pdf",
                    HtmlContent = "<html>Test</html>",
                },
                new BatchConversionItem
                {
                    Type = "invalid-type",
                },
                new BatchConversionItem
                {
                    Type = "html-to-pdf",
                    HtmlContent = "<html>Test2</html>",
                },
            ],
        };

        // Act
        var result = await this.handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.TotalItems.Should().Be(3);
        result.Value.SuccessCount.Should().Be(2);
        result.Value.FailureCount.Should().Be(1);

        result.Value.Results[0].Success.Should().BeTrue();
        result.Value.Results[0].Index.Should().Be(0);

        result.Value.Results[1].Success.Should().BeFalse();
        result.Value.Results[1].Index.Should().Be(1);

        result.Value.Results[2].Success.Should().BeTrue();
        result.Value.Results[2].Index.Should().Be(2);
    }

    /// <summary>
    /// Tests that Handle handles conversion failure correctly.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenConversionFails_ReturnsFailureResult()
    {
        // Arrange
        var error = new Error("Test.Error", "Test error message");

        this.mediatorMock
            .Setup(x => x.Send(It.IsAny<ConvertHtmlToPdfCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<ConversionJobDto>(error));

        var command = new BatchConversionCommand
        {
            Items =
            [
                new BatchConversionItem
                {
                    Type = "html-to-pdf",
                    HtmlContent = "<html>Test</html>",
                },
            ],
        };

        // Act
        var result = await this.handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.FailureCount.Should().Be(1);
        result.Value.Results[0].Success.Should().BeFalse();
        result.Value.Results[0].ErrorCode.Should().Be("Test.Error");
        result.Value.Results[0].ErrorMessage.Should().Be("Test error message");
    }

    /// <summary>
    /// Tests that Handle handles exception during processing correctly.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenProcessingThrowsException_ReturnsFailureResult()
    {
        // Arrange
        this.mediatorMock
            .Setup(x => x.Send(It.IsAny<ConvertHtmlToPdfCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Test exception"));

        var command = new BatchConversionCommand
        {
            Items =
            [
                new BatchConversionItem
                {
                    Type = "html-to-pdf",
                    HtmlContent = "<html>Test</html>",
                },
            ],
        };

        // Act
        var result = await this.handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.FailureCount.Should().Be(1);
        result.Value.Results[0].Success.Should().BeFalse();
        result.Value.Results[0].ErrorCode.Should().Be("Batch.ProcessingFailed");
        result.Value.Results[0].ErrorMessage.Should().Be("Test exception");
    }

    /// <summary>
    /// Tests that Handle passes webhook URL to conversion commands.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenWebhookUrlProvided_PassesToConversionCommands()
    {
        // Arrange
        var jobDto = CreateTestJobDto();

        ConvertHtmlToPdfCommand? capturedCommand = null;
        this.mediatorMock
            .Setup(x => x.Send(It.IsAny<ConvertHtmlToPdfCommand>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<Result<ConversionJobDto>>, CancellationToken>((cmd, _) => capturedCommand = (ConvertHtmlToPdfCommand)cmd)
            .ReturnsAsync(Result.Success(jobDto));

        var command = new BatchConversionCommand
        {
            Items =
            [
                new BatchConversionItem
                {
                    Type = "html-to-pdf",
                    HtmlContent = "<html>Test</html>",
                },
            ],
            WebhookUrl = "https://example.com/webhook",
        };

        // Act
        await this.handler.Handle(command, CancellationToken.None);

        // Assert
        capturedCommand.Should().NotBeNull();
        capturedCommand!.WebhookUrl.Should().Be("https://example.com/webhook");
    }

    /// <summary>
    /// Tests that Handle supports case-insensitive type matching.
    /// </summary>
    /// <param name="type">The conversion type with different casing.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Theory]
    [InlineData("HTML-TO-PDF")]
    [InlineData("Html-To-Pdf")]
    [InlineData("html-to-pdf")]
    public async Task Handle_WhenTypeHasDifferentCase_ProcessesCorrectly(string type)
    {
        // Arrange
        var jobDto = CreateTestJobDto();

        this.mediatorMock
            .Setup(x => x.Send(It.IsAny<ConvertHtmlToPdfCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(jobDto));

        var command = new BatchConversionCommand
        {
            Items =
            [
                new BatchConversionItem
                {
                    Type = type,
                    HtmlContent = "<html>Test</html>",
                },
            ],
        };

        // Act
        var result = await this.handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.SuccessCount.Should().Be(1);
    }

    /// <summary>
    /// Tests that constructor throws ArgumentNullException when mediator is null.
    /// </summary>
    [Fact]
    public void Constructor_WhenMediatorIsNull_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new BatchConversionCommandHandler(
            null!,
            this.loggerMock.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("mediator");
    }

    /// <summary>
    /// Tests that constructor throws ArgumentNullException when logger is null.
    /// </summary>
    [Fact]
    public void Constructor_WhenLoggerIsNull_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new BatchConversionCommandHandler(
            this.mediatorMock.Object,
            null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    /// <summary>
    /// Tests that Handle respects cancellation token.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenCancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var command = new BatchConversionCommand
        {
            Items =
            [
                new BatchConversionItem
                {
                    Type = "html-to-pdf",
                    HtmlContent = "<html>Test</html>",
                },
            ],
        };

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => this.handler.Handle(command, cts.Token));
    }

    /// <summary>
    /// Tests that Handle passes conversion options to commands.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_WhenOptionsProvided_PassesOptionsToCommands()
    {
        // Arrange
        var jobDto = CreateTestJobDto();

        ConvertHtmlToPdfCommand? capturedCommand = null;
        this.mediatorMock
            .Setup(x => x.Send(It.IsAny<ConvertHtmlToPdfCommand>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<Result<ConversionJobDto>>, CancellationToken>((cmd, _) => capturedCommand = (ConvertHtmlToPdfCommand)cmd)
            .ReturnsAsync(Result.Success(jobDto));

        var options = new ConversionOptions
        {
            PageSize = "Letter",
            Landscape = true,
            MarginTop = 30,
        };

        var command = new BatchConversionCommand
        {
            Items =
            [
                new BatchConversionItem
                {
                    Type = "html-to-pdf",
                    HtmlContent = "<html>Test</html>",
                    Options = options,
                },
            ],
        };

        // Act
        await this.handler.Handle(command, CancellationToken.None);

        // Assert
        capturedCommand.Should().NotBeNull();
        capturedCommand!.Options.Should().NotBeNull();
        capturedCommand.Options!.PageSize.Should().Be("Letter");
        capturedCommand.Options.Landscape.Should().BeTrue();
        capturedCommand.Options.MarginTop.Should().Be(30);
    }

    private static ConversionJobDto CreateTestJobDto() => new()
    {
        Id = Guid.NewGuid(),
        Status = ConversionStatus.Completed,
        SourceFormat = "html",
        TargetFormat = "pdf",
        InputFileName = "test.html",
        CreatedAt = DateTimeOffset.UtcNow,
    };
}
