// <copyright file="GetCurrentQuotaQueryHandler.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Application.DTOs;
using FileConversionApi.Application.Interfaces;
using FileConversionApi.Domain.Primitives;

using MediatR;

namespace FileConversionApi.Application.Queries.Quota;

/// <summary>
/// Handles the GetCurrentQuotaQuery.
/// </summary>
public class GetCurrentQuotaQueryHandler : IRequestHandler<GetCurrentQuotaQuery, Result<UsageQuotaDto>>
{
    private readonly IUsageQuotaService quotaService;
    private readonly ICurrentUserService currentUserService;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetCurrentQuotaQueryHandler"/> class.
    /// </summary>
    /// <param name="quotaService">The quota service.</param>
    /// <param name="currentUserService">The current user service.</param>
    public GetCurrentQuotaQueryHandler(
        IUsageQuotaService quotaService,
        ICurrentUserService currentUserService)
    {
        this.quotaService = quotaService ?? throw new ArgumentNullException(nameof(quotaService));
        this.currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
    }

    /// <inheritdoc/>
    public async Task<Result<UsageQuotaDto>> Handle(
        GetCurrentQuotaQuery request,
        CancellationToken cancellationToken)
    {
        var userId = this.currentUserService.UserId;
        if (userId is null)
        {
            return Error.Unauthorized("User is not authenticated.");
        }

        var quotaResult = await this.quotaService
            .GetCurrentQuotaAsync(userId.Value, cancellationToken)
            .ConfigureAwait(false);

        if (quotaResult.IsFailure)
        {
            return quotaResult.Error;
        }

        return UsageQuotaDto.FromEntity(quotaResult.Value);
    }
}
