// <copyright file="GetConversionJobQueryHandler.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Application.DTOs;
using FileConversionApi.Application.Interfaces;
using FileConversionApi.Domain.Errors;
using FileConversionApi.Domain.Primitives;
using FileConversionApi.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FileConversionApi.Application.Queries.Conversion;

/// <summary>
/// Handles the get conversion job query.
/// </summary>
public class GetConversionJobQueryHandler : IRequestHandler<GetConversionJobQuery, Result<ConversionJobDto>>
{
    private readonly IConversionJobRepository jobRepository;
    private readonly ICurrentUserService currentUserService;
    private readonly ILogger<GetConversionJobQueryHandler> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetConversionJobQueryHandler"/> class.
    /// </summary>
    /// <param name="jobRepository">The job repository.</param>
    /// <param name="currentUserService">The current user service.</param>
    /// <param name="logger">The logger.</param>
    public GetConversionJobQueryHandler(
        IConversionJobRepository jobRepository,
        ICurrentUserService currentUserService,
        ILogger<GetConversionJobQueryHandler> logger)
    {
        this.jobRepository = jobRepository ?? throw new ArgumentNullException(nameof(jobRepository));
        this.currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<Result<ConversionJobDto>> Handle(
        GetConversionJobQuery request,
        CancellationToken cancellationToken)
    {
        var userId = this.currentUserService.UserId;
        if (userId is null)
        {
            return new Error("Auth.Unauthorized", "User is not authenticated.");
        }

        this.logger.LogDebug(
            "Getting conversion job {JobId} for user {UserId}",
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

        return ConversionJobDto.FromEntity(job);
    }
}
