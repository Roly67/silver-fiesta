// <copyright file="ConvertXlsxToPdfCommandHandler.cs" company="FileConversionApi">
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
/// Handles the convert XLSX to PDF command.
/// </summary>
public class ConvertXlsxToPdfCommandHandler : IRequestHandler<ConvertXlsxToPdfCommand, Result<ConversionJobDto>>
{
    private const string SourceFormat = "xlsx";
    private const string TargetFormat = "pdf";

    private readonly IConversionJobRepository jobRepository;
    private readonly IUnitOfWork unitOfWork;
    private readonly IConverterFactory converterFactory;
    private readonly ICurrentUserService currentUserService;
    private readonly IWebhookService webhookService;
    private readonly IMetricsService metricsService;
    private readonly ILogger<ConvertXlsxToPdfCommandHandler> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConvertXlsxToPdfCommandHandler"/> class.
    /// </summary>
    /// <param name="jobRepository">The job repository.</param>
    /// <param name="unitOfWork">The unit of work.</param>
    /// <param name="converterFactory">The converter factory.</param>
    /// <param name="currentUserService">The current user service.</param>
    /// <param name="webhookService">The webhook service.</param>
    /// <param name="metricsService">The metrics service.</param>
    /// <param name="logger">The logger.</param>
    public ConvertXlsxToPdfCommandHandler(
        IConversionJobRepository jobRepository,
        IUnitOfWork unitOfWork,
        IConverterFactory converterFactory,
        ICurrentUserService currentUserService,
        IWebhookService webhookService,
        IMetricsService metricsService,
        ILogger<ConvertXlsxToPdfCommandHandler> logger)
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
        ConvertXlsxToPdfCommand request,
        CancellationToken cancellationToken)
    {
        var userId = this.currentUserService.UserId;
        if (userId is null)
        {
            return new Error("Auth.Unauthorized", "User is not authenticated.");
        }

        this.logger.LogInformation(
            "Starting XLSX to PDF conversion for user {UserId}",
            userId.Value);

        var converter = this.converterFactory.GetConverter(SourceFormat, TargetFormat);
        if (converter is null)
        {
            return ConversionJobErrors.UnsupportedConversion(SourceFormat, TargetFormat);
        }

        var inputFileName = request.FileName ?? $"spreadsheet_{DateTimeOffset.UtcNow:yyyyMMddHHmmss}.xlsx";
        var job = ConversionJob.Create(userId.Value, SourceFormat, TargetFormat, inputFileName, request.WebhookUrl);

        await this.jobRepository.AddAsync(job, cancellationToken).ConfigureAwait(false);
        await this.unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        job.MarkAsProcessing();
        await this.unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var stopwatch = Stopwatch.StartNew();
        this.metricsService.RecordConversionStarted(SourceFormat, TargetFormat);

        try
        {
            byte[] spreadsheetBytes;
            try
            {
                spreadsheetBytes = Convert.FromBase64String(request.SpreadsheetData ?? string.Empty);
            }
            catch (FormatException)
            {
                stopwatch.Stop();
                this.metricsService.RecordConversionFailed(SourceFormat, TargetFormat);
                var error = new Error("Conversion.InvalidBase64", "Spreadsheet data is not valid base64.");
                job.MarkAsFailed(error.Message);
                await this.unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                await this.webhookService.SendJobCompletedAsync(job, cancellationToken).ConfigureAwait(false);
                return error;
            }

            using var inputStream = new MemoryStream(spreadsheetBytes);

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
            job.MarkAsCompleted(outputFileName, conversionResult.Value);
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
