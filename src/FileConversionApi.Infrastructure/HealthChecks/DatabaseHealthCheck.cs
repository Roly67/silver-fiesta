// <copyright file="DatabaseHealthCheck.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Infrastructure.Persistence;

using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FileConversionApi.Infrastructure.HealthChecks;

/// <summary>
/// Health check for database connectivity.
/// </summary>
public class DatabaseHealthCheck : IHealthCheck
{
    private readonly AppDbContext dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseHealthCheck"/> class.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    public DatabaseHealthCheck(AppDbContext dbContext)
    {
        this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    /// <inheritdoc/>
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var canConnect = await this.dbContext.Database
                .CanConnectAsync(cancellationToken)
                .ConfigureAwait(false);

            if (canConnect)
            {
                return HealthCheckResult.Healthy("PostgreSQL connection successful");
            }

            return HealthCheckResult.Unhealthy("PostgreSQL connection failed");
        }
        catch (Exception ex) when (ex is not (OutOfMemoryException or StackOverflowException))
        {
            return HealthCheckResult.Unhealthy("PostgreSQL connection failed", ex);
        }
    }
}
