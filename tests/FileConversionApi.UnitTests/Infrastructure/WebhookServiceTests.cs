// <copyright file="WebhookServiceTests.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using System.Net;

using FileConversionApi.Domain.Entities;
using FileConversionApi.Domain.ValueObjects;
using FileConversionApi.Infrastructure.Options;
using FileConversionApi.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;

namespace FileConversionApi.UnitTests.Infrastructure;

/// <summary>
/// Unit tests for the <see cref="WebhookService"/> class.
/// </summary>
public class WebhookServiceTests : IDisposable
{
    private readonly Mock<HttpMessageHandler> httpMessageHandlerMock;
    private readonly Mock<ILogger<WebhookService>> loggerMock;
    private readonly WebhookSettings settings;
    private readonly HttpClient httpClient;
    private readonly List<HttpResponseMessage> responseMessages = [];
    private bool disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="WebhookServiceTests"/> class.
    /// </summary>
    public WebhookServiceTests()
    {
        this.httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        this.loggerMock = new Mock<ILogger<WebhookService>>();
        this.settings = new WebhookSettings
        {
            TimeoutSeconds = 30,
            MaxRetries = 3,
            RetryDelayMilliseconds = 100,
        };
        this.httpClient = new HttpClient(this.httpMessageHandlerMock.Object);
    }

