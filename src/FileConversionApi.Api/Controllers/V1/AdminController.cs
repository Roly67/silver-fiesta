// <copyright file="AdminController.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Api.Requests;
using FileConversionApi.Application.Commands.Admin;
using FileConversionApi.Application.Commands.Quota;
using FileConversionApi.Application.Commands.RateLimit;
using FileConversionApi.Application.DTOs;
using FileConversionApi.Application.Queries.Admin;
using FileConversionApi.Application.Queries.Quota;
using FileConversionApi.Application.Queries.RateLimit;
using FileConversionApi.Domain.Enums;
using FileConversionApi.Infrastructure.Options;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FileConversionApi.Api.Controllers.V1;

/// <summary>
/// Controller for administrative operations.
/// </summary>
[ApiController]
[Route("api/v1/admin")]
[Authorize(Policy = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IMediator mediator;
    private readonly RateLimitingSettings rateLimitingSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminController"/> class.
    /// </summary>
    /// <param name="mediator">The mediator.</param>
    /// <param name="rateLimitingSettings">The rate limiting settings.</param>
    public AdminController(IMediator mediator, IOptions<RateLimitingSettings> rateLimitingSettings)
    {
        this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        this.rateLimitingSettings = rateLimitingSettings?.Value ?? throw new ArgumentNullException(nameof(rateLimitingSettings));
    }

    /// <summary>
    /// Gets all users with pagination.
    /// </summary>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paginated list of users.</returns>
    [HttpGet("users")]
    [ProducesResponseType(typeof(PaginatedResult<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetUsersQuery { Page = page, PageSize = pageSize };
        var result = await this.mediator.Send(query, cancellationToken).ConfigureAwait(false);

        return result.IsSuccess
            ? this.Ok(result.Value)
            : this.BadRequest(new { error = result.Error.Code, message = result.Error.Message });
    }

    /// <summary>
    /// Gets a user by identifier.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The user details.</returns>
    [HttpGet("users/{userId:guid}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetUser(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var query = new GetUserByIdQuery { UserId = userId };
        var result = await this.mediator.Send(query, cancellationToken).ConfigureAwait(false);

        if (result.IsFailure)
        {
            return result.Error.Code == "Admin.UserNotFound"
                ? this.NotFound(new { error = result.Error.Code, message = result.Error.Message })
                : this.BadRequest(new { error = result.Error.Code, message = result.Error.Message });
        }

        return this.Ok(result.Value);
    }

    /// <summary>
    /// Disables a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>No content on success.</returns>
    [HttpPost("users/{userId:guid}/disable")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DisableUser(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var command = new DisableUserCommand { UserId = userId };
        var result = await this.mediator.Send(command, cancellationToken).ConfigureAwait(false);

        if (result.IsFailure)
        {
            return result.Error.Code == "Admin.UserNotFound"
                ? this.NotFound(new { error = result.Error.Code, message = result.Error.Message })
                : this.BadRequest(new { error = result.Error.Code, message = result.Error.Message });
        }

        return this.NoContent();
    }

    /// <summary>
    /// Enables a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>No content on success.</returns>
    [HttpPost("users/{userId:guid}/enable")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> EnableUser(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var command = new EnableUserCommand { UserId = userId };
        var result = await this.mediator.Send(command, cancellationToken).ConfigureAwait(false);

        if (result.IsFailure)
        {
            return result.Error.Code == "Admin.UserNotFound"
                ? this.NotFound(new { error = result.Error.Code, message = result.Error.Message })
                : this.BadRequest(new { error = result.Error.Code, message = result.Error.Message });
        }

        return this.NoContent();
    }

    /// <summary>
    /// Resets a user's API key.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The new API key.</returns>
    [HttpPost("users/{userId:guid}/reset-api-key")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ResetApiKey(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var command = new ResetUserApiKeyCommand { UserId = userId };
        var result = await this.mediator.Send(command, cancellationToken).ConfigureAwait(false);

        if (result.IsFailure)
        {
            return result.Error.Code == "Admin.UserNotFound"
                ? this.NotFound(new { error = result.Error.Code, message = result.Error.Message })
                : this.BadRequest(new { error = result.Error.Code, message = result.Error.Message });
        }

        return this.Ok(new { apiKey = result.Value });
    }

    /// <summary>
    /// Grants admin privileges to a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>No content on success.</returns>
    [HttpPost("users/{userId:guid}/grant-admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GrantAdmin(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var command = new SetUserAdminCommand { UserId = userId, IsAdmin = true };
        var result = await this.mediator.Send(command, cancellationToken).ConfigureAwait(false);

        if (result.IsFailure)
        {
            return result.Error.Code == "Admin.UserNotFound"
                ? this.NotFound(new { error = result.Error.Code, message = result.Error.Message })
                : this.BadRequest(new { error = result.Error.Code, message = result.Error.Message });
        }

        return this.NoContent();
    }

    /// <summary>
    /// Revokes admin privileges from a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>No content on success.</returns>
    [HttpPost("users/{userId:guid}/revoke-admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RevokeAdmin(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var command = new SetUserAdminCommand { UserId = userId, IsAdmin = false };
        var result = await this.mediator.Send(command, cancellationToken).ConfigureAwait(false);

        if (result.IsFailure)
        {
            return result.Error.Code == "Admin.UserNotFound"
                ? this.NotFound(new { error = result.Error.Code, message = result.Error.Message })
                : this.BadRequest(new { error = result.Error.Code, message = result.Error.Message });
        }

        return this.NoContent();
    }

    /// <summary>
    /// Gets job statistics.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The job statistics.</returns>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(JobStatisticsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetStatistics(CancellationToken cancellationToken = default)
    {
        var query = new GetJobStatisticsQuery();
        var result = await this.mediator.Send(query, cancellationToken).ConfigureAwait(false);

        return result.IsSuccess
            ? this.Ok(result.Value)
            : this.BadRequest(new { error = result.Error.Code, message = result.Error.Message });
    }

    /// <summary>
    /// Gets a user's current quota.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The user's quota information.</returns>
    [HttpGet("users/{userId:guid}/quota")]
    [ProducesResponseType(typeof(UsageQuotaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetUserQuota(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var query = new GetUserQuotaQuery { UserId = userId };
        var result = await this.mediator.Send(query, cancellationToken).ConfigureAwait(false);

        if (result.IsFailure)
        {
            return result.Error.Code == "Admin.UserNotFound"
                ? this.NotFound(new { error = result.Error.Code, message = result.Error.Message })
                : this.BadRequest(new { error = result.Error.Code, message = result.Error.Message });
        }

        return this.Ok(result.Value);
    }

    /// <summary>
    /// Gets a user's quota history.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="months">The number of months to retrieve (default 12).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The user's quota history.</returns>
    [HttpGet("users/{userId:guid}/quota/history")]
    [ProducesResponseType(typeof(IReadOnlyList<UsageQuotaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetUserQuotaHistory(
        Guid userId,
        [FromQuery] int months = 12,
        CancellationToken cancellationToken = default)
    {
        var query = new GetUserQuotaHistoryQuery { UserId = userId, Months = months };
        var result = await this.mediator.Send(query, cancellationToken).ConfigureAwait(false);

        if (result.IsFailure)
        {
            return result.Error.Code == "Admin.UserNotFound"
                ? this.NotFound(new { error = result.Error.Code, message = result.Error.Message })
                : this.BadRequest(new { error = result.Error.Code, message = result.Error.Message });
        }

        return this.Ok(result.Value);
    }

    /// <summary>
    /// Updates a user's quota limits.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="request">The update request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated quota information.</returns>
    [HttpPut("users/{userId:guid}/quota")]
    [ProducesResponseType(typeof(UsageQuotaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateUserQuota(
        Guid userId,
        [FromBody] UpdateUserQuotaRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateUserQuotaCommand
        {
            UserId = userId,
            ConversionsLimit = request.ConversionsLimit,
            BytesLimit = request.BytesLimit,
        };
        var result = await this.mediator.Send(command, cancellationToken).ConfigureAwait(false);

        if (result.IsFailure)
        {
            return result.Error.Code == "Admin.UserNotFound"
                ? this.NotFound(new { error = result.Error.Code, message = result.Error.Message })
                : this.BadRequest(new { error = result.Error.Code, message = result.Error.Message });
        }

        return this.Ok(result.Value);
    }

    /// <summary>
    /// Gets a user's rate limit settings.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The user's rate limit settings.</returns>
    [HttpGet("users/{userId:guid}/rate-limits")]
    [ProducesResponseType(typeof(UserRateLimitSettingsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetUserRateLimits(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var query = new GetUserRateLimitSettingsQuery { UserId = userId };
        var result = await this.mediator.Send(query, cancellationToken).ConfigureAwait(false);

        if (result.IsFailure)
        {
            return result.Error.Code == "Admin.UserNotFound"
                ? this.NotFound(new { error = result.Error.Code, message = result.Error.Message })
                : this.BadRequest(new { error = result.Error.Code, message = result.Error.Message });
        }

        return this.Ok(result.Value);
    }

    /// <summary>
    /// Sets a user's rate limit tier.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="request">The tier to set.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>No content on success.</returns>
    [HttpPut("users/{userId:guid}/rate-limits/tier")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> SetUserRateLimitTier(
        Guid userId,
        [FromBody] SetUserRateLimitTierRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<RateLimitTier>(request.Tier, ignoreCase: true, out var tier))
        {
            return this.BadRequest(new { error = "RateLimit.InvalidTier", message = $"Invalid tier '{request.Tier}'. Valid values are: Free, Basic, Premium, Unlimited." });
        }

        var command = new SetUserRateLimitTierCommand { UserId = userId, Tier = tier };
        var result = await this.mediator.Send(command, cancellationToken).ConfigureAwait(false);

        if (result.IsFailure)
        {
            return result.Error.Code == "Admin.UserNotFound"
                ? this.NotFound(new { error = result.Error.Code, message = result.Error.Message })
                : this.BadRequest(new { error = result.Error.Code, message = result.Error.Message });
        }

        return this.NoContent();
    }

    /// <summary>
    /// Sets a per-policy override for a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="policyName">The policy name (standard or conversion).</param>
    /// <param name="request">The override settings.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>No content on success.</returns>
    [HttpPut("users/{userId:guid}/rate-limits/override/{policyName}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> SetUserRateLimitOverride(
        Guid userId,
        string policyName,
        [FromBody] SetUserRateLimitOverrideRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new SetUserRateLimitOverrideCommand
        {
            UserId = userId,
            PolicyName = policyName,
            PermitLimit = request.PermitLimit,
            WindowMinutes = request.WindowMinutes,
        };
        var result = await this.mediator.Send(command, cancellationToken).ConfigureAwait(false);

        if (result.IsFailure)
        {
            return result.Error.Code == "Admin.UserNotFound"
                ? this.NotFound(new { error = result.Error.Code, message = result.Error.Message })
                : this.BadRequest(new { error = result.Error.Code, message = result.Error.Message });
        }

        return this.NoContent();
    }

    /// <summary>
    /// Clears all rate limit overrides for a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>No content on success.</returns>
    [HttpDelete("users/{userId:guid}/rate-limits/overrides")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ClearUserRateLimitOverrides(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var command = new ClearUserRateLimitOverridesCommand { UserId = userId };
        var result = await this.mediator.Send(command, cancellationToken).ConfigureAwait(false);

        if (result.IsFailure)
        {
            return result.Error.Code == "Admin.UserNotFound"
                ? this.NotFound(new { error = result.Error.Code, message = result.Error.Message })
                : this.BadRequest(new { error = result.Error.Code, message = result.Error.Message });
        }

        return this.NoContent();
    }

    /// <summary>
    /// Gets all available rate limit tiers and their configurations.
    /// </summary>
    /// <returns>The list of available tiers.</returns>
    [HttpGet("rate-limits/tiers")]
    [ProducesResponseType(typeof(IReadOnlyList<RateLimitTierConfigDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult GetRateLimitTiers()
    {
        var tiers = this.rateLimitingSettings.Tiers
            .Select(kvp => new RateLimitTierConfigDto
            {
                Name = kvp.Key,
                StandardPermitLimit = kvp.Value.StandardPolicy.PermitLimit,
                StandardWindowMinutes = kvp.Value.StandardPolicy.WindowMinutes,
                ConversionPermitLimit = kvp.Value.ConversionPolicy.PermitLimit,
                ConversionWindowMinutes = kvp.Value.ConversionPolicy.WindowMinutes,
            })
            .ToList();

        return this.Ok(tiers);
    }
}
