// <copyright file="ConvertImageCommandHandler.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using System.Diagnostics;

using FileConversionApi.Application.DTOs;
using FileConversionApi.Application.Interfaces;
using FileConversionApi.Domain.Entities;
using FileConversionApi.Domain.Errors;
using FileConversionApi.Domain.Primitives;

using MediatR;

using Microsoft.Extensions.Logging;

namespace FileConversionApi.Application.Commands.Conversion;

/// <summary>
/// Handles the convert image command.
/// </summary>
public class ConvertImageCommandHandler : IRequestHandler<ConvertImageCommand, Result<ConversionJobDto>>
{
    private static readonly HashSet<string> SupportedFormats = new(StringComparer.OrdinalIgnoreCase)
    {
        "png", "jpeg", "jpg", "webp", "gif", "bmp",
    };

    private readonly IConversionJobRepository jobRepository;
    private readonly IUnitOfWork unitOfWork;
    private readonly IConverterFactory converterFactory;
    private readonly ICurrentUserService currentUserService;
    private readonly IWebhookService webhookService;
    private readonly IMetricsService metricsService;
    private readonly ILogger<ConvertImageCommandHandler> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConvertImageCommandHandler"/> class.
    /// </summary>
    /// <param name="jobRepository">The job repository.</param>
    /// <param name="unitOfWork">The unit of work.</param>
    /// <param name="converterFactory">The converter factory.</param>
    /// <param name="currentUserService">The current user service.</param>
    /// <param name="webhookService">The webhook service.</param>
    /// <param name="metricsService">The metrics service.</param>
    /// <param name="logger">The logger.</param>
    public ConvertImageCommandHandler(
        IConversionJobRepository jobRepository,
        IUnitOfWork unitOfWork,
        IConverterFactory converterFactory,
        ICurrentUserService currentUserService,
        IWebhookService webhookService,
        IMetricsService metricsService,
        ILogger<ConvertImageCommandHandler> logger)
    {
        this.jobRepository = jobRepository ?? throw new ArgumentNullException(nameof(jobRepository));
        this.unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        this.converterFactory = converterFactory ?? throw new ArgumentNullException(nameof(converterFactory));
        this.currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        this.webhookService = webhookService ?? throw new ArgumentNullException(nameof(webhookService));
        this.metricsService = metricsService ?? throw new ArgumentNullException(nameof(metricsService));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<Result<ConversionJobDto>> Handle(
        ConvertImageCommand request,
        CancellationToken cancellationToken)
    {
        var userId = this.currentUserService.UserId;
        if (userId is null)
        {
            return new Error("Auth.Unauthorized", "User is not authenticated.");
        }

        // Validate formats
        var sourceFormat = NormalizeFormat(request.SourceFormat);
        var targetFormat = NormalizeFormat(request.TargetFormat);

        if (string.IsNullOrEmpty(sourceFormat) || !SupportedFormats.Contains(sourceFormat))
        {
            return new Error("Conversion.InvalidSourceFormat", $"Source format '{request.SourceFormat}' is not supported. Supported formats: {string.Join(", ", SupportedFormats)}");
        }

        if (string.IsNullOrEmpty(targetFormat) || !SupportedFormats.Contains(targetFormat))
        {
            return new Error("Conversion.InvalidTargetFormat", $"Target format '{request.TargetFormat}' is not supported. Supported formats: {string.Join(", ", SupportedFormats)}");
        }

        if (sourceFormat.Equals(targetFormat, StringComparison.OrdinalIgnoreCase))
        {
            return new Error("Conversion.SameFormat", "Source and target formats cannot be the same.");
        }

        this.logger.LogInformation(
            "Starting image conversion from {SourceFormat} to {TargetFormat} for user {UserId}",
            sourceFormat,
            targetFormat,
            userId.Value);

        var converter = this.converterFactory.GetConverter(sourceFormat, targetFormat);
        if (converter is null)
        {
            return ConversionJobErrors.UnsupportedConversion(sourceFormat, targetFormat);
        }

        var inputFileName = request.FileName ?? $"image_{DateTimeOffset.UtcNow:yyyyMMddHHmmss}.{sourceFormat}";
        var job = ConversionJob.Create(userId.Value, sourceFormat, targetFormat, inputFileName, request.WebhookUrl);

        await this.jobRepository.AddAsync(job, cancellationToken).ConfigureAwait(false);
        await this.unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        job.MarkAsProcessing();
        await this.unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var stopwatch = Stopwatch.StartNew();
        this.metricsService.RecordConversionStarted(sourceFormat, targetFormat);

        try
        {
            byte[] imageBytes;
            try
            {
                imageBytes = Convert.FromBase64String(request.ImageData ?? string.Empty);
            }
            catch (FormatException)
            {
                stopwatch.Stop();
                this.metricsService.RecordConversionFailed(sourceFormat, targetFormat);
                var error = new Error("Conversion.InvalidBase64", "Image data is not valid base64.");
                job.MarkAsFailed(error.Message);
                await this.unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                await this.webhookService.SendJobCompletedAsync(job, cancellationToken).ConfigureAwait(false);
                return error;
            }

            using var inputStream = new MemoryStream(imageBytes);

            var options = request.Options ?? new ConversionOptions();
            var conversionResult = await converter
                .ConvertAsync(inputStream, options, cancellationToken)
                .ConfigureAwait(false);

            if (conversionResult.IsFailure)
            {
                stopwatch.Stop();
                this.metricsService.RecordConversionFailed(sourceFormat, targetFormat);
                job.MarkAsFailed(conversionResult.Error.Message);
                await this.unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                await this.webhookService.SendJobCompletedAsync(job, cancellationToken).ConfigureAwait(false);
                return conversionResult.Error;
            }

            var outputFileName = Path.ChangeExtension(inputFileName, $".{targetFormat}");
            job.MarkAsCompleted(outputFileName, conversionResult.Value);
            await this.unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            await this.webhookService.SendJobCompletedAsync(job, cancellationToken).ConfigureAwait(false);

            stopwatch.Stop();
            this.metricsService.RecordConversionCompleted(sourceFormat, targetFormat, stopwatch.Elapsed.TotalSeconds);

            this.logger.LogInformation(
                "Conversion job {JobId} completed successfully",
                job.Id);

            return ConversionJobDto.FromEntity(job);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            this.metricsService.RecordConversionFailed(sourceFormat, targetFormat);
            this.logger.LogError(ex, "Conversion job {JobId} failed with exception", job.Id);
            job.MarkAsFailed(ex.Message);
            await this.unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            await this.webhookService.SendJobCompletedAsync(job, cancellationToken).ConfigureAwait(false);
            return ConversionJobErrors.ConversionFailed(ex.Message);
        }
    }

    private static string? NormalizeFormat(string? format)
    {
        if (string.IsNullOrWhiteSpace(format))
        {
            return null;
        }

        // Normalize jpg to jpeg
        return format.Trim().ToLowerInvariant() switch
        {
            "jpg" => "jpeg",
            _ => format.Trim().ToLowerInvariant(),
        };
    }
}
