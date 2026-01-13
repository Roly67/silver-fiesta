// <copyright file="SplitPdfCommandHandler.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using System.Diagnostics;
using System.IO.Compression;

using FileConversionApi.Application.DTOs;
using FileConversionApi.Application.Interfaces;
using FileConversionApi.Domain.Entities;
using FileConversionApi.Domain.Primitives;

using MediatR;

using Microsoft.Extensions.Logging;

namespace FileConversionApi.Application.Commands.Pdf;

/// <summary>
/// Handles the split PDF command.
/// </summary>
public class SplitPdfCommandHandler : IRequestHandler<SplitPdfCommand, Result<ConversionJobDto>>
{
    private readonly IConversionJobRepository jobRepository;
    private readonly IUnitOfWork unitOfWork;
    private readonly IPdfManipulationService pdfManipulationService;
    private readonly ICurrentUserService currentUserService;
    private readonly IWebhookService webhookService;
    private readonly IMetricsService metricsService;
    private readonly ILogger<SplitPdfCommandHandler> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SplitPdfCommandHandler"/> class.
    /// </summary>
    /// <param name="jobRepository">The job repository.</param>
    /// <param name="unitOfWork">The unit of work.</param>
    /// <param name="pdfManipulationService">The PDF manipulation service.</param>
    /// <param name="currentUserService">The current user service.</param>
    /// <param name="webhookService">The webhook service.</param>
    /// <param name="metricsService">The metrics service.</param>
    /// <param name="logger">The logger.</param>
    public SplitPdfCommandHandler(
        IConversionJobRepository jobRepository,
        IUnitOfWork unitOfWork,
        IPdfManipulationService pdfManipulationService,
        ICurrentUserService currentUserService,
        IWebhookService webhookService,
        IMetricsService metricsService,
        ILogger<SplitPdfCommandHandler> logger)
    {
        this.jobRepository = jobRepository ?? throw new ArgumentNullException(nameof(jobRepository));
        this.unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        this.pdfManipulationService = pdfManipulationService ?? throw new ArgumentNullException(nameof(pdfManipulationService));
        this.currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        this.webhookService = webhookService ?? throw new ArgumentNullException(nameof(webhookService));
        this.metricsService = metricsService ?? throw new ArgumentNullException(nameof(metricsService));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<Result<ConversionJobDto>> Handle(
        SplitPdfCommand request,
        CancellationToken cancellationToken)
    {
        var userId = this.currentUserService.UserId;
        if (userId is null)
        {
            return new Error("Auth.Unauthorized", "User is not authenticated.");
        }

        if (string.IsNullOrWhiteSpace(request.PdfData))
        {
            return new Error("Split.NoPdfData", "PDF data is required for splitting.");
        }

        this.logger.LogInformation(
            "Starting PDF split for user {UserId}",
            userId.Value);

        var inputFileName = request.FileName ?? $"split_{DateTimeOffset.UtcNow:yyyyMMddHHmmss}.pdf";
        var job = ConversionJob.Create(userId.Value, "pdf", "zip", inputFileName, request.WebhookUrl);

        await this.jobRepository.AddAsync(job, cancellationToken).ConfigureAwait(false);
        await this.unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        job.MarkAsProcessing();
        await this.unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var stopwatch = Stopwatch.StartNew();
        this.metricsService.RecordConversionStarted("pdf-split", "zip");

        try
        {
            // Decode base64 PDF
            byte[] pdfBytes;
            try
            {
                pdfBytes = Convert.FromBase64String(request.PdfData);
            }
            catch (FormatException)
            {
                stopwatch.Stop();
                this.metricsService.RecordConversionFailed("pdf-split", "zip");
                var error = new Error("Split.InvalidBase64", "PDF data is not valid base64.");
                job.MarkAsFailed(error.Message);
                await this.unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                await this.webhookService.SendJobCompletedAsync(job, cancellationToken).ConfigureAwait(false);
                return error;
            }

            var options = request.Options ?? new PdfSplitOptions { SplitIntoSinglePages = true };

            var splitResult = await this.pdfManipulationService
                .SplitAsync(pdfBytes, options, cancellationToken)
                .ConfigureAwait(false);

            if (splitResult.IsFailure)
            {
                stopwatch.Stop();
                this.metricsService.RecordConversionFailed("pdf-split", "zip");
                job.MarkAsFailed(splitResult.Error.Message);
                await this.unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                await this.webhookService.SendJobCompletedAsync(job, cancellationToken).ConfigureAwait(false);
                return splitResult.Error;
            }

            // Package split PDFs into a ZIP file
            var zipBytes = CreateZipArchive(splitResult.Value);

            var outputFileName = Path.ChangeExtension(inputFileName, ".zip");
            job.MarkAsCompleted(outputFileName, zipBytes);
            await this.unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            await this.webhookService.SendJobCompletedAsync(job, cancellationToken).ConfigureAwait(false);

            stopwatch.Stop();
            this.metricsService.RecordConversionCompleted("pdf-split", "zip", stopwatch.Elapsed.TotalSeconds);

            this.logger.LogInformation(
                "PDF split job {JobId} completed successfully with {Count} output files",
                job.Id,
                splitResult.Value.Count);

            return ConversionJobDto.FromEntity(job);
        }
        catch (Exception ex) when (ex is not (OutOfMemoryException or StackOverflowException))
        {
            stopwatch.Stop();
            this.metricsService.RecordConversionFailed("pdf-split", "zip");
            this.logger.LogError(ex, "PDF split job {JobId} failed with exception", job.Id);
            job.MarkAsFailed(ex.Message);
            await this.unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            await this.webhookService.SendJobCompletedAsync(job, cancellationToken).ConfigureAwait(false);
            return new Error("Split.Failed", $"PDF split failed: {ex.Message}");
        }
    }

    private static byte[] CreateZipArchive(Dictionary<string, byte[]> files)
    {
        using var memoryStream = new MemoryStream();
        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
        {
            foreach (var (fileName, content) in files)
            {
                var entry = archive.CreateEntry(fileName, CompressionLevel.Optimal);
                using var entryStream = entry.Open();
                entryStream.Write(content, 0, content.Length);
            }
        }

        return memoryStream.ToArray();
    }
}
