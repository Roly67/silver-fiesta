// <copyright file="ConvertDocxToPdfCommand.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Application.DTOs;
using FileConversionApi.Domain.Primitives;

using MediatR;

namespace FileConversionApi.Application.Commands.Conversion;

/// <summary>
/// Command to convert a DOCX (Word) document to PDF.
/// </summary>
public record ConvertDocxToPdfCommand : IRequest<Result<ConversionJobDto>>, IConversionCommand
{
    /// <summary>
    /// Gets the DOCX document data as base64 encoded string.
    /// </summary>
    public string? DocumentData { get; init; }

    /// <summary>
    /// Gets the file name for the output.
    /// </summary>
    public string? FileName { get; init; }

    /// <summary>
    /// Gets the conversion options.
    /// </summary>
    public ConversionOptions? Options { get; init; }

    /// <summary>
    /// Gets the webhook URL to notify when conversion completes.
    /// </summary>
    public string? WebhookUrl { get; init; }
}
