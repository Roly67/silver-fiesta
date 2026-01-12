// <copyright file="IWebhookService.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Domain.Entities;

namespace FileConversionApi.Application.Interfaces;

/// <summary>
/// Service for sending webhook notifications when conversion jobs complete.
/// </summary>
public interface IWebhookService
{
    /// <summary>
    /// Sends a webhook notification for a completed or failed conversion job.
    /// </summary>
    /// <param name="job">The conversion job that completed.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SendJobCompletedAsync(ConversionJob job, CancellationToken cancellationToken);
}
