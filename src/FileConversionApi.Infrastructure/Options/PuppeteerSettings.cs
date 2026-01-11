// <copyright file="PuppeteerSettings.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

namespace FileConversionApi.Infrastructure.Options;

/// <summary>
/// Configuration settings for Puppeteer.
/// </summary>
public class PuppeteerSettings
{
    /// <summary>
    /// The configuration section name.
    /// </summary>
    public const string SectionName = "PuppeteerSettings";

    /// <summary>
    /// Gets or sets the path to the Chromium executable.
    /// </summary>
    public string? ExecutablePath { get; set; }

    /// <summary>
    /// Gets or sets the default timeout in milliseconds.
    /// </summary>
    public int Timeout { get; set; } = 30000;
}
