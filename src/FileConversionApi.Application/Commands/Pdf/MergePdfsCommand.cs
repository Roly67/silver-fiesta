// <copyright file="MergePdfsCommand.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Application.DTOs;
using FileConversionApi.Domain.Primitives;

using MediatR;

namespace FileConversionApi.Application.Commands.Pdf;

/// <summary>
/// Command to merge multiple PDF documents into one.
/// </summary>
public record MergePdfsCommand : IRequest<Result<ConversionJobDto>>
{
    /// <summary>
    /// Gets the PDF documents as base64 encoded strings.
    /// </summary>
    public string[]? PdfDocuments { get; init; }

    /// <summary>
    /// Gets the file name for the output.
    /// </summary>
    public string? FileName { get; init; }

    /// <summary>
    /// Gets the webhook URL to notify when operation completes.
    /// </summary>
    public string? WebhookUrl { get; init; }
}
