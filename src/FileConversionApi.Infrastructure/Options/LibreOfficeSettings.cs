// <copyright file="LibreOfficeSettings.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

namespace FileConversionApi.Infrastructure.Options;

/// <summary>
/// Configuration settings for LibreOffice.
/// </summary>
public class LibreOfficeSettings
{
    /// <summary>
    /// The configuration section name.
    /// </summary>
    public const string SectionName = "LibreOfficeSettings";

    /// <summary>
    /// Gets or sets the path to the LibreOffice executable (soffice).
    /// If not specified, uses the default system path.
    /// </summary>
    public string? ExecutablePath { get; set; }

    /// <summary>
    /// Gets or sets the timeout in milliseconds for conversion operations.
    /// </summary>
    public int TimeoutMs { get; set; } = 60000;

    /// <summary>
    /// Gets or sets the temporary directory for conversion operations.
    /// If not specified, uses the system temp directory.
    /// </summary>
    public string? TempDirectory { get; set; }
}
