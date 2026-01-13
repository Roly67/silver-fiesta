// <copyright file="BatchConversionCommand.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Application.DTOs;
using FileConversionApi.Domain.Primitives;

using MediatR;

namespace FileConversionApi.Application.Commands.Conversion;

/// <summary>
/// Command to process multiple conversions in a single batch.
/// </summary>
public record BatchConversionCommand : IRequest<Result<BatchConversionResult>>, IConversionCommand
{
    /// <summary>
    /// Gets the conversion items in the batch.
    /// </summary>
    public List<BatchConversionItem> Items { get; init; } = [];

    /// <summary>
    /// Gets the webhook URL to notify when all conversions complete.
    /// </summary>
    public string? WebhookUrl { get; init; }
}
