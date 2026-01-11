// <copyright file="DownloadConversionResultQuery.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Domain.Primitives;
using MediatR;

namespace FileConversionApi.Application.Queries.Conversion;

/// <summary>
/// Query to download the result of a conversion job.
/// </summary>
/// <param name="JobId">The job identifier.</param>
public record DownloadConversionResultQuery(Guid JobId) : IRequest<Result<FileDownloadResult>>;
