// <copyright file="GetCurrentQuotaQuery.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Application.DTOs;
using FileConversionApi.Domain.Primitives;

using MediatR;

namespace FileConversionApi.Application.Queries.Quota;

/// <summary>
/// Query to get the current user's quota for the current month.
/// </summary>
public record GetCurrentQuotaQuery : IRequest<Result<UsageQuotaDto>>;
