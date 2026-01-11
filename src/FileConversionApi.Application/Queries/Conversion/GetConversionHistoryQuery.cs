// <copyright file="GetConversionHistoryQuery.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Application.DTOs;
using FileConversionApi.Domain.Primitives;
using MediatR;

namespace FileConversionApi.Application.Queries.Conversion;

/// <summary>
/// Query to get conversion history for the current user.
/// </summary>
/// <param name="Page">The page number (1-based).</param>
/// <param name="PageSize">The page size.</param>
public record GetConversionHistoryQuery(int Page = 1, int PageSize = 20)
    : IRequest<Result<PagedResult<ConversionJobDto>>>;
