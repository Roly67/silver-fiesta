// <copyright file="SetUserRateLimitTierCommandHandler.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Application.Interfaces;
using FileConversionApi.Domain.Primitives;
using FileConversionApi.Domain.ValueObjects;

using MediatR;

namespace FileConversionApi.Application.Commands.RateLimit;

/// <summary>
/// Handles the SetUserRateLimitTierCommand.
/// </summary>
public class SetUserRateLimitTierCommandHandler : IRequestHandler<SetUserRateLimitTierCommand, Result>
{
    private readonly IUserRateLimitService rateLimitService;
    private readonly IUserRepository userRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="SetUserRateLimitTierCommandHandler"/> class.
    /// </summary>
    /// <param name="rateLimitService">The rate limit service.</param>
    /// <param name="userRepository">The user repository.</param>
    public SetUserRateLimitTierCommandHandler(
        IUserRateLimitService rateLimitService,
        IUserRepository userRepository)
    {
        this.rateLimitService = rateLimitService ?? throw new ArgumentNullException(nameof(rateLimitService));
        this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    /// <inheritdoc/>
    public async Task<Result> Handle(
        SetUserRateLimitTierCommand request,
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

        return await this.rateLimitService
            .UpdateTierAsync(userId, request.Tier, cancellationToken)
            .ConfigureAwait(false);
    }
}
