// <copyright file="ConvertPdfToImageCommandHandler.cs" company="FileConversionApi">
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
/// Handles the convert PDF to image command.
/// </summary>
public class ConvertPdfToImageCommandHandler : IRequestHandler<ConvertPdfToImageCommand, Result<ConversionJobDto>>
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
    private readonly ILogger<ConvertPdfToImageCommandHandler> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConvertPdfToImageCommandHandler"/> class.
    /// </summary>
    /// <param name="jobRepository">The job repository.</param>
    /// <param name="unitOfWork">The unit of work.</param>
    /// <param name="converterFactory">The converter factory.</param>
    /// <param name="currentUserService">The current user service.</param>
    /// <param name="webhookService">The webhook service.</param>
    /// <param name="metricsService">The metrics service.</param>
    /// <param name="cloudStorageService">The cloud storage service.</param>
    /// <param name="logger">The logger.</param>
    public ConvertPdfToImageCommandHandler(
        IConversionJobRepository jobRepository,
        IUnitOfWork unitOfWork,
        IConverterFactory converterFactory,
        ICurrentUserService currentUserService,
        IWebhookService webhookService,
        IMetricsService metricsService,
        ICloudStorageService cloudStorageService,
        ILogger<ConvertPdfToImageCommandHandler> logger)
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
        ConvertPdfToImageCommand request,
        CancellationToken cancellationToken)
    {
        var userId = this.currentUserService.UserId;
        if (userId is null)
        {
            return new Error("Auth.Unauthorized", "User is not authenticated.");
        }

        if (string.IsNullOrWhiteSpace(request.PdfData))
        {
            return new Error("Conversion.InvalidInput", "PDF data is required.");
        }

        var targetFormat = NormalizeFormat(request.TargetFormat);
        if (!SupportedFormats.Contains(targetFormat))
        {
            return new Error(
                "Conversion.UnsupportedFormat",
                $"Unsupported target format '{request.TargetFormat}'. Supported formats: png, jpeg, webp.");
        }

        this.logger.LogInformation(
            "Starting PDF to {Format} conversion for user {UserId}",
            targetFormat.ToUpperInvariant(),
            userId.Value);

        var converter = this.converterFactory.GetConverter("pdf", targetFormat);
        if (converter is null)
        {
            return ConversionJobErrors.UnsupportedConversion("pdf", targetFormat);
        }

        var inputFileName = request.FileName ?? $"document_{DateTimeOffset.UtcNow:yyyyMMddHHmmss}.pdf";
        var job = ConversionJob.Create(userId.Value, "pdf", targetFormat, inputFileName, request.WebhookUrl);

        await this.jobRepository.AddAsync(job, cancellationToken).ConfigureAwait(false);
        await this.unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        // Process conversion synchronously for now (could be moved to background job)
        job.MarkAsProcessing();
        await this.unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var stopwatch = Stopwatch.StartNew();
        this.metricsService.RecordConversionStarted("pdf", targetFormat);

        try
        {
            var pdfBytes = Convert.FromBase64String(request.PdfData);
            using var inputStream = new MemoryStream(pdfBytes);

            var options = request.Options ?? new ConversionOptions();
            var conversionResult = await converter
                .ConvertAsync(inputStream, options, cancellationToken)
                .ConfigureAwait(false);

            if (conversionResult.IsFailure)
            {
                stopwatch.Stop();
                this.metricsService.RecordConversionFailed("pdf", targetFormat);
                job.MarkAsFailed(conversionResult.Error.Message);
                await this.unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                await this.webhookService.SendJobCompletedAsync(job, cancellationToken).ConfigureAwait(false);
                return conversionResult.Error;
            }

            // Determine output file extension based on result
            // If multiple pages, result is a ZIP file
            var isMultiPage = !options.PageNumber.HasValue && conversionResult.Value.Length > 0 &&
                              IsZipFile(conversionResult.Value);
            var outputExtension = isMultiPage ? "zip" : targetFormat;
            var outputFileName = Path.ChangeExtension(inputFileName, $".{outputExtension}");
            var contentType = GetContentType(outputExtension);

            if (this.cloudStorageService.IsEnabled)
            {
                var storageKey = $"{userId.Value.Value}/{job.Id.Value}/{outputFileName}";
                var uploadResult = await this.cloudStorageService
                    .UploadAsync(conversionResult.Value, storageKey, contentType, cancellationToken)
                    .ConfigureAwait(false);

                if (uploadResult.IsFailure)
                {
                    stopwatch.Stop();
                    this.metricsService.RecordConversionFailed("pdf", targetFormat);
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
            this.metricsService.RecordConversionCompleted("pdf", targetFormat, stopwatch.Elapsed.TotalSeconds);

            this.logger.LogInformation(
                "Conversion job {JobId} completed successfully",
                job.Id);

            return ConversionJobDto.FromEntity(job);
        }
        catch (FormatException)
        {
            stopwatch.Stop();
            this.metricsService.RecordConversionFailed("pdf", targetFormat);
            job.MarkAsFailed("Invalid base64 PDF data.");
            await this.unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            await this.webhookService.SendJobCompletedAsync(job, cancellationToken).ConfigureAwait(false);
            return new Error("Conversion.InvalidInput", "Invalid base64 PDF data.");
        }
        catch (Exception ex) when (ex is not (OutOfMemoryException or StackOverflowException))
        {
            stopwatch.Stop();
            this.metricsService.RecordConversionFailed("pdf", targetFormat);
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

    private static bool IsZipFile(byte[] data)
    {
        // ZIP files start with PK (0x50 0x4B)
        return data.Length >= 2 && data[0] == 0x50 && data[1] == 0x4B;
    }

    private static string GetContentType(string format)
    {
        return format switch
        {
            "jpeg" or "jpg" => "image/jpeg",
            "webp" => "image/webp",
            "zip" => "application/zip",
            _ => "image/png",
        };
    }
}
