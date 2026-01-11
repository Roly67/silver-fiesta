// <copyright file="GetConversionJobQuery.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Application.DTOs;
using FileConversionApi.Domain.Primitives;
using MediatR;

namespace FileConversionApi.Application.Queries.Conversion;

/// <summary>
/// Query to get a conversion job by ID.
/// </summary>
/// <param name="JobId">The job identifier.</param>
public record GetConversionJobQuery(Guid JobId) : IRequest<Result<ConversionJobDto>>;
