// <copyright file="DiskSpaceHealthCheck.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Infrastructure.Options;

using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace FileConversionApi.Infrastructure.HealthChecks;

/// <summary>
/// Health check for available disk space.
/// </summary>
public class DiskSpaceHealthCheck : IHealthCheck
{
    private readonly HealthCheckSettings settings;

    /// <summary>
    /// Initializes a new instance of the <see cref="DiskSpaceHealthCheck"/> class.
    /// </summary>
    /// <param name="settings">The health check settings.</param>
    public DiskSpaceHealthCheck(IOptions<HealthCheckSettings> settings)
    {
        this.settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
    }

    /// <inheritdoc/>
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var tempPath = Path.GetTempPath();
        var minimumRequiredBytes = (long)this.settings.DiskSpaceMinimumMB * 1024 * 1024;

        try
        {
            var rootPath = Path.GetPathRoot(tempPath);
            if (string.IsNullOrEmpty(rootPath))
            {
                return Task.FromResult(HealthCheckResult.Degraded(
                    "Unable to determine disk root path"));
            }

            var driveInfo = new DriveInfo(rootPath);
            var availableBytes = driveInfo.AvailableFreeSpace;

            var data = new Dictionary<string, object>
            {
                ["availableBytes"] = availableBytes,
                ["availableMB"] = availableBytes / 1024 / 1024,
                ["minimumRequiredBytes"] = minimumRequiredBytes,
                ["minimumRequiredMB"] = this.settings.DiskSpaceMinimumMB,
            };

            if (availableBytes < minimumRequiredBytes)
            {
                return Task.FromResult(HealthCheckResult.Unhealthy(
                    $"Low disk space: {availableBytes / 1024 / 1024} MB available, {this.settings.DiskSpaceMinimumMB} MB required",
                    data: data));
            }

            var availableGb = availableBytes / 1024.0 / 1024.0 / 1024.0;

            return Task.FromResult(HealthCheckResult.Healthy(
                $"{availableGb:F1} GB available",
                data: data));
        }
        catch (Exception ex) when (ex is not (OutOfMemoryException or StackOverflowException))
        {
            return Task.FromResult(HealthCheckResult.Unhealthy(
                "Failed to check disk space",
                ex));
        }
    }
}