    /// <summary>
    /// Tests that SendJobCompletedAsync does nothing when webhook URL is null.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task SendJobCompletedAsync_WhenWebhookUrlIsNull_DoesNothing()
    {
        // Arrange
        var service = this.CreateService();
        var job = CreateJob(webhookUrl: null);

        // Act
        await service.SendJobCompletedAsync(job, CancellationToken.None);

        // Assert
        this.httpMessageHandlerMock.Protected()
            .Verify(
                "SendAsync",
                Times.Never(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
    }

    /// <summary>
    /// Tests that SendJobCompletedAsync does nothing when webhook URL is empty.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task SendJobCompletedAsync_WhenWebhookUrlIsEmpty_DoesNothing()
    {
        // Arrange
        var service = this.CreateService();
        var job = CreateJob(webhookUrl: string.Empty);

        // Act
        await service.SendJobCompletedAsync(job, CancellationToken.None);

        // Assert
        this.httpMessageHandlerMock.Protected()
            .Verify(
                "SendAsync",
                Times.Never(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
    }

    /// <summary>
    /// Tests that SendJobCompletedAsync does nothing when webhook URL is whitespace.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task SendJobCompletedAsync_WhenWebhookUrlIsWhitespace_DoesNothing()
    {
        // Arrange
        var service = this.CreateService();
        var job = CreateJob(webhookUrl: "   ");

        // Act
        await service.SendJobCompletedAsync(job, CancellationToken.None);

        // Assert
        this.httpMessageHandlerMock.Protected()
            .Verify(
                "SendAsync",
                Times.Never(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
    }

    /// <summary>
    /// Tests that SendJobCompletedAsync sends POST request to webhook URL.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task SendJobCompletedAsync_WhenWebhookUrlIsValid_SendsPostRequest()
    {
        // Arrange
        var webhookUrl = "https://example.com/webhook";
        var service = this.CreateService();
        var job = CreateJob(webhookUrl: webhookUrl);

        this.SetupHttpResponse(HttpStatusCode.OK);

        // Act
        await service.SendJobCompletedAsync(job, CancellationToken.None);

        // Assert
        this.httpMessageHandlerMock.Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri!.ToString() == webhookUrl),
                ItExpr.IsAny<CancellationToken>());
    }

    /// <summary>
    /// Tests that SendJobCompletedAsync retries on failure.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task SendJobCompletedAsync_WhenRequestFails_RetriesConfiguredTimes()
    {
        // Arrange
        var webhookUrl = "https://example.com/webhook";
        var service = this.CreateService();
        var job = CreateJob(webhookUrl: webhookUrl);

        this.SetupHttpResponse(HttpStatusCode.InternalServerError);

        // Act
        await service.SendJobCompletedAsync(job, CancellationToken.None);

        // Assert
        this.httpMessageHandlerMock.Protected()
            .Verify(
                "SendAsync",
                Times.Exactly(this.settings.MaxRetries),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
    }

    /// <summary>
    /// Tests that SendJobCompletedAsync stops retrying on success.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task SendJobCompletedAsync_WhenSecondAttemptSucceeds_StopsRetrying()
    {
        // Arrange
        var webhookUrl = "https://example.com/webhook";
        var service = this.CreateService();
        var job = CreateJob(webhookUrl: webhookUrl);

        var callCount = 0;
        this.httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() =>
            {
                callCount++;
                return new HttpResponseMessage(
                    callCount == 1 ? HttpStatusCode.InternalServerError : HttpStatusCode.OK);
            });

        // Act
        await service.SendJobCompletedAsync(job, CancellationToken.None);

        // Assert
        this.httpMessageHandlerMock.Protected()
            .Verify(
                "SendAsync",
                Times.Exactly(2),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
    }

    /// <summary>
    /// Tests that SendJobCompletedAsync handles HttpRequestException gracefully.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task SendJobCompletedAsync_WhenHttpRequestExceptionThrown_HandlesGracefully()
    {
        // Arrange
        var webhookUrl = "https://example.com/webhook";
        var service = this.CreateService();
        var job = CreateJob(webhookUrl: webhookUrl);

        this.httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection failed"));

        // Act & Assert
        var act = () => service.SendJobCompletedAsync(job, CancellationToken.None);
        await act.Should().NotThrowAsync();
    }

    /// <summary>
    /// Tests that SendJobCompletedAsync handles timeout gracefully.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task SendJobCompletedAsync_WhenTimeoutOccurs_HandlesGracefully()
    {
        // Arrange
        var webhookUrl = "https://example.com/webhook";
        var service = this.CreateService();
        var job = CreateJob(webhookUrl: webhookUrl);

        this.httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new TaskCanceledException("Request timed out"));

        // Act & Assert
        var act = () => service.SendJobCompletedAsync(job, CancellationToken.None);
        await act.Should().NotThrowAsync();
    }

    /// <summary>
    /// Tests that constructor throws ArgumentNullException when httpClient is null.
    /// </summary>
    [Fact]
    public void Constructor_WhenHttpClientIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        var options = Options.Create(this.settings);

        // Act
        var act = () => new WebhookService(null!, options, this.loggerMock.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("httpClient");
    }

    /// <summary>
    /// Tests that constructor throws ArgumentNullException when settings is null.
    /// </summary>
    [Fact]
    public void Constructor_WhenSettingsIsNull_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new WebhookService(this.httpClient, null!, this.loggerMock.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("settings");
    }

    /// <summary>
    /// Tests that constructor throws ArgumentNullException when logger is null.
    /// </summary>
    [Fact]
    public void Constructor_WhenLoggerIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        var options = Options.Create(this.settings);

        // Act
        var act = () => new WebhookService(this.httpClient, options, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the test resources.
    /// </summary>
    /// <param name="disposing">Whether to dispose managed resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!this.disposed)
        {
            if (disposing)
            {
                foreach (var response in this.responseMessages)
                {
                    response.Dispose();
                }

                this.httpClient.Dispose();
            }

            this.disposed = true;
        }
    }

    private static ConversionJob CreateJob(string? webhookUrl)
    {
        return ConversionJob.Create(
            UserId.New(),
            "html",
            "pdf",
            "test.html",
            webhookUrl);
    }

    private WebhookService CreateService()
    {
        var options = Options.Create(this.settings);
        return new WebhookService(this.httpClient, options, this.loggerMock.Object);
    }

    private void SetupHttpResponse(HttpStatusCode statusCode)
    {
        this.httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() =>
            {
                var response = new HttpResponseMessage(statusCode);
                this.responseMessages.Add(response);
                return response;
            });
    }
}
