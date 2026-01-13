// <copyright file="EnableUserCommandHandler.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Application.Interfaces;
using FileConversionApi.Domain.Primitives;
using FileConversionApi.Domain.ValueObjects;

using MediatR;

using Microsoft.Extensions.Logging;

namespace FileConversionApi.Application.Commands.Admin;

/// <summary>
/// Handles the EnableUserCommand.
/// </summary>
public class EnableUserCommandHandler : IRequestHandler<EnableUserCommand, Result<Unit>>
{
    private readonly IUserRepository userRepository;
    private readonly IUnitOfWork unitOfWork;
    private readonly ILogger<EnableUserCommandHandler> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="EnableUserCommandHandler"/> class.
    /// </summary>
    /// <param name="userRepository">The user repository.</param>
    /// <param name="unitOfWork">The unit of work.</param>
    /// <param name="logger">The logger.</param>
    public EnableUserCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ILogger<EnableUserCommandHandler> logger)
    {
        this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        this.unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<Result<Unit>> Handle(
        EnableUserCommand request,
        CancellationToken cancellationToken)
    {
        var userId = new UserId(request.UserId);
        var user = await this.userRepository
            .GetByIdAsync(userId, cancellationToken)
            .ConfigureAwait(false);

        if (user is null)
        {
            return new Error("Admin.UserNotFound", $"User with ID '{request.UserId}' was not found.");
        }

        if (user.IsActive)
        {
            return new Error("Admin.UserAlreadyEnabled", "User is already enabled.");
        }

        user.Activate();
        this.userRepository.Update(user);
        await this.unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        this.logger.LogInformation("User {UserId} has been enabled", request.UserId);

        return Unit.Value;
    }
}
