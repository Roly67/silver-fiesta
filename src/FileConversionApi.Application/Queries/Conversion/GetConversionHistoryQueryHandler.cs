// <copyright file="GetConversionHistoryQueryHandler.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Application.DTOs;
using FileConversionApi.Application.Interfaces;
using FileConversionApi.Domain.Primitives;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FileConversionApi.Application.Queries.Conversion;

/// <summary>
/// Handles the get conversion history query.
/// </summary>
public class GetConversionHistoryQueryHandler
    : IRequestHandler<GetConversionHistoryQuery, Result<PagedResult<ConversionJobDto>>>
{
    private readonly IConversionJobRepository jobRepository;
    private readonly ICurrentUserService currentUserService;
    private readonly ILogger<GetConversionHistoryQueryHandler> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetConversionHistoryQueryHandler"/> class.
    /// </summary>
    /// <param name="jobRepository">The job repository.</param>
    /// <param name="currentUserService">The current user service.</param>
    /// <param name="logger">The logger.</param>
    public GetConversionHistoryQueryHandler(
        IConversionJobRepository jobRepository,
        ICurrentUserService currentUserService,
        ILogger<GetConversionHistoryQueryHandler> logger)
    {
        this.jobRepository = jobRepository ?? throw new ArgumentNullException(nameof(jobRepository));
        this.currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<Result<PagedResult<ConversionJobDto>>> Handle(
        GetConversionHistoryQuery request,
        CancellationToken cancellationToken)
    {
        var userId = this.currentUserService.UserId;
        if (userId is null)
        {
            return new Error("Auth.Unauthorized", "User is not authenticated.");
        }

        this.logger.LogDebug(
            "Getting conversion history for user {UserId}, page {Page}, pageSize {PageSize}",
            userId.Value,
            request.Page,
            request.PageSize);

        var jobs = await this.jobRepository
            .GetByUserIdAsync(userId.Value, request.Page, request.PageSize, cancellationToken)
            .ConfigureAwait(false);

        var totalCount = await this.jobRepository
            .GetCountByUserIdAsync(userId.Value, cancellationToken)
            .ConfigureAwait(false);

        var items = jobs.Select(ConversionJobDto.FromEntity).ToList();

        return new PagedResult<ConversionJobDto>
        {
            Items = items,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount,
        };
    }
}
