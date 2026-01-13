// <copyright file="SetUserAdminCommandHandler.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Application.Interfaces;
using FileConversionApi.Domain.Primitives;
using FileConversionApi.Domain.ValueObjects;

using MediatR;

using Microsoft.Extensions.Logging;

namespace FileConversionApi.Application.Commands.Admin;

/// <summary>
/// Handles the SetUserAdminCommand.
/// </summary>
public class SetUserAdminCommandHandler : IRequestHandler<SetUserAdminCommand, Result<Unit>>
{
    private readonly IUserRepository userRepository;
    private readonly IUnitOfWork unitOfWork;
    private readonly ILogger<SetUserAdminCommandHandler> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SetUserAdminCommandHandler"/> class.
    /// </summary>
    /// <param name="userRepository">The user repository.</param>
    /// <param name="unitOfWork">The unit of work.</param>
    /// <param name="logger">The logger.</param>
    public SetUserAdminCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ILogger<SetUserAdminCommandHandler> logger)
    {
        this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        this.unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<Result<Unit>> Handle(
        SetUserAdminCommand request,
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

        if (request.IsAdmin)
        {
            if (user.IsAdmin)
            {
                return new Error("Admin.UserAlreadyAdmin", "User already has admin privileges.");
            }

            user.GrantAdmin();
            this.logger.LogInformation("Admin privileges granted to user {UserId}", request.UserId);
        }
        else
        {
            if (!user.IsAdmin)
            {
                return new Error("Admin.UserNotAdmin", "User does not have admin privileges.");
            }

            user.RevokeAdmin();
            this.logger.LogInformation("Admin privileges revoked from user {UserId}", request.UserId);
        }

        this.userRepository.Update(user);
        await this.unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Unit.Value;
    }
}
