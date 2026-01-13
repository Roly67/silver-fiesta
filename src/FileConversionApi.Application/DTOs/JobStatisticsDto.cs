// <copyright file="JobStatisticsDto.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

namespace FileConversionApi.Application.DTOs;

/// <summary>
/// Data transfer object for job statistics.
/// </summary>
public record JobStatisticsDto
{
    /// <summary>
    /// Gets the total number of jobs.
    /// </summary>
    public required int TotalJobs { get; init; }

    /// <summary>
    /// Gets the number of completed jobs.
    /// </summary>
    public required int CompletedJobs { get; init; }

    /// <summary>
    /// Gets the number of failed jobs.
    /// </summary>
    public required int FailedJobs { get; init; }

    /// <summary>
    /// Gets the number of pending jobs.
    /// </summary>
    public required int PendingJobs { get; init; }

    /// <summary>
    /// Gets the total number of users.
    /// </summary>
    public required int TotalUsers { get; init; }

    /// <summary>
    /// Gets the success rate as a percentage.
    /// </summary>
    public double SuccessRate => this.TotalJobs > 0
        ? Math.Round((double)this.CompletedJobs / this.TotalJobs * 100, 2)
        : 0;
}
