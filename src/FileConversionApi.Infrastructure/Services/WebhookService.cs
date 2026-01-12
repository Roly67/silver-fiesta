// <copyright file="WebhookService.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

using FileConversionApi.Application.Interfaces;
using FileConversionApi.Domain.Entities;
using FileConversionApi.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FileConversionApi.Infrastructure.Services;

/// <summary>
/// Service for sending webhook notifications when conversion jobs complete.
/// </summary>
public class WebhookService : IWebhookService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly HttpClient httpClient;
    private readonly WebhookSettings settings;
    private readonly ILogger<WebhookService> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="WebhookService"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client.</param>
    /// <param name="settings">The webhook settings.</param>
    /// <param name="logger">The logger.</param>
    public WebhookService(
        HttpClient httpClient,
        IOptions<WebhookSettings> settings,
        ILogger<WebhookService> logger)
    {
        this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        this.settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

        this.httpClient.Timeout = TimeSpan.FromSeconds(this.settings.TimeoutSeconds);
    }

    /// <inheritdoc/>
    public async Task SendJobCompletedAsync(ConversionJob job, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(job.WebhookUrl))
        {
            return;
        }

        var payload = CreatePayload(job);

        for (var attempt = 1; attempt <= this.settings.MaxRetries; attempt++)
        {
            try
            {
                this.logger.LogDebug(
                    "Sending webhook for job {JobId} to {WebhookUrl} (attempt {Attempt}/{MaxRetries})",
                    job.Id,
                    job.WebhookUrl,
                    attempt,
                    this.settings.MaxRetries);

                var response = await this.httpClient.PostAsJsonAsync(
                    job.WebhookUrl,
                    payload,
                    JsonOptions,
                    cancellationToken).ConfigureAwait(false);

                if (response.IsSuccessStatusCode)
                {
                    this.logger.LogInformation(
                        "Webhook sent successfully for job {JobId} to {WebhookUrl}",
                        job.Id,
                        job.WebhookUrl);
                    return;
                }

                this.logger.LogWarning(
                    "Webhook request for job {JobId} returned status {StatusCode}",
                    job.Id,
                    response.StatusCode);
            }
            catch (HttpRequestException ex)
            {
                this.logger.LogWarning(
                    ex,
                    "Webhook request failed for job {JobId} (attempt {Attempt}/{MaxRetries})",
                    job.Id,
                    attempt,
                    this.settings.MaxRetries);
            }
            catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
            {
                this.logger.LogWarning(
                    ex,
                    "Webhook request timed out for job {JobId} (attempt {Attempt}/{MaxRetries})",
                    job.Id,
                    attempt,
                    this.settings.MaxRetries);
            }

            if (attempt < this.settings.MaxRetries)
            {
                await Task.Delay(this.settings.RetryDelayMilliseconds, cancellationToken).ConfigureAwait(false);
            }
        }

        this.logger.LogError(
            "All webhook attempts failed for job {JobId} to {WebhookUrl}",
            job.Id,
            job.WebhookUrl);
    }

    private static WebhookPayload CreatePayload(ConversionJob job)
    {
        return new WebhookPayload
        {
            JobId = job.Id.Value,
            Status = job.Status.ToString(),
            SourceFormat = job.SourceFormat,
            TargetFormat = job.TargetFormat,
            InputFileName = job.InputFileName,
            OutputFileName = job.OutputFileName,
            ErrorMessage = job.ErrorMessage,
            CreatedAt = job.CreatedAt,
            CompletedAt = job.CompletedAt,
            DownloadUrl = $"/api/v1/convert/{job.Id.Value}/download",
        };
    }

    private sealed class WebhookPayload
    {
        public Guid JobId { get; init; }

        public string Status { get; init; } = string.Empty;

        public string SourceFormat { get; init; } = string.Empty;

        public string TargetFormat { get; init; } = string.Empty;

        public string InputFileName { get; init; } = string.Empty;

        public string? OutputFileName { get; init; }

        public string? ErrorMessage { get; init; }

        public DateTimeOffset CreatedAt { get; init; }

        public DateTimeOffset? CompletedAt { get; init; }

        public string DownloadUrl { get; init; } = string.Empty;
    }
}
