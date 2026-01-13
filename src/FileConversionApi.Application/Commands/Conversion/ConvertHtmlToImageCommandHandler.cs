// <copyright file="ConvertHtmlToImageCommandHandler.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using System.Diagnostics;
using System.Text;

using FileConversionApi.Application.DTOs;
using FileConversionApi.Application.Interfaces;
using FileConversionApi.Domain.Entities;
using FileConversionApi.Domain.Errors;
using FileConversionApi.Domain.Primitives;

using MediatR;

using Microsoft.Extensions.Logging;

namespace FileConversionApi.Application.Commands.Conversion;

/// <summary>
/// Handles the convert HTML to image command.
/// </summary>
public class ConvertHtmlToImageCommandHandler : IRequestHandler<ConvertHtmlToImageCommand, Result<ConversionJobDto>>
{
    private static readonly HashSet<string> SupportedFormats = new(StringComparer.OrdinalIgnoreCase)
    {
        "png",
        "jpeg",
        "jpg",
        "webp",
    };

    private readonly IConversionJobRepository jobRepository;
    private readonly IUnitOfWork unitOfWork;
    private readonly IConverterFactory converterFactory;
    private readonly ICurrentUserService currentUserService;
    private readonly IWebhookService webhookService;
    private readonly IMetricsService metricsService;
    private readonly ICloudStorageService cloudStorageService;
    private readonly ILogger<ConvertHtmlToImageCommandHandler> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConvertHtmlToImageCommandHandler"/> class.
    /// </summary>
    /// <param name="jobRepository">The job repository.</param>
    /// <param name="unitOfWork">The unit of work.</param>
    /// <param name="converterFactory">The converter factory.</param>
    /// <param name="currentUserService">The current user service.</param>
    /// <param name="webhookService">The webhook service.</param>
    /// <param name="metricsService">The metrics service.</param>
    /// <param name="cloudStorageService">The cloud storage service.</param>
    /// <param name="logger">The logger.</param>
    public ConvertHtmlToImageCommandHandler(
        IConversionJobRepository jobRepository,
        IUnitOfWork unitOfWork,
        IConverterFactory converterFactory,
        ICurrentUserService currentUserService,
        IWebhookService webhookService,
        IMetricsService metricsService,
        ICloudStorageService cloudStorageService,
        ILogger<ConvertHtmlToImageCommandHandler> logger)
    {
        this.jobRepository = jobRepository ?? throw new ArgumentNullException(nameof(jobRepository));
        this.unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        this.converterFactory = converterFactory ?? throw new ArgumentNullException(nameof(converterFactory));
        this.currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        this.webhookService = webhookService ?? throw new ArgumentNullException(nameof(webhookService));
        this.metricsService = metricsService ?? throw new ArgumentNullException(nameof(metricsService));
        this.cloudStorageService = cloudStorageService ?? throw new ArgumentNullException(nameof(cloudStorageService));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<Result<ConversionJobDto>> Handle(
        ConvertHtmlToImageCommand request,
        CancellationToken cancellationToken)
    {
        var userId = this.currentUserService.UserId;
        if (userId is null)
        {
            return new Error("Auth.Unauthorized", "User is not authenticated.");
        }

        var targetFormat = NormalizeFormat(request.TargetFormat);
        if (!SupportedFormats.Contains(targetFormat))
        {
            return new Error(
                "Conversion.UnsupportedFormat",
                $"Unsupported target format '{request.TargetFormat}'. Supported formats: png, jpeg, webp.");
        }

        this.logger.LogInformation(
            "Starting HTML to {Format} conversion for user {UserId}",
            targetFormat.ToUpperInvariant(),
            userId.Value);

        var converter = this.converterFactory.GetConverter("html", targetFormat);
        if (converter is null)
        {
            return ConversionJobErrors.UnsupportedConversion("html", targetFormat);
        }

        var inputFileName = request.FileName ?? $"screenshot_{DateTimeOffset.UtcNow:yyyyMMddHHmmss}.html";
        var job = ConversionJob.Create(userId.Value, "html", targetFormat, inputFileName, request.WebhookUrl);

        await this.jobRepository.AddAsync(job, cancellationToken).ConfigureAwait(false);
        await this.unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        // Process conversion synchronously for now (could be moved to background job)
        job.MarkAsProcessing();
        await this.unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var stopwatch = Stopwatch.StartNew();
        this.metricsService.RecordConversionStarted("html", targetFormat);

        try
        {
            var content = request.HtmlContent ?? request.Url ?? string.Empty;
            using var inputStream = new MemoryStream(Encoding.UTF8.GetBytes(content));

            var options = request.Options ?? new ConversionOptions();
            var conversionResult = await converter
                .ConvertAsync(inputStream, options, cancellationToken)
                .ConfigureAwait(false);

            if (conversionResult.IsFailure)
            {
                stopwatch.Stop();
                this.metricsService.RecordConversionFailed("html", targetFormat);
                job.MarkAsFailed(conversionResult.Error.Message);
                await this.unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                await this.webhookService.SendJobCompletedAsync(job, cancellationToken).ConfigureAwait(false);
                return conversionResult.Error;
            }

            var outputFileName = Path.ChangeExtension(inputFileName, $".{targetFormat}");
            var contentType = GetContentType(targetFormat);

            if (this.cloudStorageService.IsEnabled)
            {
                var storageKey = $"{userId.Value.Value}/{job.Id.Value}/{outputFileName}";
                var uploadResult = await this.cloudStorageService
                    .UploadAsync(conversionResult.Value, storageKey, contentType, cancellationToken)
                    .ConfigureAwait(false);

                if (uploadResult.IsFailure)
                {
                    stopwatch.Stop();
                    this.metricsService.RecordConversionFailed("html", targetFormat);
                    job.MarkAsFailed($"Cloud storage upload failed: {uploadResult.Error.Message}");
                    await this.unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                    await this.webhookService.SendJobCompletedAsync(job, cancellationToken).ConfigureAwait(false);
                    return uploadResult.Error;
                }

                job.MarkAsCompletedWithCloudStorage(outputFileName, storageKey);
            }
            else
            {
                job.MarkAsCompleted(outputFileName, conversionResult.Value);
            }

            await this.unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            await this.webhookService.SendJobCompletedAsync(job, cancellationToken).ConfigureAwait(false);

            stopwatch.Stop();
            this.metricsService.RecordConversionCompleted("html", targetFormat, stopwatch.Elapsed.TotalSeconds);

            this.logger.LogInformation(
                "Conversion job {JobId} completed successfully",
                job.Id);

            return ConversionJobDto.FromEntity(job);
        }
        catch (Exception ex) when (ex is not (OutOfMemoryException or StackOverflowException))
        {
            stopwatch.Stop();
            this.metricsService.RecordConversionFailed("html", targetFormat);
            this.logger.LogError(ex, "Conversion job {JobId} failed with exception", job.Id);
            job.MarkAsFailed(ex.Message);
            await this.unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            await this.webhookService.SendJobCompletedAsync(job, cancellationToken).ConfigureAwait(false);
            return ConversionJobErrors.ConversionFailed(ex.Message);
        }
    }

    private static string NormalizeFormat(string format)
    {
        var normalized = format.ToLowerInvariant();
        return normalized == "jpg" ? "jpeg" : normalized;
    }

    private static string GetContentType(string format)
    {
        return format switch
        {
            "jpeg" or "jpg" => "image/jpeg",
            "webp" => "image/webp",
            _ => "image/png",
        };
    }
}
