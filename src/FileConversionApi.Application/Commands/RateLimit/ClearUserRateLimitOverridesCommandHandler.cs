// <copyright file="ClearUserRateLimitOverridesCommandHandler.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Application.Interfaces;
using FileConversionApi.Domain.Primitives;
using FileConversionApi.Domain.ValueObjects;

using MediatR;

namespace FileConversionApi.Application.Commands.RateLimit;

/// <summary>
/// Handles the ClearUserRateLimitOverridesCommand.
/// </summary>
public class ClearUserRateLimitOverridesCommandHandler : IRequestHandler<ClearUserRateLimitOverridesCommand, Result>
{
    private readonly IUserRateLimitService rateLimitService;
    private readonly IUserRepository userRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="ClearUserRateLimitOverridesCommandHandler"/> class.
    /// </summary>
    /// <param name="rateLimitService">The rate limit service.</param>
    /// <param name="userRepository">The user repository.</param>
    public ClearUserRateLimitOverridesCommandHandler(
        IUserRateLimitService rateLimitService,
        IUserRepository userRepository)
    {
        this.rateLimitService = rateLimitService ?? throw new ArgumentNullException(nameof(rateLimitService));
        this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    /// <inheritdoc/>
    public async Task<Result> Handle(
        ClearUserRateLimitOverridesCommand request,
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
            .ClearOverridesAsync(userId, cancellationToken)
            .ConfigureAwait(false);
    }
}
