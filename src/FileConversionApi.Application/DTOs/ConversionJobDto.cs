// <copyright file="ConversionJobDto.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Domain.Entities;
using FileConversionApi.Domain.Enums;

namespace FileConversionApi.Application.DTOs;

/// <summary>
/// Data transfer object for conversion jobs.
/// </summary>
public record ConversionJobDto
{
    /// <summary>
    /// Gets the job identifier.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Gets the source format.
    /// </summary>
    public required string SourceFormat { get; init; }

    /// <summary>
    /// Gets the target format.
    /// </summary>
    public required string TargetFormat { get; init; }

    /// <summary>
    /// Gets the conversion status.
    /// </summary>
    public required ConversionStatus Status { get; init; }

    /// <summary>
    /// Gets the input file name.
    /// </summary>
    public required string InputFileName { get; init; }

    /// <summary>
    /// Gets the output file name.
    /// </summary>
    public string? OutputFileName { get; init; }

    /// <summary>
    /// Gets the error message if the conversion failed.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Gets the creation date and time.
    /// </summary>
    public required DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    /// Gets the completion date and time.
    /// </summary>
    public DateTimeOffset? CompletedAt { get; init; }

    /// <summary>
    /// Creates a DTO from a domain entity.
    /// </summary>
    /// <param name="job">The conversion job entity.</param>
    /// <returns>The DTO.</returns>
    public static ConversionJobDto FromEntity(ConversionJob job) =>
        new()
        {
            Id = job.Id.Value,
            SourceFormat = job.SourceFormat,
            TargetFormat = job.TargetFormat,
            Status = job.Status,
            InputFileName = job.InputFileName,
            OutputFileName = job.OutputFileName,
            ErrorMessage = job.ErrorMessage,
            CreatedAt = job.CreatedAt,
            CompletedAt = job.CompletedAt,
        };
}
