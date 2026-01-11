// <copyright file="DownloadConversionResultQueryHandler.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Application.Interfaces;
using FileConversionApi.Domain.Enums;
using FileConversionApi.Domain.Errors;
using FileConversionApi.Domain.Primitives;
using FileConversionApi.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FileConversionApi.Application.Queries.Conversion;

/// <summary>
/// Handles the download conversion result query.
/// </summary>
public class DownloadConversionResultQueryHandler
    : IRequestHandler<DownloadConversionResultQuery, Result<FileDownloadResult>>
{
    private readonly IConversionJobRepository jobRepository;
    private readonly ICurrentUserService currentUserService;
    private readonly ILogger<DownloadConversionResultQueryHandler> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DownloadConversionResultQueryHandler"/> class.
    /// </summary>
    /// <param name="jobRepository">The job repository.</param>
    /// <param name="currentUserService">The current user service.</param>
    /// <param name="logger">The logger.</param>
    public DownloadConversionResultQueryHandler(
        IConversionJobRepository jobRepository,
        ICurrentUserService currentUserService,
        ILogger<DownloadConversionResultQueryHandler> logger)
    {
        this.jobRepository = jobRepository ?? throw new ArgumentNullException(nameof(jobRepository));
        this.currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<Result<FileDownloadResult>> Handle(
        DownloadConversionResultQuery request,
        CancellationToken cancellationToken)
    {
        var userId = this.currentUserService.UserId;
        if (userId is null)
        {
            return new Error("Auth.Unauthorized", "User is not authenticated.");
        }

        this.logger.LogDebug(
            "Downloading conversion result {JobId} for user {UserId}",
            request.JobId,
            userId.Value);

        var jobId = ConversionJobId.From(request.JobId);
        var job = await this.jobRepository
            .GetByIdForUserAsync(jobId, userId.Value, cancellationToken)
            .ConfigureAwait(false);

        if (job is null)
        {
            return ConversionJobErrors.NotFound(request.JobId);
        }

        if (job.Status != ConversionStatus.Completed)
        {
            return ConversionJobErrors.NotCompleted;
        }

        if (job.OutputData is null || job.OutputFileName is null)
        {
            return ConversionJobErrors.NoOutputAvailable;
        }

        var contentType = GetContentType(job.TargetFormat);

        return new FileDownloadResult
        {
            Content = job.OutputData,
            FileName = job.OutputFileName,
            ContentType = contentType,
        };
    }

    private static string GetContentType(string format)
    {
        return format.ToLowerInvariant() switch
        {
            "pdf" => "application/pdf",
            "html" => "text/html",
            "docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            "xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "png" => "image/png",
            "jpg" or "jpeg" => "image/jpeg",
            _ => "application/octet-stream",
        };
    }
}
