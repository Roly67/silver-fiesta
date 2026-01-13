// <copyright file="BatchConversionCommandHandler.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Application.DTOs;
using FileConversionApi.Domain.Primitives;

using MediatR;

using Microsoft.Extensions.Logging;

namespace FileConversionApi.Application.Commands.Conversion;

/// <summary>
/// Handles the batch conversion command by delegating to individual conversion handlers.
/// </summary>
public class BatchConversionCommandHandler : IRequestHandler<BatchConversionCommand, Result<BatchConversionResult>>
{
    private const int MaxBatchSize = 20;

    private static readonly HashSet<string> ValidConversionTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "html-to-pdf",
        "markdown-to-pdf",
        "markdown-to-html",
        "image",
    };

    private readonly IMediator mediator;
    private readonly ILogger<BatchConversionCommandHandler> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="BatchConversionCommandHandler"/> class.
    /// </summary>
    /// <param name="mediator">The mediator.</param>
    /// <param name="logger">The logger.</param>
    public BatchConversionCommandHandler(
        IMediator mediator,
        ILogger<BatchConversionCommandHandler> logger)
    {
        this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<Result<BatchConversionResult>> Handle(
        BatchConversionCommand request,
        CancellationToken cancellationToken)
    {
        if (request.Items.Count == 0)
        {
            return new Error("Batch.EmptyRequest", "At least one conversion item is required.");
        }

        if (request.Items.Count > MaxBatchSize)
        {
            return new Error("Batch.TooManyItems", $"Batch size cannot exceed {MaxBatchSize} items.");
        }

        this.logger.LogInformation("Processing batch conversion with {Count} items", request.Items.Count);

        var result = new BatchConversionResult
        {
            TotalItems = request.Items.Count,
        };

        for (var i = 0; i < request.Items.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var item = request.Items[i];
            var itemResult = await this.ProcessItemAsync(item, i, request.WebhookUrl, cancellationToken)
                .ConfigureAwait(false);

            result.Results.Add(itemResult);

            if (itemResult.Success)
            {
                result.SuccessCount++;
            }
            else
            {
                result.FailureCount++;
            }
        }

        this.logger.LogInformation(
            "Batch conversion completed: {Success} succeeded, {Failed} failed",
            result.SuccessCount,
            result.FailureCount);

        return result;
    }

    private static BatchItemResult CreateFailureResult(int index, string errorCode, string errorMessage)
    {
        return new BatchItemResult
        {
            Index = index,
            Success = false,
            ErrorCode = errorCode,
            ErrorMessage = errorMessage,
        };
    }

    private async Task<BatchItemResult> ProcessItemAsync(
        BatchConversionItem item,
        int index,
        string? batchWebhookUrl,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(item.Type))
        {
            return CreateFailureResult(index, "Batch.MissingType", "Conversion type is required.");
        }

        if (!ValidConversionTypes.Contains(item.Type))
        {
            return CreateFailureResult(
                index,
                "Batch.InvalidType",
                $"Invalid conversion type '{item.Type}'. Valid types: {string.Join(", ", ValidConversionTypes)}");
        }

        try
        {
            var conversionResult = item.Type.ToLowerInvariant() switch
            {
                "html-to-pdf" => await this.ProcessHtmlToPdfAsync(item, batchWebhookUrl, cancellationToken).ConfigureAwait(false),
                "markdown-to-pdf" => await this.ProcessMarkdownToPdfAsync(item, batchWebhookUrl, cancellationToken).ConfigureAwait(false),
                "markdown-to-html" => await this.ProcessMarkdownToHtmlAsync(item, batchWebhookUrl, cancellationToken).ConfigureAwait(false),
                "image" => await this.ProcessImageAsync(item, batchWebhookUrl, cancellationToken).ConfigureAwait(false),
                _ => (Result<ConversionJobDto>)new Error("Batch.InvalidType", $"Unknown type: {item.Type}"),
            };

            if (conversionResult.IsSuccess)
            {
                return new BatchItemResult
                {
                    Index = index,
                    Success = true,
                    Job = conversionResult.Value,
                };
            }

            return CreateFailureResult(index, conversionResult.Error.Code, conversionResult.Error.Message);
        }
        catch (Exception ex) when (ex is not (OutOfMemoryException or StackOverflowException))
        {
            this.logger.LogError(ex, "Failed to process batch item at index {Index}", index);
            return CreateFailureResult(index, "Batch.ProcessingFailed", ex.Message);
        }
    }

    private async Task<Result<ConversionJobDto>> ProcessHtmlToPdfAsync(
        BatchConversionItem item,
        string? webhookUrl,
        CancellationToken cancellationToken)
    {
        var command = new ConvertHtmlToPdfCommand
        {
            HtmlContent = item.HtmlContent,
            Url = item.Url,
            FileName = item.FileName,
            Options = item.Options,
            WebhookUrl = webhookUrl,
        };

        return await this.mediator.Send(command, cancellationToken).ConfigureAwait(false);
    }

    private async Task<Result<ConversionJobDto>> ProcessMarkdownToPdfAsync(
        BatchConversionItem item,
        string? webhookUrl,
        CancellationToken cancellationToken)
    {
        var command = new ConvertMarkdownToPdfCommand
        {
            Markdown = item.Markdown,
            FileName = item.FileName,
            Options = item.Options,
            WebhookUrl = webhookUrl,
        };

        return await this.mediator.Send(command, cancellationToken).ConfigureAwait(false);
    }

    private async Task<Result<ConversionJobDto>> ProcessMarkdownToHtmlAsync(
        BatchConversionItem item,
        string? webhookUrl,
        CancellationToken cancellationToken)
    {
        var command = new ConvertMarkdownToHtmlCommand
        {
            Markdown = item.Markdown,
            FileName = item.FileName,
            Options = item.Options,
            WebhookUrl = webhookUrl,
        };

        return await this.mediator.Send(command, cancellationToken).ConfigureAwait(false);
    }

    private async Task<Result<ConversionJobDto>> ProcessImageAsync(
        BatchConversionItem item,
        string? webhookUrl,
        CancellationToken cancellationToken)
    {
        var command = new ConvertImageCommand
        {
            ImageData = item.ImageData,
            SourceFormat = item.SourceFormat,
            TargetFormat = item.TargetFormat,
            FileName = item.FileName,
            Options = item.Options,
            WebhookUrl = webhookUrl,
        };

        return await this.mediator.Send(command, cancellationToken).ConfigureAwait(false);
    }
}
