// <copyright file="ExtractPdfTextCommandHandler.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using System.Diagnostics;
using System.Text;

using FileConversionApi.Application.DTOs;
using FileConversionApi.Application.Interfaces;
using FileConversionApi.Domain.Entities;
using FileConversionApi.Domain.Primitives;

using MediatR;

using Microsoft.Extensions.Logging;

namespace FileConversionApi.Application.Commands.Pdf;

/// <summary>
/// Handles the extract PDF text command.
/// </summary>
public class ExtractPdfTextCommandHandler : IRequestHandler<ExtractPdfTextCommand, Result<ConversionJobDto>>
{
    private readonly IConversionJobRepository jobRepository;
    private readonly IUnitOfWork unitOfWork;
    private readonly IPdfTextExtractor pdfTextExtractor;
    private readonly ICurrentUserService currentUserService;
    private readonly IWebhookService webhookService;
    private readonly IMetricsService metricsService;
    private readonly ICloudStorageService cloudStorageService;
    private readonly ILogger<ExtractPdfTextCommandHandler> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExtractPdfTextCommandHandler"/> class.
    /// </summary>
    /// <param name="jobRepository">The job repository.</param>
    /// <param name="unitOfWork">The unit of work.</param>
    /// <param name="pdfTextExtractor">The PDF text extractor.</param>
    /// <param name="currentUserService">The current user service.</param>
    /// <param name="webhookService">The webhook service.</param>
    /// <param name="metricsService">The metrics service.</param>
    /// <param name="cloudStorageService">The cloud storage service.</param>
    /// <param name="logger">The logger.</param>
    public ExtractPdfTextCommandHandler(
        IConversionJobRepository jobRepository,
        IUnitOfWork unitOfWork,
        IPdfTextExtractor pdfTextExtractor,
        ICurrentUserService currentUserService,
        IWebhookService webhookService,
        IMetricsService metricsService,
        ICloudStorageService cloudStorageService,
        ILogger<ExtractPdfTextCommandHandler> logger)
    {
        this.jobRepository = jobRepository ?? throw new ArgumentNullException(nameof(jobRepository));
        this.unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        this.pdfTextExtractor = pdfTextExtractor ?? throw new ArgumentNullException(nameof(pdfTextExtractor));
        this.currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        this.webhookService = webhookService ?? throw new ArgumentNullException(nameof(webhookService));
        this.metricsService = metricsService ?? throw new ArgumentNullException(nameof(metricsService));
        this.cloudStorageService = cloudStorageService ?? throw new ArgumentNullException(nameof(cloudStorageService));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<Result<ConversionJobDto>> Handle(
        ExtractPdfTextCommand request,
        CancellationToken cancellationToken)
    {
        var userId = this.currentUserService.UserId;
        if (userId is null)
        {
            return new Error("Auth.Unauthorized", "User is not authenticated.");
        }

        if (string.IsNullOrWhiteSpace(request.PdfData))
        {
            return new Error("Extraction.InvalidInput", "PDF data is required.");
        }

        this.logger.LogInformation(
            "Starting PDF text extraction for user {UserId}",
            userId.Value);

        var inputFileName = request.FileName ?? $"document_{DateTimeOffset.UtcNow:yyyyMMddHHmmss}.pdf";
        var job = ConversionJob.Create(userId.Value, "pdf", "txt", inputFileName, request.WebhookUrl);

        await this.jobRepository.AddAsync(job, cancellationToken).ConfigureAwait(false);
        await this.unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        job.MarkAsProcessing();
        await this.unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var stopwatch = Stopwatch.StartNew();
        this.metricsService.RecordConversionStarted("pdf", "txt");

        try
        {
            var pdfBytes = Convert.FromBase64String(request.PdfData);
            using var inputStream = new MemoryStream(pdfBytes);

            var extractionResult = await this.pdfTextExtractor
                .ExtractTextAsync(inputStream, request.PageNumber, request.Password, cancellationToken)
                .ConfigureAwait(false);

            if (extractionResult.IsFailure)
            {
                stopwatch.Stop();
                this.metricsService.RecordConversionFailed("pdf", "txt");
                job.MarkAsFailed(extractionResult.Error.Message);
                await this.unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                await this.webhookService.SendJobCompletedAsync(job, cancellationToken).ConfigureAwait(false);
                return extractionResult.Error;
            }

            var textBytes = Encoding.UTF8.GetBytes(extractionResult.Value);
            var outputFileName = Path.ChangeExtension(inputFileName, ".txt");

            if (this.cloudStorageService.IsEnabled)
            {
                var storageKey = $"{userId.Value.Value}/{job.Id.Value}/{outputFileName}";
                var uploadResult = await this.cloudStorageService
                    .UploadAsync(textBytes, storageKey, "text/plain; charset=utf-8", cancellationToken)
                    .ConfigureAwait(false);

                if (uploadResult.IsFailure)
                {
                    stopwatch.Stop();
                    this.metricsService.RecordConversionFailed("pdf", "txt");
                    job.MarkAsFailed($"Cloud storage upload failed: {uploadResult.Error.Message}");
                    await this.unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                    await this.webhookService.SendJobCompletedAsync(job, cancellationToken).ConfigureAwait(false);
                    return uploadResult.Error;
                }

                job.MarkAsCompletedWithCloudStorage(outputFileName, storageKey);
            }
            else
            {
                job.MarkAsCompleted(outputFileName, textBytes);
            }

            await this.unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            await this.webhookService.SendJobCompletedAsync(job, cancellationToken).ConfigureAwait(false);

            stopwatch.Stop();
            this.metricsService.RecordConversionCompleted("pdf", "txt", stopwatch.Elapsed.TotalSeconds);

            this.logger.LogInformation(
                "PDF text extraction job {JobId} completed successfully, extracted {CharCount} characters",
                job.Id,
                extractionResult.Value.Length);

            return ConversionJobDto.FromEntity(job);
        }
        catch (FormatException)
        {
            stopwatch.Stop();
            this.metricsService.RecordConversionFailed("pdf", "txt");
            job.MarkAsFailed("Invalid base64 PDF data.");
            await this.unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            await this.webhookService.SendJobCompletedAsync(job, cancellationToken).ConfigureAwait(false);
            return new Error("Extraction.InvalidInput", "Invalid base64 PDF data.");
        }
        catch (Exception ex) when (ex is not (OutOfMemoryException or StackOverflowException))
        {
            stopwatch.Stop();
            this.metricsService.RecordConversionFailed("pdf", "txt");
            this.logger.LogError(ex, "PDF text extraction job {JobId} failed with exception", job.Id);
            job.MarkAsFailed(ex.Message);
            await this.unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            await this.webhookService.SendJobCompletedAsync(job, cancellationToken).ConfigureAwait(false);
            return new Error("Extraction.Failed", $"PDF text extraction failed: {ex.Message}");
        }
    }
}
