// <copyright file="MergePdfsCommandHandler.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using System.Diagnostics;

using FileConversionApi.Application.DTOs;
using FileConversionApi.Application.Interfaces;
using FileConversionApi.Domain.Entities;
using FileConversionApi.Domain.Primitives;

using MediatR;

using Microsoft.Extensions.Logging;

namespace FileConversionApi.Application.Commands.Pdf;

/// <summary>
/// Handles the merge PDFs command.
/// </summary>
public class MergePdfsCommandHandler : IRequestHandler<MergePdfsCommand, Result<ConversionJobDto>>
{
    private readonly IConversionJobRepository jobRepository;
    private readonly IUnitOfWork unitOfWork;
    private readonly IPdfManipulationService pdfManipulationService;
    private readonly ICurrentUserService currentUserService;
    private readonly IWebhookService webhookService;
    private readonly IMetricsService metricsService;
    private readonly ICloudStorageService cloudStorageService;
    private readonly ILogger<MergePdfsCommandHandler> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MergePdfsCommandHandler"/> class.
    /// </summary>
    /// <param name="jobRepository">The job repository.</param>
    /// <param name="unitOfWork">The unit of work.</param>
    /// <param name="pdfManipulationService">The PDF manipulation service.</param>
    /// <param name="currentUserService">The current user service.</param>
    /// <param name="webhookService">The webhook service.</param>
    /// <param name="metricsService">The metrics service.</param>
    /// <param name="cloudStorageService">The cloud storage service.</param>
    /// <param name="logger">The logger.</param>
    public MergePdfsCommandHandler(
        IConversionJobRepository jobRepository,
        IUnitOfWork unitOfWork,
        IPdfManipulationService pdfManipulationService,
        ICurrentUserService currentUserService,
        IWebhookService webhookService,
        IMetricsService metricsService,
        ICloudStorageService cloudStorageService,
        ILogger<MergePdfsCommandHandler> logger)
    {
        this.jobRepository = jobRepository ?? throw new ArgumentNullException(nameof(jobRepository));
        this.unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        this.pdfManipulationService = pdfManipulationService ?? throw new ArgumentNullException(nameof(pdfManipulationService));
        this.currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        this.webhookService = webhookService ?? throw new ArgumentNullException(nameof(webhookService));
        this.metricsService = metricsService ?? throw new ArgumentNullException(nameof(metricsService));
        this.cloudStorageService = cloudStorageService ?? throw new ArgumentNullException(nameof(cloudStorageService));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<Result<ConversionJobDto>> Handle(
        MergePdfsCommand request,
        CancellationToken cancellationToken)
    {
        var userId = this.currentUserService.UserId;
        if (userId is null)
        {
            return new Error("Auth.Unauthorized", "User is not authenticated.");
        }

        if (request.PdfDocuments is null || request.PdfDocuments.Length < 2)
        {
            return new Error("Merge.InsufficientDocuments", "At least two PDF documents are required for merging.");
        }

        this.logger.LogInformation(
            "Starting PDF merge of {Count} documents for user {UserId}",
            request.PdfDocuments.Length,
            userId.Value);

        var inputFileName = request.FileName ?? $"merged_{DateTimeOffset.UtcNow:yyyyMMddHHmmss}.pdf";
        var job = ConversionJob.Create(userId.Value, "pdf", "pdf", inputFileName, request.WebhookUrl);

        await this.jobRepository.AddAsync(job, cancellationToken).ConfigureAwait(false);
        await this.unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        job.MarkAsProcessing();
        await this.unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var stopwatch = Stopwatch.StartNew();
        this.metricsService.RecordConversionStarted("pdf-merge", "pdf");

        try
        {
            // Decode base64 PDFs
            var pdfBytesList = new List<byte[]>();
            for (var i = 0; i < request.PdfDocuments.Length; i++)
            {
                try
                {
                    var pdfBytes = Convert.FromBase64String(request.PdfDocuments[i]);
                    pdfBytesList.Add(pdfBytes);
                }
                catch (FormatException)
                {
                    stopwatch.Stop();
                    this.metricsService.RecordConversionFailed("pdf-merge", "pdf");
                    var error = new Error("Merge.InvalidBase64", $"PDF document at index {i} is not valid base64.");
                    job.MarkAsFailed(error.Message);
                    await this.unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                    await this.webhookService.SendJobCompletedAsync(job, cancellationToken).ConfigureAwait(false);
                    return error;
                }
            }

            var mergeResult = await this.pdfManipulationService
                .MergeAsync(pdfBytesList, cancellationToken)
                .ConfigureAwait(false);

            if (mergeResult.IsFailure)
            {
                stopwatch.Stop();
                this.metricsService.RecordConversionFailed("pdf-merge", "pdf");
                job.MarkAsFailed(mergeResult.Error.Message);
                await this.unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                await this.webhookService.SendJobCompletedAsync(job, cancellationToken).ConfigureAwait(false);
                return mergeResult.Error;
            }

            var outputFileName = inputFileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase)
                ? inputFileName
                : $"{inputFileName}.pdf";

            if (this.cloudStorageService.IsEnabled)
            {
                var storageKey = $"{userId.Value.Value}/{job.Id.Value}/{outputFileName}";
                var uploadResult = await this.cloudStorageService
                    .UploadAsync(mergeResult.Value, storageKey, "application/pdf", cancellationToken)
                    .ConfigureAwait(false);

                if (uploadResult.IsFailure)
                {
                    stopwatch.Stop();
                    this.metricsService.RecordConversionFailed("pdf-merge", "pdf");
                    job.MarkAsFailed($"Cloud storage upload failed: {uploadResult.Error.Message}");
                    await this.unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                    await this.webhookService.SendJobCompletedAsync(job, cancellationToken).ConfigureAwait(false);
                    return uploadResult.Error;
                }

                job.MarkAsCompletedWithCloudStorage(outputFileName, storageKey);
            }
            else
            {
                job.MarkAsCompleted(outputFileName, mergeResult.Value);
            }

            await this.unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            await this.webhookService.SendJobCompletedAsync(job, cancellationToken).ConfigureAwait(false);

            stopwatch.Stop();
            this.metricsService.RecordConversionCompleted("pdf-merge", "pdf", stopwatch.Elapsed.TotalSeconds);

            this.logger.LogInformation(
                "PDF merge job {JobId} completed successfully",
                job.Id);

            return ConversionJobDto.FromEntity(job);
        }
        catch (Exception ex) when (ex is not (OutOfMemoryException or StackOverflowException))
        {
            stopwatch.Stop();
            this.metricsService.RecordConversionFailed("pdf-merge", "pdf");
            this.logger.LogError(ex, "PDF merge job {JobId} failed with exception", job.Id);
            job.MarkAsFailed(ex.Message);
            await this.unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            await this.webhookService.SendJobCompletedAsync(job, cancellationToken).ConfigureAwait(false);
            return new Error("Merge.Failed", $"PDF merge failed: {ex.Message}");
        }
    }
}
