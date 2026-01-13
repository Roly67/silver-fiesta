// <copyright file="GetUserRateLimitSettingsQueryHandler.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Application.DTOs;
using FileConversionApi.Application.Interfaces;
using FileConversionApi.Domain.Primitives;
using FileConversionApi.Domain.ValueObjects;

using MediatR;

namespace FileConversionApi.Application.Queries.RateLimit;

/// <summary>
/// Handles the GetUserRateLimitSettingsQuery.
/// </summary>
public class GetUserRateLimitSettingsQueryHandler
    : IRequestHandler<GetUserRateLimitSettingsQuery, Result<UserRateLimitSettingsDto>>
{
    private readonly IUserRateLimitService rateLimitService;
    private readonly IUserRepository userRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetUserRateLimitSettingsQueryHandler"/> class.
    /// </summary>
    /// <param name="rateLimitService">The rate limit service.</param>
    /// <param name="userRepository">The user repository.</param>
    public GetUserRateLimitSettingsQueryHandler(
        IUserRateLimitService rateLimitService,
        IUserRepository userRepository)
    {
        this.rateLimitService = rateLimitService ?? throw new ArgumentNullException(nameof(rateLimitService));
        this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    /// <inheritdoc/>
    public async Task<Result<UserRateLimitSettingsDto>> Handle(
        GetUserRateLimitSettingsQuery request,
        CancellationToken cancellationToken)
    {
        // Verify user exists
        var userId = new UserId(request.UserId);
        var user = await this.userRepository
            .GetByIdAsync(userId, cancellationToken)
            .ConfigureAwait(false);

        if (user is null)
        {
            return Error.NotFound("Admin.UserNotFound", $"User with ID '{request.UserId}' was not found.");
        }

        // Get or create settings
        var userSettings = await this.rateLimitService
            .GetOrCreateSettingsAsync(userId, cancellationToken)
            .ConfigureAwait(false);

        // Get effective limits via service
        var standardLimits = await this.rateLimitService
            .GetEffectiveLimitsAsync(userId, "standard", cancellationToken)
            .ConfigureAwait(false);

        var conversionLimits = await this.rateLimitService
            .GetEffectiveLimitsAsync(userId, "conversion", cancellationToken)
            .ConfigureAwait(false);

        var standardEffective = (
            PermitLimit: standardLimits.PermitLimit,
            WindowMinutes: (int)standardLimits.Window.TotalMinutes);

        var conversionEffective = (
            PermitLimit: conversionLimits.PermitLimit,
            WindowMinutes: (int)conversionLimits.Window.TotalMinutes);

        return UserRateLimitSettingsDto.FromEntity(userSettings, standardEffective, conversionEffective);
    }
}
