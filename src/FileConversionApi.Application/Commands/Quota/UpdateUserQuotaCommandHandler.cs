// <copyright file="UpdateUserQuotaCommandHandler.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Application.DTOs;
using FileConversionApi.Application.Interfaces;
using FileConversionApi.Domain.Primitives;

using MediatR;

namespace FileConversionApi.Application.Commands.Quota;

/// <summary>
/// Handles the UpdateUserQuotaCommand.
/// </summary>
public class UpdateUserQuotaCommandHandler : IRequestHandler<UpdateUserQuotaCommand, Result<UsageQuotaDto>>
{
    private readonly IUsageQuotaService quotaService;
    private readonly IUserRepository userRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateUserQuotaCommandHandler"/> class.
    /// </summary>
    /// <param name="quotaService">The quota service.</param>
    /// <param name="userRepository">The user repository.</param>
    public UpdateUserQuotaCommandHandler(
        IUsageQuotaService quotaService,
        IUserRepository userRepository)
    {
        this.quotaService = quotaService ?? throw new ArgumentNullException(nameof(quotaService));
        this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    /// <inheritdoc/>
    public async Task<Result<UsageQuotaDto>> Handle(
        UpdateUserQuotaCommand request,
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

        var updateResult = await this.quotaService
            .UpdateQuotaLimitsAsync(
                userId,
                request.ConversionsLimit,
                request.BytesLimit,
                cancellationToken)
            .ConfigureAwait(false);

        if (updateResult.IsFailure)
        {
            return updateResult.Error;
        }

        return UsageQuotaDto.FromEntity(updateResult.Value);
    }
}
