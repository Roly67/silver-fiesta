// <copyright file="ResetUserApiKeyCommandHandler.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Application.Interfaces;
using FileConversionApi.Domain.Primitives;
using FileConversionApi.Domain.ValueObjects;

using MediatR;

using Microsoft.Extensions.Logging;

namespace FileConversionApi.Application.Commands.Admin;

/// <summary>
/// Handles the ResetUserApiKeyCommand.
/// </summary>
public class ResetUserApiKeyCommandHandler : IRequestHandler<ResetUserApiKeyCommand, Result<string>>
{
    private readonly IUserRepository userRepository;
    private readonly IUnitOfWork unitOfWork;
    private readonly ILogger<ResetUserApiKeyCommandHandler> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResetUserApiKeyCommandHandler"/> class.
    /// </summary>
    /// <param name="userRepository">The user repository.</param>
    /// <param name="unitOfWork">The unit of work.</param>
    /// <param name="logger">The logger.</param>
    public ResetUserApiKeyCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ILogger<ResetUserApiKeyCommandHandler> logger)
    {
        this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        this.unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<Result<string>> Handle(
        ResetUserApiKeyCommand request,
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

        var newApiKey = user.RegenerateApiKey();
        this.userRepository.Update(user);
        await this.unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        this.logger.LogInformation("API key has been reset for user {UserId}", request.UserId);

        return newApiKey;
    }
}
