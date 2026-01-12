// <copyright file="GetJobStatisticsQueryHandler.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Application.DTOs;
using FileConversionApi.Application.Interfaces;
using FileConversionApi.Domain.Primitives;

using MediatR;

namespace FileConversionApi.Application.Queries.Admin;

/// <summary>
/// Handles the GetJobStatisticsQuery.
/// </summary>
public class GetJobStatisticsQueryHandler : IRequestHandler<GetJobStatisticsQuery, Result<JobStatisticsDto>>
{
    private readonly IConversionJobRepository jobRepository;
    private readonly IUserRepository userRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetJobStatisticsQueryHandler"/> class.
    /// </summary>
    /// <param name="jobRepository">The job repository.</param>
    /// <param name="userRepository">The user repository.</param>
    public GetJobStatisticsQueryHandler(
        IConversionJobRepository jobRepository,
        IUserRepository userRepository)
    {
        this.jobRepository = jobRepository ?? throw new ArgumentNullException(nameof(jobRepository));
        this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    /// <inheritdoc/>
    public async Task<Result<JobStatisticsDto>> Handle(
        GetJobStatisticsQuery request,
        CancellationToken cancellationToken)
    {
        var (total, completed, failed, pending) = await this.jobRepository
            .GetStatisticsAsync(cancellationToken)
            .ConfigureAwait(false);

        var totalUsers = await this.userRepository
            .GetTotalCountAsync(cancellationToken)
            .ConfigureAwait(false);

        return new JobStatisticsDto
        {
            TotalJobs = total,
            CompletedJobs = completed,
            FailedJobs = failed,
            PendingJobs = pending,
            TotalUsers = totalUsers,
        };
    }
}
