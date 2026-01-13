// <copyright file="GetJobStatisticsQuery.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Application.DTOs;
using FileConversionApi.Domain.Primitives;

using MediatR;

namespace FileConversionApi.Application.Queries.Admin;

/// <summary>
/// Query to get job statistics.
/// </summary>
public record GetJobStatisticsQuery : IRequest<Result<JobStatisticsDto>>;
