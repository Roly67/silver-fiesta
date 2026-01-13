// <copyright file="HtmlToPdfConverter.cs" company="FileConversionApi">
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
using PuppeteerSharp.Media;

namespace FileConversionApi.Infrastructure.Converters;

/// <summary>
/// Converter for HTML to PDF using PuppeteerSharp.
/// </summary>
public class HtmlToPdfConverter : IFileConverter, IAsyncDisposable
{
    private readonly PuppeteerSettings settings;
    private readonly IPdfWatermarkService watermarkService;
    private readonly IPdfEncryptionService encryptionService;
    private readonly ILogger<HtmlToPdfConverter> logger;
    private IBrowser? browser;
    private bool disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="HtmlToPdfConverter"/> class.
    /// </summary>
    /// <param name="settings">The Puppeteer settings.</param>
    /// <param name="watermarkService">The PDF watermark service.</param>
    /// <param name="encryptionService">The PDF encryption service.</param>
    /// <param name="logger">The logger.</param>
    public HtmlToPdfConverter(
        IOptions<PuppeteerSettings> settings,
        IPdfWatermarkService watermarkService,
        IPdfEncryptionService encryptionService,
        ILogger<HtmlToPdfConverter> logger)
    {
        this.settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        this.watermarkService = watermarkService ?? throw new ArgumentNullException(nameof(watermarkService));
        this.encryptionService = encryptionService ?? throw new ArgumentNullException(nameof(encryptionService));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public string SourceFormat => "html";

    /// <inheritdoc/>
    public string TargetFormat => "pdf";

    /// <inheritdoc/>
    public async Task<Result<byte[]>> ConvertAsync(
        Stream input,
        ConversionOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            this.logger.LogDebug("Starting HTML to PDF conversion");

            await this.EnsureBrowserInitializedAsync().ConfigureAwait(false);

            using var reader = new StreamReader(input);
            var content = await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);

            await using var page = await this.browser!.NewPageAsync().ConfigureAwait(false);

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

            var pdfOptions = new PdfOptions
            {
                Format = GetPaperFormat(options.PageSize),
                Landscape = options.Landscape,
                MarginOptions = new MarginOptions
                {
                    Top = $"{options.MarginTop}px",
                    Bottom = $"{options.MarginBottom}px",
                    Left = $"{options.MarginLeft}px",
                    Right = $"{options.MarginRight}px",
                },
                PrintBackground = true,
            };

            if (!string.IsNullOrWhiteSpace(options.HeaderTemplate))
            {
                pdfOptions.HeaderTemplate = options.HeaderTemplate;
                pdfOptions.DisplayHeaderFooter = true;
            }

            if (!string.IsNullOrWhiteSpace(options.FooterTemplate))
            {
                pdfOptions.FooterTemplate = options.FooterTemplate;
                pdfOptions.DisplayHeaderFooter = true;
            }

            var pdfBytes = await page.PdfDataAsync(pdfOptions).ConfigureAwait(false);

            this.logger.LogDebug("HTML to PDF conversion completed, size: {Size} bytes", pdfBytes.Length);

            // Apply watermark if specified
            if (options.Watermark is not null && !string.IsNullOrWhiteSpace(options.Watermark.Text))
            {
                this.logger.LogDebug("Applying watermark to PDF");
                var watermarkResult = await this.watermarkService
                    .ApplyWatermarkAsync(pdfBytes, options.Watermark, cancellationToken)
                    .ConfigureAwait(false);

                if (watermarkResult.IsFailure)
                {
                    return watermarkResult.Error;
                }

                pdfBytes = watermarkResult.Value;
                this.logger.LogDebug("Watermark applied, new size: {Size} bytes", pdfBytes.Length);
            }

            // Apply password protection if specified
            if (options.PasswordProtection is not null && !string.IsNullOrWhiteSpace(options.PasswordProtection.UserPassword))
            {
                this.logger.LogDebug("Applying password protection to PDF");
                var encryptionResult = await this.encryptionService
                    .EncryptAsync(pdfBytes, options.PasswordProtection, cancellationToken)
                    .ConfigureAwait(false);

                if (encryptionResult.IsFailure)
                {
                    return encryptionResult.Error;
                }

                pdfBytes = encryptionResult.Value;
                this.logger.LogDebug("Password protection applied, new size: {Size} bytes", pdfBytes.Length);
            }

            return pdfBytes;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "HTML to PDF conversion failed");
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

    private static PaperFormat GetPaperFormat(string? pageSize)
    {
        return pageSize?.ToUpperInvariant() switch
        {
            "LETTER" => PaperFormat.Letter,
            "LEGAL" => PaperFormat.Legal,
            "TABLOID" => PaperFormat.Tabloid,
            "LEDGER" => PaperFormat.Ledger,
            "A3" => PaperFormat.A3,
            "A5" => PaperFormat.A5,
            _ => PaperFormat.A4,
        };
    }

    private async Task EnsureBrowserInitializedAsync()
    {
        if (this.browser is not null)
        {
            return;
        }

        this.logger.LogDebug("Initializing Puppeteer browser");

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
