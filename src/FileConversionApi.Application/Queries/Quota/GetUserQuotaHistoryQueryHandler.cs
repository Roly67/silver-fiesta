// <copyright file="GetUserQuotaHistoryQueryHandler.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Application.DTOs;
using FileConversionApi.Application.Interfaces;
using FileConversionApi.Domain.Primitives;

using MediatR;

namespace FileConversionApi.Application.Queries.Quota;

/// <summary>
/// Handles the GetUserQuotaHistoryQuery.
/// </summary>
public class GetUserQuotaHistoryQueryHandler : IRequestHandler<GetUserQuotaHistoryQuery, Result<IReadOnlyList<UsageQuotaDto>>>
{
    private readonly IUsageQuotaService quotaService;
    private readonly IUserRepository userRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetUserQuotaHistoryQueryHandler"/> class.
    /// </summary>
    /// <param name="quotaService">The quota service.</param>
    /// <param name="userRepository">The user repository.</param>
    public GetUserQuotaHistoryQueryHandler(
        IUsageQuotaService quotaService,
        IUserRepository userRepository)
    {
        this.quotaService = quotaService ?? throw new ArgumentNullException(nameof(quotaService));
        this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<UsageQuotaDto>>> Handle(
        GetUserQuotaHistoryQuery request,
        CancellationToken cancellationToken)
    {
        // Verify user exists
        var userId = new Domain.ValueObjects.UserId(request.UserId);
        var user = await this.userRepository
            .GetByIdAsync(userId, cancellationToken)
            .ConfigureAwait(false);

        if (user is null)
        {
            return Error.NotFound("Admin.UserNotFound", $"User with ID '{request.UserId}' was not found.");
        }

        var historyResult = await this.quotaService
            .GetQuotaHistoryAsync(userId, request.Months, cancellationToken)
            .ConfigureAwait(false);

        if (historyResult.IsFailure)
        {
            return historyResult.Error;
        }

        var dtos = historyResult.Value
            .Select(UsageQuotaDto.FromEntity)
            .ToList();

        return dtos;
    }
}
