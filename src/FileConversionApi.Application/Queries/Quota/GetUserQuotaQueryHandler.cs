// <copyright file="GetUserQuotaQueryHandler.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Application.DTOs;
using FileConversionApi.Application.Interfaces;
using FileConversionApi.Domain.Primitives;

using MediatR;

namespace FileConversionApi.Application.Queries.Quota;

/// <summary>
/// Handles the GetUserQuotaQuery.
/// </summary>
public class GetUserQuotaQueryHandler : IRequestHandler<GetUserQuotaQuery, Result<UsageQuotaDto>>
{
    private readonly IUsageQuotaService quotaService;
    private readonly IUserRepository userRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetUserQuotaQueryHandler"/> class.
    /// </summary>
    /// <param name="quotaService">The quota service.</param>
    /// <param name="userRepository">The user repository.</param>
    public GetUserQuotaQueryHandler(
        IUsageQuotaService quotaService,
        IUserRepository userRepository)
    {
        this.quotaService = quotaService ?? throw new ArgumentNullException(nameof(quotaService));
        this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    /// <inheritdoc/>
    public async Task<Result<UsageQuotaDto>> Handle(
        GetUserQuotaQuery request,
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

        var quotaResult = await this.quotaService
            .GetCurrentQuotaAsync(userId, cancellationToken)
            .ConfigureAwait(false);

        if (quotaResult.IsFailure)
        {
            return quotaResult.Error;
        }

        return UsageQuotaDto.FromEntity(quotaResult.Value);
    }
}
