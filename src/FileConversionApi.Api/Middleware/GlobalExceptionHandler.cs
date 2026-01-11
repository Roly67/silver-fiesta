// <copyright file="GlobalExceptionHandler.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using System.Diagnostics;

using FileConversionApi.Api.Models;
using FileConversionApi.Domain.Exceptions;

using FluentValidation;

using Microsoft.AspNetCore.Diagnostics;

namespace FileConversionApi.Api.Middleware;

/// <summary>
/// Global exception handler that converts exceptions to Problem Details.
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GlobalExceptionHandler"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;

        this.logger.LogError(exception, "Exception occurred. TraceId: {TraceId}", traceId);

        var problemDetails = exception switch
        {
            ValidationException validationEx => CreateValidationProblemDetails(validationEx, traceId),
            EntityNotFoundException notFoundEx => CreateNotFoundProblemDetails(notFoundEx, traceId),
            BusinessRuleException businessEx => CreateBusinessRuleProblemDetails(businessEx, traceId),
            UnauthorizedException => CreateUnauthorizedProblemDetails(traceId),
            ForbiddenException forbiddenEx => CreateForbiddenProblemDetails(forbiddenEx, traceId),
            DomainException domainEx => CreateDomainProblemDetails(domainEx, traceId),
            _ => CreateInternalErrorProblemDetails(traceId),
        };

        httpContext.Response.StatusCode = problemDetails.Status;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    private static ProblemDetailsResponse CreateValidationProblemDetails(
        ValidationException exception,
        string traceId)
    {
        return new ProblemDetailsResponse
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Title = "Validation Failed",
            Status = StatusCodes.Status400BadRequest,
            Detail = "One or more validation errors occurred.",
            TraceId = traceId,
            ErrorCode = "VALIDATION_ERROR",
            Errors = exception.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray()),
        };
    }

    private static ProblemDetailsResponse CreateNotFoundProblemDetails(
        EntityNotFoundException exception,
        string traceId)
    {
        return new ProblemDetailsResponse
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
            Title = "Resource Not Found",
            Status = StatusCodes.Status404NotFound,
            Detail = exception.Message,
            TraceId = traceId,
            ErrorCode = "NOT_FOUND",
        };
    }

    private static ProblemDetailsResponse CreateBusinessRuleProblemDetails(
        BusinessRuleException exception,
        string traceId)
    {
        return new ProblemDetailsResponse
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Title = "Business Rule Violation",
            Status = StatusCodes.Status400BadRequest,
            Detail = exception.Message,
            TraceId = traceId,
            ErrorCode = exception.RuleCode,
        };
    }

    private static ProblemDetailsResponse CreateUnauthorizedProblemDetails(string traceId)
    {
        return new ProblemDetailsResponse
        {
            Type = "https://tools.ietf.org/html/rfc7235#section-3.1",
            Title = "Unauthorized",
            Status = StatusCodes.Status401Unauthorized,
            Detail = "Authentication is required.",
            TraceId = traceId,
            ErrorCode = "UNAUTHORIZED",
        };
    }

    private static ProblemDetailsResponse CreateForbiddenProblemDetails(
        ForbiddenException exception,
        string traceId)
    {
        return new ProblemDetailsResponse
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.3",
            Title = "Forbidden",
            Status = StatusCodes.Status403Forbidden,
            Detail = exception.Message,
            TraceId = traceId,
            ErrorCode = "FORBIDDEN",
        };
    }

    private static ProblemDetailsResponse CreateDomainProblemDetails(
        DomainException exception,
        string traceId)
    {
        return new ProblemDetailsResponse
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Title = "Domain Error",
            Status = StatusCodes.Status400BadRequest,
            Detail = exception.Message,
            TraceId = traceId,
            ErrorCode = "DOMAIN_ERROR",
        };
    }

    private static ProblemDetailsResponse CreateInternalErrorProblemDetails(string traceId)
    {
        return new ProblemDetailsResponse
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            Title = "Internal Server Error",
            Status = StatusCodes.Status500InternalServerError,
            Detail = "An unexpected error occurred. Please try again later.",
            TraceId = traceId,
            ErrorCode = "INTERNAL_ERROR",
        };
    }
}
