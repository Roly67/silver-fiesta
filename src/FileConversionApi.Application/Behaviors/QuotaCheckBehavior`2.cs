// <copyright file="QuotaCheckBehavior`2.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Application.Commands.Conversion;
using FileConversionApi.Application.Interfaces;
using FileConversionApi.Domain.Primitives;

using MediatR;

using Microsoft.Extensions.Logging;

namespace FileConversionApi.Application.Behaviors;

/// <summary>
/// Pipeline behavior that checks usage quotas before processing conversion commands.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public class QuotaCheckBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IConversionCommand
{
    private readonly IUsageQuotaService quotaService;
    private readonly ICurrentUserService currentUserService;
    private readonly ILogger<QuotaCheckBehavior<TRequest, TResponse>> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="QuotaCheckBehavior{TRequest, TResponse}"/> class.
    /// </summary>
    /// <param name="quotaService">The quota service.</param>
    /// <param name="currentUserService">The current user service.</param>
    /// <param name="logger">The logger.</param>
    public QuotaCheckBehavior(
        IUsageQuotaService quotaService,
        ICurrentUserService currentUserService,
        ILogger<QuotaCheckBehavior<TRequest, TResponse>> logger)
    {
        this.quotaService = quotaService ?? throw new ArgumentNullException(nameof(quotaService));
        this.currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var userId = this.currentUserService.UserId;
        if (userId is null)
        {
            // User not authenticated - let the handler deal with this
            return await next().ConfigureAwait(false);
        }

        var quotaResult = await this.quotaService.CheckQuotaAsync(userId.Value, cancellationToken)
            .ConfigureAwait(false);

        if (quotaResult.IsFailure)
        {
            this.logger.LogWarning(
                "Quota check failed for user {UserId}: {Error}",
                userId.Value,
                quotaResult.Error.Message);

            // Return the quota error wrapped in the expected response type
            return CreateErrorResponse(quotaResult.Error);
        }

        return await next().ConfigureAwait(false);
    }

    private static TResponse CreateErrorResponse(Error error)
    {
        // Handle Result<T> response types
        var responseType = typeof(TResponse);

        if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            var valueType = responseType.GetGenericArguments()[0];
            var failureMethod = typeof(Result<>)
                .MakeGenericType(valueType)
                .GetMethod("Failure", [typeof(Error)]);

            if (failureMethod is not null)
            {
                return (TResponse)failureMethod.Invoke(null, [error])!;
            }
        }

        // Handle plain Result response type
        if (responseType == typeof(Result))
        {
            return (TResponse)(object)Result.Failure(error);
        }

        // This shouldn't happen for properly typed commands, but throw to surface the issue
        throw new InvalidOperationException(
            $"Cannot create error response for type {responseType}. " +
            "Conversion commands should return Result or Result<T>.");
    }
}
