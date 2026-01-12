// <copyright file="ConvertController.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Api.Models;
using FileConversionApi.Application.Commands.Conversion;
using FileConversionApi.Application.Commands.Pdf;
using FileConversionApi.Application.DTOs;
using FileConversionApi.Application.Queries.Conversion;

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace FileConversionApi.Api.Controllers.V1;

/// <summary>
/// Controller for file conversion operations.
/// </summary>
[ApiController]
[Route("api/v1/convert")]
[Produces("application/json")]
[Authorize]
[EnableRateLimiting("conversion")]
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
            WebhookUrl = request.WebhookUrl,
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
            WebhookUrl = request.WebhookUrl,
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
    /// Converts Markdown to HTML.
    /// </summary>
    /// <param name="request">The conversion request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The conversion job details.</returns>
    /// <response code="202">Conversion job accepted.</response>
    /// <response code="400">Invalid request.</response>
    /// <response code="401">Unauthorized.</response>
    [HttpPost("markdown-to-html")]
    [ProducesResponseType(typeof(ConversionJobDto), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetailsResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetailsResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ConvertMarkdownToHtml(
        [FromBody] MarkdownToHtmlRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ConvertMarkdownToHtmlCommand
        {
            Markdown = request.Markdown,
            FileName = request.FileName,
            Options = request.Options,
            WebhookUrl = request.WebhookUrl,
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
    /// Converts an image between formats.
    /// </summary>
    /// <param name="request">The conversion request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The conversion job details.</returns>
    /// <remarks>
    /// Supported formats: png, jpeg (jpg), webp, gif, bmp.
    /// Image data should be base64 encoded.
    /// </remarks>
    /// <response code="202">Conversion job accepted.</response>
    /// <response code="400">Invalid request.</response>
    /// <response code="401">Unauthorized.</response>
    [HttpPost("image")]
    [ProducesResponseType(typeof(ConversionJobDto), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetailsResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetailsResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ConvertImage(
        [FromBody] ImageConversionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ConvertImageCommand
        {
            ImageData = request.ImageData,
            SourceFormat = request.SourceFormat,
            TargetFormat = request.TargetFormat,
            FileName = request.FileName,
            Options = request.Options,
            WebhookUrl = request.WebhookUrl,
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
    /// Merges multiple PDF documents into one.
    /// </summary>
    /// <param name="request">The merge request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The job details.</returns>
    /// <remarks>
    /// PDF documents should be base64 encoded. At least two documents are required.
    /// </remarks>
    /// <response code="202">Merge job accepted.</response>
    /// <response code="400">Invalid request.</response>
    /// <response code="401">Unauthorized.</response>
    [HttpPost("pdf/merge")]
    [ProducesResponseType(typeof(ConversionJobDto), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetailsResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetailsResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> MergePdfs(
        [FromBody] MergePdfsRequest request,
        CancellationToken cancellationToken)
    {
        var command = new MergePdfsCommand
        {
            PdfDocuments = request.PdfDocuments,
            FileName = request.FileName,
            WebhookUrl = request.WebhookUrl,
        };

        var result = await this.mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return this.BadRequest(new ProblemDetailsResponse
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Title = "Merge Failed",
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
    /// Splits a PDF document into multiple PDFs.
    /// </summary>
    /// <param name="request">The split request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The job details.</returns>
    /// <remarks>
    /// PDF data should be base64 encoded. The output is a ZIP file containing the split PDFs.
    /// You can split by page ranges (e.g., "1-3", "5", "7-10") or into individual pages.
    /// </remarks>
    /// <response code="202">Split job accepted.</response>
    /// <response code="400">Invalid request.</response>
    /// <response code="401">Unauthorized.</response>
    [HttpPost("pdf/split")]
    [ProducesResponseType(typeof(ConversionJobDto), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetailsResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetailsResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SplitPdf(
        [FromBody] SplitPdfRequest request,
        CancellationToken cancellationToken)
    {
        var command = new SplitPdfCommand
        {
            PdfData = request.PdfData,
            FileName = request.FileName,
            Options = request.Options,
            WebhookUrl = request.WebhookUrl,
        };

        var result = await this.mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return this.BadRequest(new ProblemDetailsResponse
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Title = "Split Failed",
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
    [EnableRateLimiting("standard")]
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
    [EnableRateLimiting("standard")]
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
    [EnableRateLimiting("standard")]
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
