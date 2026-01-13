// <copyright file="ConvertDocxToPdfCommandHandler.cs" company="FileConversionApi">
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
/// Handles the convert DOCX to PDF command.
/// </summary>
public class ConvertDocxToPdfCommandHandler : IRequestHandler<ConvertDocxToPdfCommand, Result<ConversionJobDto>>
{
    private const string SourceFormat = "docx";
    private const string TargetFormat = "pdf";

    private readonly IConversionJobRepository jobRepository;
    private readonly IUnitOfWork unitOfWork;
    private readonly IConverterFactory converterFactory;
    private readonly ICurrentUserService currentUserService;
    private readonly IWebhookService webhookService;
    private readonly IMetricsService metricsService;
    private readonly ICloudStorageService cloudStorageService;
    private readonly ILogger<ConvertDocxToPdfCommandHandler> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConvertDocxToPdfCommandHandler"/> class.
    /// </summary>
    /// <param name="jobRepository">The job repository.</param>
    /// <param name="unitOfWork">The unit of work.</param>
    /// <param name="converterFactory">The converter factory.</param>
    /// <param name="currentUserService">The current user service.</param>
    /// <param name="webhookService">The webhook service.</param>
    /// <param name="metricsService">The metrics service.</param>
    /// <param name="cloudStorageService">The cloud storage service.</param>
    /// <param name="logger">The logger.</param>
    public ConvertDocxToPdfCommandHandler(
        IConversionJobRepository jobRepository,
        IUnitOfWork unitOfWork,
        IConverterFactory converterFactory,
        ICurrentUserService currentUserService,
        IWebhookService webhookService,
        IMetricsService metricsService,
        ICloudStorageService cloudStorageService,
        ILogger<ConvertDocxToPdfCommandHandler> logger)
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
        ConvertDocxToPdfCommand request,
        CancellationToken cancellationToken)
    {
        var userId = this.currentUserService.UserId;
        if (userId is null)
        {
            return new Error("Auth.Unauthorized", "User is not authenticated.");
        }

        this.logger.LogInformation(
            "Starting DOCX to PDF conversion for user {UserId}",
            userId.Value);

        var converter = this.converterFactory.GetConverter(SourceFormat, TargetFormat);
        if (converter is null)
        {
            return ConversionJobErrors.UnsupportedConversion(SourceFormat, TargetFormat);
        }

        var inputFileName = request.FileName ?? $"document_{DateTimeOffset.UtcNow:yyyyMMddHHmmss}.docx";
        var job = ConversionJob.Create(userId.Value, SourceFormat, TargetFormat, inputFileName, request.WebhookUrl);

        await this.jobRepository.AddAsync(job, cancellationToken).ConfigureAwait(false);
        await this.unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        job.MarkAsProcessing();
        await this.unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var stopwatch = Stopwatch.StartNew();
        this.metricsService.RecordConversionStarted(SourceFormat, TargetFormat);

        try
        {
            byte[] documentBytes;
            try
            {
                documentBytes = Convert.FromBase64String(request.DocumentData ?? string.Empty);
            }
            catch (FormatException)
            {
                stopwatch.Stop();
                this.metricsService.RecordConversionFailed(SourceFormat, TargetFormat);
                var error = new Error("Conversion.InvalidBase64", "Document data is not valid base64.");
                job.MarkAsFailed(error.Message);
                await this.unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                await this.webhookService.SendJobCompletedAsync(job, cancellationToken).ConfigureAwait(false);
                return error;
            }

            using var inputStream = new MemoryStream(documentBytes);

            var options = request.Options ?? new ConversionOptions();
            var conversionResult = await converter
                .ConvertAsync(inputStream, options, cancellationToken)
                .ConfigureAwait(false);

            if (conversionResult.IsFailure)
            {
                stopwatch.Stop();
                this.metricsService.RecordConversionFailed(SourceFormat, TargetFormat);
                job.MarkAsFailed(conversionResult.Error.Message);
                await this.unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                await this.webhookService.SendJobCompletedAsync(job, cancellationToken).ConfigureAwait(false);
                return conversionResult.Error;
            }

            var outputFileName = Path.ChangeExtension(inputFileName, ".pdf");

            if (this.cloudStorageService.IsEnabled)
            {
                var storageKey = $"{userId.Value.Value}/{job.Id.Value}/{outputFileName}";
                var uploadResult = await this.cloudStorageService
                    .UploadAsync(conversionResult.Value, storageKey, "application/pdf", cancellationToken)
                    .ConfigureAwait(false);

                if (uploadResult.IsFailure)
                {
                    stopwatch.Stop();
                    this.metricsService.RecordConversionFailed(SourceFormat, TargetFormat);
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
            this.metricsService.RecordConversionCompleted(SourceFormat, TargetFormat, stopwatch.Elapsed.TotalSeconds);

            this.logger.LogInformation(
                "Conversion job {JobId} completed successfully",
                job.Id);

            return ConversionJobDto.FromEntity(job);
        }
        catch (Exception ex) when (ex is not (OutOfMemoryException or StackOverflowException))
        {
            stopwatch.Stop();
            this.metricsService.RecordConversionFailed(SourceFormat, TargetFormat);
            this.logger.LogError(ex, "Conversion job {JobId} failed with exception", job.Id);
            job.MarkAsFailed(ex.Message);
            await this.unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            await this.webhookService.SendJobCompletedAsync(job, cancellationToken).ConfigureAwait(false);
            return ConversionJobErrors.ConversionFailed(ex.Message);
        }
    }
}
