// <copyright file="UrlValidationSettings.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

namespace FileConversionApi.Infrastructure.Options;

/// <summary>
/// Settings for URL validation in HTML to PDF conversion.
/// </summary>
public class UrlValidationSettings
{
    /// <summary>
    /// Gets or sets a value indicating whether URL validation is enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to use allowlist mode.
    /// When true, only URLs matching the allowlist are permitted.
    /// When false, URLs matching the blocklist are rejected.
    /// </summary>
    public bool UseAllowlist { get; set; }

    /// <summary>
    /// Gets or sets the list of allowed URL patterns (domains or regex patterns).
    /// Only used when UseAllowlist is true.
    /// Example: ["*.example.com", "github.com", "*.githubusercontent.com"].
    /// </summary>
    public List<string> Allowlist { get; set; } = [];

    /// <summary>
    /// Gets or sets the list of blocked URL patterns (domains or regex patterns).
    /// Only used when UseAllowlist is false.
    /// Example: ["localhost", "127.0.0.1", "*.local", "10.*", "192.168.*"].
    /// </summary>
    public List<string> Blocklist { get; set; } =
    [
        "localhost",
        "127.0.0.1",
        "::1",
        "0.0.0.0",
        "*.local",
        "*.localhost",
        "10.*",
        "172.16.*",
        "172.17.*",
        "172.18.*",
        "172.19.*",
        "172.20.*",
        "172.21.*",
        "172.22.*",
        "172.23.*",
        "172.24.*",
        "172.25.*",
        "172.26.*",
        "172.27.*",
        "172.28.*",
        "172.29.*",
        "172.30.*",
        "172.31.*",
        "192.168.*",
        "169.254.*",
        "metadata.google.internal",
        "169.254.169.254",
    ];

    /// <summary>
    /// Gets or sets a value indicating whether to block private/internal IP addresses.
    /// This provides additional protection even if not in blocklist.
    /// </summary>
    public bool BlockPrivateIpAddresses { get; set; } = true;
}
