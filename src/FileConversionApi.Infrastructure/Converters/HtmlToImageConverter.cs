// <copyright file="HtmlToImageConverter.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Application.DTOs;
using FileConversionApi.Application.Interfaces;
using FileConversionApi.Domain.Errors;
using FileConversionApi.Domain.Primitives;
using FileConversionApi.Infrastructure.Options;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using PuppeteerSharp;

namespace FileConversionApi.Infrastructure.Converters;

/// <summary>
/// Converter for HTML to image (PNG, JPEG, WebP) using PuppeteerSharp.
/// </summary>
public class HtmlToImageConverter : IFileConverter, IAsyncDisposable
{
    private readonly PuppeteerSettings settings;
    private readonly string targetFormat;
    private readonly ILogger<HtmlToImageConverter> logger;
    private IBrowser? browser;
    private bool disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="HtmlToImageConverter"/> class.
    /// </summary>
    /// <param name="settings">The Puppeteer settings.</param>
    /// <param name="targetFormat">The target image format (png, jpeg, webp).</param>
    /// <param name="logger">The logger.</param>
    public HtmlToImageConverter(
        IOptions<PuppeteerSettings> settings,
        string targetFormat,
        ILogger<HtmlToImageConverter> logger)
    {
        this.settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        this.targetFormat = targetFormat?.ToLowerInvariant() ?? throw new ArgumentNullException(nameof(targetFormat));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public string SourceFormat => "html";

    /// <inheritdoc/>
    public string TargetFormat => this.targetFormat;

    /// <inheritdoc/>
    public async Task<Result<byte[]>> ConvertAsync(
        Stream input,
        ConversionOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            this.logger.LogDebug("Starting HTML to {Format} conversion", this.targetFormat.ToUpperInvariant());

            await this.EnsureBrowserInitializedAsync().ConfigureAwait(false);

            using var reader = new StreamReader(input);
            var content = await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);

            await using var page = await this.browser!.NewPageAsync().ConfigureAwait(false);

            // Set viewport size
            await page.SetViewportAsync(new ViewPortOptions
            {
                Width = options.ViewportWidth,
                Height = options.ViewportHeight,
            }).ConfigureAwait(false);

            // Check if content is a URL or HTML
            if (Uri.TryCreate(content, UriKind.Absolute, out var uri) &&
                (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            {
                await page.GoToAsync(content, new NavigationOptions
                {
                    WaitUntil = options.WaitForJavaScript
                        ? [WaitUntilNavigation.Networkidle0]
                        : [WaitUntilNavigation.Load],
                    Timeout = options.JavaScriptTimeout,
                }).ConfigureAwait(false);
            }
            else
            {
                await page.SetContentAsync(content, new NavigationOptions
                {
                    WaitUntil = options.WaitForJavaScript
                        ? [WaitUntilNavigation.Networkidle0]
                        : [WaitUntilNavigation.Load],
                    Timeout = options.JavaScriptTimeout,
                }).ConfigureAwait(false);
            }

            var screenshotOptions = new ScreenshotOptions
            {
                Type = this.GetScreenshotType(),
                FullPage = options.FullPage,
            };

            // Set quality for JPEG and WebP (not applicable for PNG)
            if (this.targetFormat != "png" && options.ImageQuality.HasValue)
            {
                screenshotOptions.Quality = options.ImageQuality.Value;
            }

            var imageBytes = await page.ScreenshotDataAsync(screenshotOptions).ConfigureAwait(false);

            this.logger.LogDebug(
                "HTML to {Format} conversion completed, size: {Size} bytes",
                this.targetFormat.ToUpperInvariant(),
                imageBytes.Length);

            return imageBytes;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "HTML to {Format} conversion failed", this.targetFormat.ToUpperInvariant());
            return ConversionJobErrors.ConversionFailed(ex.Message);
        }
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (this.disposed)
        {
            return;
        }

        if (this.browser is not null)
        {
            await this.browser.CloseAsync().ConfigureAwait(false);
            this.browser.Dispose();
        }

        this.disposed = true;
        GC.SuppressFinalize(this);
    }

    private ScreenshotType GetScreenshotType()
    {
        return this.targetFormat switch
        {
            "jpeg" or "jpg" => ScreenshotType.Jpeg,
            "webp" => ScreenshotType.Webp,
            _ => ScreenshotType.Png,
        };
    }

    private async Task EnsureBrowserInitializedAsync()
    {
        if (this.browser is not null)
        {
            return;
        }

        this.logger.LogDebug("Initializing Puppeteer browser for screenshot");

        var browserFetcher = new BrowserFetcher();
        await browserFetcher.DownloadAsync().ConfigureAwait(false);

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
        };

        if (!string.IsNullOrWhiteSpace(this.settings.ExecutablePath))
        {
            launchOptions.ExecutablePath = this.settings.ExecutablePath;
        }

        this.browser = await Puppeteer.LaunchAsync(launchOptions).ConfigureAwait(false);
    }
}
