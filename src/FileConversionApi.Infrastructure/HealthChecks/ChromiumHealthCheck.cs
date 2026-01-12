// <copyright file="ChromiumHealthCheck.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Infrastructure.Options;

using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using PuppeteerSharp;

namespace FileConversionApi.Infrastructure.HealthChecks;

/// <summary>
/// Health check for Chromium browser availability.
/// </summary>
public class ChromiumHealthCheck : IHealthCheck
{
    private readonly PuppeteerSettings settings;
    private readonly HealthCheckSettings healthCheckSettings;
    private readonly ILogger<ChromiumHealthCheck> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChromiumHealthCheck"/> class.
    /// </summary>
    /// <param name="settings">The Puppeteer settings.</param>
    /// <param name="healthCheckSettings">The health check settings.</param>
    /// <param name="logger">The logger.</param>
    public ChromiumHealthCheck(
        IOptions<PuppeteerSettings> settings,
        IOptions<HealthCheckSettings> healthCheckSettings,
        ILogger<ChromiumHealthCheck> logger)
    {
        this.settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        this.healthCheckSettings = healthCheckSettings?.Value ?? throw new ArgumentNullException(nameof(healthCheckSettings));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            this.logger.LogDebug("Starting Chromium health check");

            // Check if browser executable exists
            var browserFetcher = new BrowserFetcher();
            var installedBrowser = browserFetcher.GetInstalledBrowsers().FirstOrDefault();

            if (installedBrowser is null)
            {
                // Try to download if not installed
                this.logger.LogDebug("No browser installed, attempting download");
                await browserFetcher.DownloadAsync().ConfigureAwait(false);
                installedBrowser = browserFetcher.GetInstalledBrowsers().FirstOrDefault();
            }

            if (installedBrowser is null && string.IsNullOrWhiteSpace(this.settings.ExecutablePath))
            {
                return HealthCheckResult.Unhealthy("Chromium browser not installed and no executable path configured");
            }

            // Perform a quick browser launch to verify it works
            var launchOptions = new LaunchOptions
            {
                Headless = true,
                Args =
                [
                    "--no-sandbox",
                    "--disable-setuid-sandbox",
                    "--disable-dev-shm-usage",
                    "--disable-gpu",
                    "--single-process",
                ],
                Timeout = this.healthCheckSettings.ChromiumTimeoutSeconds * 1000,
            };

            if (!string.IsNullOrWhiteSpace(this.settings.ExecutablePath))
            {
                launchOptions.ExecutablePath = this.settings.ExecutablePath;
            }

            using var browser = await Puppeteer.LaunchAsync(launchOptions).ConfigureAwait(false);
            await browser.CloseAsync().ConfigureAwait(false);

            this.logger.LogDebug("Chromium health check passed");

            return HealthCheckResult.Healthy("Chromium browser available");
        }
        catch (Exception ex)
        {
            this.logger.LogWarning(ex, "Chromium health check failed");
            return HealthCheckResult.Unhealthy("Chromium browser unavailable", ex);
        }
    }
}
