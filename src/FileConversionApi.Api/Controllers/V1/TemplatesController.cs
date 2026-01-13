// <copyright file="TemplatesController.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using System.Security.Claims;

using FileConversionApi.Application.Commands.Templates;
using FileConversionApi.Application.DTOs;
using FileConversionApi.Application.Queries.Templates;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace FileConversionApi.Api.Controllers.V1;

/// <summary>
/// Controller for managing conversion templates.
/// </summary>
[ApiController]
[Route("api/v1/templates")]
[Authorize]
[EnableRateLimiting("standard")]
public class TemplatesController : ControllerBase
{
    private readonly IMediator mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="TemplatesController"/> class.
    /// </summary>
    /// <param name="mediator">The mediator.</param>
    public TemplatesController(IMediator mediator)
    {
        this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    /// <summary>
    /// Gets all templates for the current user.
    /// </summary>
    /// <param name="targetFormat">Optional target format filter.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of templates.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ConversionTemplateDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetTemplates(
        [FromQuery] string? targetFormat = null,
        CancellationToken cancellationToken = default)
    {
        var userId = this.GetCurrentUserId();
        if (userId == null)
        {
            return this.Unauthorized();
        }

        var query = new GetUserTemplatesQuery
        {
            UserId = userId.Value,
            TargetFormat = targetFormat,
        };

        var result = await this.mediator.Send(query, cancellationToken);

        return result.IsSuccess
            ? this.Ok(result.Value)
            : this.BadRequest(new { error = result.Error.Code, message = result.Error.Message });
    }

    /// <summary>
    /// Gets a specific template by identifier.
    /// </summary>
    /// <param name="templateId">The template identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The template details.</returns>
    [HttpGet("{templateId:guid}")]
    [ProducesResponseType(typeof(ConversionTemplateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetTemplate(
        Guid templateId,
        CancellationToken cancellationToken = default)
    {
        var userId = this.GetCurrentUserId();
        if (userId == null)
        {
            return this.Unauthorized();
        }

        var query = new GetTemplateByIdQuery
        {
            TemplateId = templateId,
            UserId = userId.Value,
        };

        var result = await this.mediator.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.Code switch
            {
                "Template.NotFound" => this.NotFound(new { error = result.Error.Code, message = result.Error.Message }),
                "Template.AccessDenied" => this.Forbid(),
                _ => this.BadRequest(new { error = result.Error.Code, message = result.Error.Message }),
            };
        }

        return this.Ok(result.Value);
    }

    /// <summary>
    /// Creates a new conversion template.
    /// </summary>
    /// <param name="request">The create template request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created template.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ConversionTemplateDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateTemplate(
        [FromBody] CreateTemplateRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = this.GetCurrentUserId();
        if (userId == null)
        {
            return this.Unauthorized();
        }

        var command = new CreateConversionTemplateCommand
        {
            UserId = userId.Value,
            Name = request.Name,
            Description = request.Description,
            TargetFormat = request.TargetFormat,
            Options = request.Options,
        };

        var result = await this.mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return this.BadRequest(new { error = result.Error.Code, message = result.Error.Message });
        }

        return this.CreatedAtAction(
            nameof(this.GetTemplate),
            new { templateId = result.Value.Id },
            result.Value);
    }

    /// <summary>
    /// Updates an existing conversion template.
    /// </summary>
    /// <param name="templateId">The template identifier.</param>
    /// <param name="request">The update template request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated template.</returns>
    [HttpPut("{templateId:guid}")]
    [ProducesResponseType(typeof(ConversionTemplateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateTemplate(
        Guid templateId,
        [FromBody] UpdateTemplateRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = this.GetCurrentUserId();
        if (userId == null)
        {
            return this.Unauthorized();
        }

        var command = new UpdateConversionTemplateCommand
        {
            TemplateId = templateId,
            UserId = userId.Value,
            Name = request.Name,
            Description = request.Description,
            TargetFormat = request.TargetFormat,
            Options = request.Options,
        };

        var result = await this.mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.Code switch
            {
                "Template.NotFound" => this.NotFound(new { error = result.Error.Code, message = result.Error.Message }),
                "Template.AccessDenied" => this.Forbid(),
                _ => this.BadRequest(new { error = result.Error.Code, message = result.Error.Message }),
            };
        }

        return this.Ok(result.Value);
    }

    /// <summary>
    /// Deletes a conversion template.
    /// </summary>
    /// <param name="templateId">The template identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>No content on success.</returns>
    [HttpDelete("{templateId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteTemplate(
        Guid templateId,
        CancellationToken cancellationToken = default)
    {
        var userId = this.GetCurrentUserId();
        if (userId == null)
        {
            return this.Unauthorized();
        }

        var command = new DeleteConversionTemplateCommand
        {
            TemplateId = templateId,
            UserId = userId.Value,
        };

        var result = await this.mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.Code switch
            {
                "Template.NotFound" => this.NotFound(new { error = result.Error.Code, message = result.Error.Message }),
                "Template.AccessDenied" => this.Forbid(),
                _ => this.BadRequest(new { error = result.Error.Code, message = result.Error.Message }),
            };
        }

        return this.NoContent();
    }

    private Guid? GetCurrentUserId()
    {
        var userIdClaim = this.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return null;
        }

        return userId;
    }
}
