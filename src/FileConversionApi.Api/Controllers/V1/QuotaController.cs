// <copyright file="QuotaController.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Application.DTOs;
using FileConversionApi.Application.Queries.Quota;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace FileConversionApi.Api.Controllers.V1;

/// <summary>
/// Controller for user quota operations.
/// </summary>
[ApiController]
[Route("api/v1/quota")]
[Authorize]
[EnableRateLimiting("standard")]
public class QuotaController : ControllerBase
{
    private readonly IMediator mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="QuotaController"/> class.
    /// </summary>
    /// <param name="mediator">The mediator.</param>
    public QuotaController(IMediator mediator)
    {
        this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    /// <summary>
    /// Gets the current user's quota for the current month.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The current quota information.</returns>
    /// <response code="200">Returns the current quota.</response>
    /// <response code="401">User is not authenticated.</response>
    [HttpGet]
    [ProducesResponseType(typeof(UsageQuotaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentQuota(CancellationToken cancellationToken = default)
    {
        var query = new GetCurrentQuotaQuery();
        var result = await this.mediator.Send(query, cancellationToken).ConfigureAwait(false);

        if (result.IsFailure)
        {
            return result.Error.Code == "Error.Unauthorized"
                ? this.Unauthorized(new { error = result.Error.Code, message = result.Error.Message })
                : this.BadRequest(new { error = result.Error.Code, message = result.Error.Message });
        }

        return this.Ok(result.Value);
    }
}
