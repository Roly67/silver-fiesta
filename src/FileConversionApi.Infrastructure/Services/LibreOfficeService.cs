// <copyright file="LibreOfficeService.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using System.Diagnostics;

using FileConversionApi.Application.Interfaces;
using FileConversionApi.Domain.Primitives;
using FileConversionApi.Infrastructure.Options;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FileConversionApi.Infrastructure.Services;

/// <summary>
/// Service for converting documents using LibreOffice headless mode.
/// </summary>
public class LibreOfficeService : ILibreOfficeService
{
    private static readonly string[] SupportedFormats =
    [
        "docx", "doc", "xlsx", "xls", "pptx", "ppt", "odt", "ods", "odp", "rtf"
    ];

    private readonly LibreOfficeSettings settings;
    private readonly ILogger<LibreOfficeService> logger;
    private readonly SemaphoreSlim conversionLock = new(1, 1);

    /// <summary>
    /// Initializes a new instance of the <see cref="LibreOfficeService"/> class.
    /// </summary>
    /// <param name="settings">The LibreOffice settings.</param>
    /// <param name="logger">The logger.</param>
    public LibreOfficeService(
        IOptions<LibreOfficeSettings> settings,
        ILogger<LibreOfficeService> logger)
    {
        this.settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<Result<byte[]>> ConvertToPdfAsync(
        byte[] inputData,
        string inputFormat,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(inputData);
        ArgumentException.ThrowIfNullOrWhiteSpace(inputFormat);

        var normalizedFormat = inputFormat.TrimStart('.').ToLowerInvariant();
        if (!SupportedFormats.Contains(normalizedFormat))
        {
            return new Error(
                "LibreOffice.UnsupportedFormat",
                $"Format '{inputFormat}' is not supported for PDF conversion.");
        }

        var tempDir = this.settings.TempDirectory ?? Path.GetTempPath();
        var workDir = Path.Combine(tempDir, $"libreoffice_{Guid.NewGuid():N}");

        try
        {
            Directory.CreateDirectory(workDir);

            var inputFileName = $"input.{normalizedFormat}";
            var inputPath = Path.Combine(workDir, inputFileName);
            await File.WriteAllBytesAsync(inputPath, inputData, cancellationToken).ConfigureAwait(false);

            var executablePath = this.GetExecutablePath();

            // LibreOffice needs exclusive access - use lock to prevent conflicts
            await this.conversionLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var result = await this.RunLibreOfficeAsync(
                    executablePath,
                    workDir,
                    inputPath,
                    cancellationToken).ConfigureAwait(false);

                if (result.IsFailure)
                {
                    return result.Error;
                }

                // LibreOffice outputs PDF with same base name
                var outputPath = Path.Combine(workDir, "input.pdf");
                if (!File.Exists(outputPath))
                {
                    this.logger.LogError("LibreOffice did not produce expected output file at {OutputPath}", outputPath);
                    return new Error(
                        "LibreOffice.ConversionFailed",
                        "LibreOffice did not produce output file.");
                }

                var pdfData = await File.ReadAllBytesAsync(outputPath, cancellationToken).ConfigureAwait(false);

                this.logger.LogInformation(
                    "Successfully converted {Format} to PDF ({InputSize} bytes -> {OutputSize} bytes)",
                    normalizedFormat,
                    inputData.Length,
                    pdfData.Length);

                return Result<byte[]>.Success(pdfData);
            }
            finally
            {
                this.conversionLock.Release();
            }
        }
        catch (OperationCanceledException)
        {
            this.logger.LogWarning("LibreOffice conversion was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error during LibreOffice conversion");
            return new Error(
                "LibreOffice.ConversionFailed",
                $"LibreOffice conversion failed: {ex.Message}");
        }
        finally
        {
            // Cleanup temp directory
            try
            {
                if (Directory.Exists(workDir))
                {
                    Directory.Delete(workDir, recursive: true);
                }
            }
            catch (Exception ex)
            {
                this.logger.LogWarning(ex, "Failed to cleanup temp directory {WorkDir}", workDir);
            }
        }
    }

    /// <inheritdoc/>
    public IEnumerable<string> GetSupportedFormats() => SupportedFormats;

    /// <inheritdoc/>
    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var executablePath = this.GetExecutablePath();

            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = executablePath,
                    Arguments = "--version",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                },
            };

            process.Start();

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(5000);

            await process.WaitForExitAsync(cts.Token).ConfigureAwait(false);

            var output = await process.StandardOutput.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
            var isAvailable = process.ExitCode == 0 && output.Contains("LibreOffice", StringComparison.OrdinalIgnoreCase);

            this.logger.LogDebug("LibreOffice availability check: {IsAvailable}, output: {Output}", isAvailable, output.Trim());

            return isAvailable;
        }
        catch (Exception ex)
        {
            this.logger.LogWarning(ex, "LibreOffice availability check failed");
            return false;
        }
    }

    private string GetExecutablePath()
    {
        if (!string.IsNullOrWhiteSpace(this.settings.ExecutablePath))
        {
            return this.settings.ExecutablePath;
        }

        // Default paths based on OS
        if (OperatingSystem.IsWindows())
        {
            return @"C:\Program Files\LibreOffice\program\soffice.exe";
        }

        if (OperatingSystem.IsMacOS())
        {
            return "/Applications/LibreOffice.app/Contents/MacOS/soffice";
        }

        // Linux default
        return "soffice";
    }

    private async Task<Result> RunLibreOfficeAsync(
        string executablePath,
        string workDir,
        string inputPath,
        CancellationToken cancellationToken)
    {
        var arguments = $"--headless --convert-to pdf --outdir \"{workDir}\" \"{inputPath}\"";

        this.logger.LogDebug("Running LibreOffice: {Executable} {Arguments}", executablePath, arguments);

        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = executablePath,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = workDir,
            },
        };

        process.Start();

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(this.settings.TimeoutMs);

        try
        {
            await process.WaitForExitAsync(cts.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            try
            {
                process.Kill(entireProcessTree: true);
            }
            catch
            {
                // Ignore kill errors
            }

            return new Error(
                "LibreOffice.Timeout",
                "LibreOffice conversion timed out.");
        }

        var stdout = await process.StandardOutput.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
        var stderr = await process.StandardError.ReadToEndAsync(cancellationToken).ConfigureAwait(false);

        if (process.ExitCode != 0)
        {
            this.logger.LogError(
                "LibreOffice exited with code {ExitCode}. Stdout: {Stdout}, Stderr: {Stderr}",
                process.ExitCode,
                stdout,
                stderr);

            return new Error(
                "LibreOffice.ConversionFailed",
                $"LibreOffice conversion failed (exit code {process.ExitCode}).");
        }

        this.logger.LogDebug("LibreOffice completed successfully. Output: {Stdout}", stdout);

        return Result.Success();
    }
}
