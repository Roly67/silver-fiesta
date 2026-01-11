// <copyright file="ConvertController.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Api.Models;
using FileConversionApi.Application.Commands.Conversion;
using FileConversionApi.Application.DTOs;
using FileConversionApi.Application.Queries.Conversion;

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FileConversionApi.Api.Controllers.V1;

/// <summary>
/// Controller for file conversion operations.
/// </summary>
[ApiController]
[Route("api/v1/convert")]
[Produces("application/json")]
[Authorize]
public class ConvertController : ControllerBase
{
    private readonly IMediator mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConvertController"/> class.
    /// </summary>
    /// <param name="mediator">The mediator.</param>
    public ConvertController(IMediator mediator)
    {
        this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    /// <summary>
    /// Converts HTML to PDF.
    /// </summary>
    /// <param name="request">The conversion request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The conversion job details.</returns>
    /// <response code="202">Conversion job accepted.</response>
    /// <response code="400">Invalid request.</response>
    /// <response code="401">Unauthorized.</response>
    [HttpPost("html-to-pdf")]
    [ProducesResponseType(typeof(ConversionJobDto), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetailsResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetailsResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ConvertHtmlToPdf(
        [FromBody] HtmlToPdfRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ConvertHtmlToPdfCommand
        {
            HtmlContent = request.HtmlContent,
            Url = request.Url,
            FileName = request.FileName,
            Options = request.Options,
        };

        var result = await this.mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return this.BadRequest(new ProblemDetailsResponse
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Title = "Conversion Failed",
                Status = StatusCodes.Status400BadRequest,
                Detail = result.Error.Message,
                ErrorCode = result.Error.Code,
            });
        }

        return this.AcceptedAtAction(
            nameof(this.GetJob),
            new { jobId = result.Value.Id },
            result.Value);
    }

    /// <summary>
    /// Converts Markdown to PDF.
    /// </summary>
    /// <param name="request">The conversion request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The conversion job details.</returns>
    /// <response code="202">Conversion job accepted.</response>
    /// <response code="400">Invalid request.</response>
    /// <response code="401">Unauthorized.</response>
    [HttpPost("markdown-to-pdf")]
    [ProducesResponseType(typeof(ConversionJobDto), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetailsResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetailsResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ConvertMarkdownToPdf(
        [FromBody] MarkdownToPdfRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ConvertMarkdownToPdfCommand
        {
            Markdown = request.Markdown,
            FileName = request.FileName,
            Options = request.Options,
        };

        var result = await this.mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return this.BadRequest(new ProblemDetailsResponse
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Title = "Conversion Failed",
                Status = StatusCodes.Status400BadRequest,
                Detail = result.Error.Message,
                ErrorCode = result.Error.Code,
            });
        }

        return this.AcceptedAtAction(
            nameof(this.GetJob),
            new { jobId = result.Value.Id },
            result.Value);
    }

    /// <summary>
    /// Gets a conversion job by ID.
    /// </summary>
    /// <param name="jobId">The job identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The conversion job details.</returns>
    /// <response code="200">Job found.</response>
    /// <response code="404">Job not found.</response>
    /// <response code="401">Unauthorized.</response>
    [HttpGet("{jobId:guid}")]
    [ProducesResponseType(typeof(ConversionJobDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetailsResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetailsResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetJob(
        Guid jobId,
        CancellationToken cancellationToken)
    {
        var query = new GetConversionJobQuery(jobId);
        var result = await this.mediator.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return this.NotFound(new ProblemDetailsResponse
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                Title = "Not Found",
                Status = StatusCodes.Status404NotFound,
                Detail = result.Error.Message,
                ErrorCode = result.Error.Code,
            });
        }

        return this.Ok(result.Value);
    }

    /// <summary>
    /// Downloads the converted file.
    /// </summary>
    /// <param name="jobId">The job identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The converted file.</returns>
    /// <response code="200">File downloaded.</response>
    /// <response code="400">Conversion not completed.</response>
    /// <response code="404">Job not found.</response>
    /// <response code="401">Unauthorized.</response>
    [HttpGet("{jobId:guid}/download")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetailsResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetailsResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetailsResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Download(
        Guid jobId,
        CancellationToken cancellationToken)
    {
        var query = new DownloadConversionResultQuery(jobId);
        var result = await this.mediator.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            var statusCode = result.Error.Code == "ConversionJob.NotFound"
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;

            return this.StatusCode(statusCode, new ProblemDetailsResponse
            {
                Type = statusCode == StatusCodes.Status404NotFound
                    ? "https://tools.ietf.org/html/rfc7231#section-6.5.4"
                    : "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Title = statusCode == StatusCodes.Status404NotFound ? "Not Found" : "Download Failed",
                Status = statusCode,
                Detail = result.Error.Message,
                ErrorCode = result.Error.Code,
            });
        }

        return this.File(result.Value.Content, result.Value.ContentType, result.Value.FileName);
    }

    /// <summary>
    /// Gets the conversion history for the current user.
    /// </summary>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The paginated conversion history.</returns>
    /// <response code="200">History retrieved.</response>
    /// <response code="401">Unauthorized.</response>
    [HttpGet("history")]
    [ProducesResponseType(typeof(PagedResult<ConversionJobDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetailsResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetHistory(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetConversionHistoryQuery(page, pageSize);
        var result = await this.mediator.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return this.BadRequest(new ProblemDetailsResponse
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Title = "Request Failed",
                Status = StatusCodes.Status400BadRequest,
                Detail = result.Error.Message,
                ErrorCode = result.Error.Code,
            });
        }

        return this.Ok(result.Value);
    }
}
